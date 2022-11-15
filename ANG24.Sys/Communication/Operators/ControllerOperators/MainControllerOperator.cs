using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using ANG24.Sys.Application.Types.Enum;
using ANG24.Sys.Communication.Interfaces;
using ANG24.Sys.Infrastructure.Logging;
using Autofac;
using System.Text;

namespace ANG24.Sys.Communication.Operators.ControllerOperators;

public sealed class MainControllerOperator : StringSerialControllerOperator<MEAData>, IMainControllerOperator
{
    #region props
    public override event Action<MEAData> OnDataReceived;
    public event Action<bool, int> ModuleStateChanged;
    private readonly INotificationService notification;
    private readonly ILabConfigurationService config;
    private readonly ILifetimeScope container;
    private IMainPowerService mainPowerService;
    private MEAData lastData;
    private bool isRegulatorEnabled = true;
    private bool isControllerBlocked;
    private readonly double minKV;
    private readonly double maxKV;
    private readonly FazeType fazeType;
    private bool volChecked = false;
    private bool RegulatorDisabled = false;
    private bool SVI_Power_On;
    private readonly AutoResetEvent SimulationHandler = new(false);

    public int MaxCurrentRN { get; set; }
    public bool ModuleOnPower { get; set; } = false;
    public bool BlockStart { get; set; } = false;
    public string ActiveModule { get; set; } = string.Empty;
    public LabModule SelectedModule { get; private set; }
    public LabModuleStates LabModuleState { get; private set; }
    #endregion
    public MainControllerOperator(INotificationService notification,
                                  ILabConfigurationService config,
                                  ILifetimeScope container)
    {
        Name = LabController.MainController.ToString();
        this.notification = notification;
        this.config = config;
        this.container = container;
        MaxCurrentRN = (int)config["MaxCurrentRN"];
        minKV = (double)config["MinKV"];
        maxKV = (double)config["MaxKV"];
        fazeType = (FazeType)config["FazeType"];
        ModuleStateChanged += MainControllerOperator_ModuleStateChanged;
    }
    public override bool Connect(int AttemptCount = 5)
    {
        if (Connected) return true;
        try
        {
            bool isSimAccepted = false;
            mainPowerService = container.Resolve<IMainPowerService>();
            while (!isSimAccepted) // Почему так много бесконечных циклов?
                if (base.Connect(AttemptCount))
                {
                    LabState.IsSimulationOn = false;
                    return true;
                }
                else
                {
                    notification.SendNotificationOKCancel("Не удалось установить соединение с контроллером. \n" +
                        "Перейти в режим симуляции?\n\n" +
                        "Внимание! После выбора режима симуляции для возобновления работы программы с контроллером необходимо перезагрузить ее.",
                    () => // Если согласился
                    {
                        LabState.IsSimulationOn = true;
                        config["DebugMode"] = true;
                        isSimAccepted = true;
                        Thread.Sleep(100); // по не выясненной причине, isSimAccepted не сразу присваивается в другом потоке 
                        SimulationHandler.Set();
                    },
                    () =>
                    SimulationHandler.Set()); // Если отказался, попробовать подключится снова
                    if ((bool)config["DebugMode"]) break;
                    SimulationHandler.WaitOne();
                }
        }
        catch (Exception) { }
        return false;
    }
    public void RunLab() => mainPowerService.Run();
    public void PowerOff()
    {
        var StopState = TargetState.Unchecked;
        if (LabModuleState != LabModuleStates.Stopping)
        {
            SendCommand("POWERDOWN");
            LabModuleState = LabModuleStates.Stopping;
            OnDataReceived += StopPowerDataReceived;
            var result = Task<bool>.Factory.StartNew(() => StopPower(ref StopState)).Result;
            OnDataReceived -= StopPowerDataReceived;
            if (result)
            {
                ModuleStateChanged?.Invoke(false, 0);
                ModuleOnPower = false;
                ControllerLogger.WriteString("Notify: Module Stopped");
            }
            else ModuleStateChanged?.Invoke(false, -1);
            LabModuleState = LabModuleStates.Stop;
            RegulatorDisable();
            SVI_Power_On = false;
        }
        bool StopPower(ref TargetState StopState)
        {
            var s = Stopwatch.StartNew();
            while (s.ElapsedMilliseconds < 5000) // AAAAA
            {
                try
                {
                    if (StopState != TargetState.Unchecked)
                    {
                        if (StopState == TargetState.Ready)
                        {
                            Debug.WriteLine("Module Stopped successful");
                            return true;
                        }
                        if (StopState == TargetState.NotReady)
                        {
                            Debug.WriteLine("Module stopped failed");
                            return false;
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return true;
        }
        void StopPowerDataReceived(MEAData model)
        {
            var mes = model.Message.Split(' ');
            if (mes.Length == 4)
            {
                if (mes[0] == "Power" && mes[1] == "off")
                    switch (mes[3])
                    {
                        case "OK":
                            StopState = TargetState.Ready;
                            break;
                        case "fault":
                            StopState = TargetState.NotReady;
                            break;
                    }
            }
        }
    }
    public void PowerOn(IEnumerable<PowerTargets> powerTargets)
    {
        if (!isControllerBlocked)
            if ((FazeType)config["FazeType"] == FazeType.ThreeFaze && !lastData.MKZState.MKZError
             || lastData.Module != LabModule.GP500 && lastData.MKZState.Ground && !lastData.MKZState.MKZError
             || !lastData.MKZState.MKZError)
                SuccessPowerOn(powerTargets).Wait();



            else
                FailPowerOn();

        async Task SuccessPowerOn(IEnumerable<PowerTargets> powerTargets)
        {
            var startTargets = new Dictionary<PowerTargets, TargetState>();
            foreach (var item in powerTargets) startTargets.Add(item, TargetState.Unchecked);
            if (!powerTargets.Contains(PowerTargets.ModulePower))
                startTargets.Add(PowerTargets.ModulePower, TargetState.Unchecked);

            SendCommand("POWERUP");
            LabModuleState = LabModuleStates.Starting;

            OnDataReceived += StartPowerDataReceived;
            var result = await Task<bool>.Factory.StartNew(() => StartPower());
            OnDataReceived -= StartPowerDataReceived;

            if (result)
            {
                ModuleStateChanged?.Invoke(true, 0);
                ModuleOnPower = true;
                LabModuleState = LabModuleStates.Running;
                ControllerLogger.WriteString("Notify: Module Started");
            }
            else
            {
                ModuleStateChanged?.Invoke(true, -1);
                LabModuleState = LabModuleStates.Stop;
            }
            new Timer(state =>
            {
                RegulatorEnable();
            }).Change(500, Timeout.Infinite);
            new Timer(state =>
            {
                SVI_Power_On = true;
            }).Change(5000, Timeout.Infinite);

            bool StartPower()
            {
                var timer = Stopwatch.StartNew();
                while (true) // Почему мир так жесток?!
                {
                    if (timer.ElapsedMilliseconds >= 7000) // 7 секунд
                    {
                        Debug.WriteLine("Module not started: Timeout");
                        return false;
                    }
                    if (startTargets.ContainsValue(TargetState.NotReady))
                    {
                        Debug.WriteLine("Module not started");
                        return false;
                    }
                    else
                    {
                        if (!startTargets.ContainsValue(TargetState.Unchecked) && !startTargets.ContainsValue(TargetState.NotReady))
                        {
                            Debug.WriteLine("Module started successful");
                            return true;
                        }
                        if (startTargets.ContainsKey(PowerTargets.ModulePower)
                            && startTargets[PowerTargets.ModulePower] == TargetState.Ready)
                        {
                            Debug.WriteLine("Module started successful, but skipped other power targets");
                            return true;
                        }
                    }
                    Thread.Sleep(50);
                }
            }
            void StartPowerDataReceived(MEAData model)
            {
                var splitMessage = model.Message.Split(' ');

                if (splitMessage.Length == 4)
                {
                    var target = PowerTargets.Undefined;
                    switch (splitMessage[1])
                    {
                        case "shorter":
                            target = splitMessage[0] switch
                            {
                                "MVK" => PowerTargets.MVK,
                                "SVI" => PowerTargets.SVI,
                                "MSK" => PowerTargets.MSK,
                                _ => PowerTargets.Undefined
                            };

                            if (startTargets.ContainsKey(target))
                            {
                                switch (splitMessage[3])
                                {
                                    case "up":
                                        startTargets[target] = TargetState.Ready;
                                        break;
                                    case "down":
                                        var ruName = target switch
                                        {// Отправка сообщения пользователю в случае проблемы
                                            PowerTargets.MVK => "МВК",
                                            PowerTargets.MSK => "МСК",
                                            PowerTargets.SVI => "СВИ",
                                            _ => string.Empty,
                                        };
                                        notification.SendNotificationOK(target + " магнит не поднялся", null);
                                        startTargets[target] = TargetState.NotReady;
                                        break;
                                }
                            }
                            break;
                        case "power":
                            if (splitMessage[3].Contains("ready"))
                                startTargets[PowerTargets.ModulePower] = TargetState.Ready;
                            break;
                        case "mode":
                            if (splitMessage[3].Contains("incorrect"))
                            {
                                notification.SendNotificationOK("Положение главного переключателя неверно", null);
                                startTargets[PowerTargets.ModulePower] = TargetState.NotReady;
                            }
                            break;
                    }
                }
                if (splitMessage.Length == 3)
                    if (splitMessage[1].Contains("power") && splitMessage[2].Contains("error"))
                        startTargets[PowerTargets.ModulePower] = TargetState.NotReady;
            }
        }
        void FailPowerOn()
        {
            var stringResMKZ = new StringBuilder();
            if (!lastData.MKZState.DoorLeft) stringResMKZ.AppendLine(" -Открыта левая дверь");
            if (!lastData.MKZState.DoorRight) stringResMKZ.AppendLine(" -Открыта правая дверь");
            if (!lastData.MKZState.DangerousPotencial) stringResMKZ.AppendLine(" -Обнаружен опасный потенциал");
            if (!lastData.MKZState.Ground) stringResMKZ.AppendLine(" -Нет заземления");
            if (!lastData.MKZState.SafeKey) stringResMKZ.AppendLine(" -Ключ безопасности в положении \"0\"");
            if (!lastData.MKZState.Stop) stringResMKZ.AppendLine(" -Авария");
            //notification.ShowDialog(string.Format("Предупреждения системы безопасности:\n{0}", stringResMKZ.ToString()));
            new Timer(state =>
            {
                ModuleStateChanged?.Invoke(true, -1);
            }).Change(1000, Timeout.Infinite);
        }
    }
    public void RegulatorDisable()
    {
        if (isRegulatorEnabled)
        {
            SendCommand("VOLTAGE_REGULATOR_DISABLE");
            ControllerLogger.WriteString("Notify: Voltage Regulator Disabled");
            isRegulatorEnabled = false;
        }
    }
    public void RegulatorEnable()
    {
        if (!isRegulatorEnabled)
        {
            SendCommand("VOLTAGE_REGULATOR_ENABLE");
            ControllerLogger.WriteString("Notify: Voltage Regulator Enabled");
            isRegulatorEnabled = true;
        }
    }
    public void VoltageDown() => SendCommand("VOLT_STEP_DOWN");
    public void VoltageUp() => SendCommand("VOLT_STEP_UP");
    public void SetStep(int step) => SendCommand("BURN:STEP," + step + ";");
    public void SetPower(BurnPower power)
    {
        if (Enum.IsDefined(power))
        {
            var p = power switch
            {
                BurnPower.Power50 => 50,
                BurnPower.Power100 => 100,
                _ => 0,
            };
            SendCommand("BURN:POWER," + p + ";");
        }
    }
    public void SetModule(LabModule module) => SetModuleLogic(module);
    public void SetModuleAndDontOffCurrent(LabModule module) => SetModuleLogic(module, true);
    public void ResetModule()
    {
        if (ModuleOnPower) PowerOff();
        if (ActiveModule != string.Empty)
            SendCommand(string.Format("{0}:EmergencyStop;", ActiveModule));
        ActiveModule = string.Empty;

        SelectedModule = LabModule.Main;
        //powerProtection.SetDefault();
        MaxCurrentRN = (int)config.Configuration["MaxCurrentRN"];
    }
    public void GetTrial() => SendCommand("ReadTrial");
    public void EnterKeys(int key1, int key2, int key3, int key4)
    {
        SendCommand("EnterKey1," + key1 + ";", true);
        SendCommand("EnterKey2," + key2 + ";", true);
        SendCommand("EnterKey3," + key3 + ";", true);
        SendCommand("EnterKey4," + key4 + ";", true);
    }
    public FazeType GetFazeType()
    {
        FazeType returnedFazeType = FazeType.NoFaze;
        var port = Controller as ISerialPortController;
        if (Connected)
        {
            int counter = 10;
            while (counter != 0)
            {
                try
                {
                    Controller.WriteCommandPriority(new ControllerCommand()
                    {
                        Message = "#ReadLabPh"
                    });
                    var request = port.ApplyCommand("#ReadLabPh").Replace('\r', ' ').Replace('\n', ' ').Trim();
                    if (request.Contains("Lab Phase"))
                    {
                        if (int.TryParse(request.Split(' ')[3], out int intFazeType)) returnedFazeType = (FazeType)intFazeType;
                    }
                }
                catch (Exception)
                {
                    counter--;
                }
            }
        }
        return returnedFazeType;
    }
    public void SendCommand(string command, bool NoSparp = false) => SetCommandPriority(NoSparp == true ? $"{command}" : $"#{command}");
    private void SetModuleLogic(LabModule module, bool DontOff = false)
    {
        if (!DontOff && ModuleOnPower)
            PowerOff();
        if (module != lastData?.Module)
            switch (module)
            {
                case LabModule.Burn:
                    ActiveModule = "BURN";
                    SendCommand("BURN:START,MANUAL;");
                    break;
                case LabModule.HVBurn:
                    ActiveModule = "HVBURN";
                    SendCommand("HVBURN:START,MANUAL;");
                    break;
                case LabModule.GVI:
                    ActiveModule = "HVPULSE";
                    SendCommand("HVPULSE:START,MANUAL;");
                    break;
                case LabModule.JoinBurn:
                    ActiveModule = "JOINTBURN";
                    SendCommand("JOINTBURN:START,MANUAL;");
                    break;
                case LabModule.HVMAC:
                case LabModule.HVMDCHi:
                    ActiveModule = "HVM";
                    SendCommand("HVM:AC,MANUAL,START");
                    break;
                case LabModule.Tangent2000:
                    ActiveModule = "BRIDGE"; //
                    SendCommand("BRIDGE:START,MANUAL");
                    break;
                case LabModule.Bridge: //
                    ActiveModule = "BRIDGE";
                    SendCommand("BRIDGE:START,MANUAL;");
                    break;
                case LabModule.GP500: //
                    ActiveModule = "GP500";
                    SendCommand("GP500:START,MANUAL;");
                    break;
                case LabModule.HVMDC: //
                    ActiveModule = "HVM";
                    SendCommand("HVM:DC,MANUAL,START");
                    break;
                case LabModule.LVMeas: //
                    ActiveModule = "LVM";
                    SendCommand("LVM:START,MANUAL;");
                    break;
                case LabModule.SA540_1: //
                    ActiveModule = "SA540_1";
                    SendCommand("SA540_1:START");
                    break;
                case LabModule.SA540_3: //
                    SendCommand("SA540_3:START");
                    break;
                case LabModule.SA640: //
                    SendCommand("SA640:START");
                    break;
                case LabModule.VLF: //
                    SendCommand("VLF:START");
                    break;
                case LabModule.Meas:
                case LabModule.Reflect: //
                    ActiveModule = "MEASURE";
                    SendCommand("MEASURE:START,MANUAL;");
                    break;
            }

        SelectedModule = module;
    }
    private async void MainControllerOperator_ModuleStateChanged(bool isPowerOn, int code)
    {
        if (code == 0)
            switch (LabState.CurrentModule)
            {
                case LabModule.GVI:
                    if (isPowerOn)
                    {
                        notification.SendNotification("Включение ИДМ модуля", 8000);
                        await Task.Delay(1000);
                        GVI_SetDelay(6);
                        await Task.Delay(1000);

                        SendCommand("HVPULSE:ICE;");
                        await Task.Delay(3000);
                        SendCommand("HVPULSE:ARC;");
                        await Task.Delay(3000);
                        SendCommand("HVPULSE:ICE;");
                        //принести логику включения IDM из рефлектометра (start)
                    }
                    break;
                case LabModule.HVMAC:
                    if (!isPowerOn)
                    {
                        await Task.Delay(300);
                        ResetModule();
                        await Task.Delay(50);
                        while (LabState.CurrentModule != LabModule.Main)
                            await Task.Delay(50);
                        this.SetModule(LabModule.HVMAC);
                    }
                    break;
                case LabModule.HVMDC:
                    if (!isPowerOn)
                    {
                        await Task.Delay(300);
                        ResetModule();
                        await Task.Delay(50);
                        while (LabState.CurrentModule != LabModule.Main)
                            await Task.Delay(50);
                        this.SetModule(LabModule.HVMDC);
                    }
                    break;
                case LabModule.GP500:
                    break;
                case LabModule.HVBurn:
                    break;
                case LabModule.Meas:
                    break;
            }
    }
    protected override void AddDataInfo(ref MEAData data)
    {
        data.OptionalInfo = $"{minKV} {maxKV} {SVI_Power_On} {fazeType}";
    }
    protected override void CommandBroker(MEAData data)
    {
        //Protect 25A power
        if (LabModuleState == LabModuleStates.Running)
        {
            if (lastData.OutRNCurrent > MaxCurrentRN) PowerOff();
            if (ModuleOnPower)
                if (data.Message.Contains("Power off is"))
                {
                    ModuleStateChanged?.Invoke(false, 0);
                    ModuleOnPower = false;
                }
        }
        if (data.Module != LabModule.NoMode) lastData = data;

        LabState.CurrentModule = data.Module;

        if (data.Message.Contains("Trial version"))
        {
            var mes = data.Message.Split('.')[1].Trim().Split(' ')[0];
            int.TryParse(mes, out int ExternalCount);
            if (ExternalCount == 0) isControllerBlocked = true;
            notification.SendNotificationOK($"Благодарим Вас за выбор электротехнической лаборатории АНГСТРЕМ. " +
                $"\n Зарегистрируйте продукт сегодня, чтобы пользоваться нашей персональной поддержкой 24 / 7. " +
                $"\n (осталось включений: {ExternalCount})", null);
            return;
        }
        switch (data.Module)
        {
            case LabModule.GVI:
                OnDataRecievenInGVIMode(data.Step, data.OutRNVoltage);
                break;
            case LabModule.HVBurn:
                OnDataRecievenInHVBurnMode(data.Voltage, data.StableVoltage, data.VoltageType);
                break;
        }
        OnDataReceived?.Invoke(data);


        void OnDataRecievenInHVBurnMode(double voltage, double stableVoltage, int voltageType)
        {
            if (ModuleOnPower)
                if ((voltage > 4000 || stableVoltage > 4000)
                    && !volChecked
                    && (voltageType == 1 || voltageType == 3))
                {
                    notification.SendNotificationOK("Проверьте выпрямитель на наличие шунта. Работа остановлена", null);
                    PowerOff();
                    volChecked = true;
                }
                else { }
            else
                volChecked = false;
        }
    }
    #region GVI Control
    private void OnDataRecievenInGVIMode(int step, double RN_Vol)
    {
        var max_RN_vol = step switch
        {
            1 => 200,
            2 => 200,
            3 => 175,
            _ => 215
        };
        if (step != 0)
            if (!RegulatorDisabled)//разрешение инертности хода регулятора
                if (RN_Vol > max_RN_vol)
                {
                    RegulatorDisable();
                    RegulatorDisabled = true;
                }
                else//чтобы избегать "дребезга" флага отключения регулятора
                if (RN_Vol < max_RN_vol - 10)
                {
                    RegulatorEnable();
                    RegulatorDisabled = false;
                }
    }
    public void GVI_OneFire() => SendCommand("HVPULSE:ONEFIRE;");
    public void GVI_LineFire(int count) => SendCommand(string.Format("HVPULSE:LINEFIRE,{0};", count));
    public void GVI_FireStop() => SendCommand("HVPULSE:LINEEmergencyStop;");
    public void GVI_SetDelay(int delay) => SendCommand(string.Format("HVPULSE:PAUSE,{0};", delay));
    #endregion
    #region CalibControl
    public void SetDateCalib(int year, int mouth, int day)
    {
        string m, d;
        if (mouth < 10) m = "0" + mouth.ToString();
        else m = mouth.ToString();
        if (day < 10) d = "0" + day.ToString();
        else d = day.ToString();
        SendCommand($"$VOL-SYS:SETCALIB:{year}{m}{d};", true);
    }
    public void GetDateCalib() => SendCommand("$VOL-SYS:GETCALIB;", true);//
    public void GetSerialNumCalib() => SendCommand("$VOL-SYS:GETSN;", true);
    public void SetSerialNumCalib(int serial) => SendCommand($"$VOL-SYS:SETSN:{serial};", true);
    public void GetLevelZeroPointCalib() => SendCommand("$VOL-ADJ:GETZP;", true);
    public void SetLevelZeroPointCalib(int delta) => SendCommand($"$VOL-ADJ:SETZP:{2048 + delta};", true);
    public void AddPointDCCalib(int serial, int voltage) => SendCommand($"$VOL-ADJ:ADDDCPTR:{serial},{voltage};", true);
    public void AddPointACCalib(int pointNum, int voltage) => SendCommand($"$VOL-ADJ:ADDACPTR:{pointNum},{voltage};", true);
    public void DeleteAllPointsCalib(int serial) => SendCommand($"$VOL-ADJ:ERASE:{serial};", true);
    public void GetPointCountCalib(int serial) => SendCommand($"$VOL-ADJ:GETPTRS:{serial};", true);
    public void RemovePointDCCalib(int serial, int index) => SendCommand($"$VOL-ADJ:DELSELPTRDC:{serial},{index};", true);
    public void RemovePointACCalib(int serial, int index) => SendCommand($"$VOL-ADJ:DELSELPTRAC:{serial},{index};", true);
    public void EditPointDCCalib(int serial, int index, int voltage) => SendCommand($"$VOL-ADJ:EDITDC:{serial},{index},{voltage};", true);
    public void EditPointACCalib(int serial, int index, int voltage) => SendCommand($"$VOL-ADJ:EDITAC:{serial},{index},{voltage};", true);
    public void GetKoefDCCalib(int serial, int index) => SendCommand($"$VOL-ADJ:GETSELPTRDC:{serial},{index};", true);
    public void GetKoefACCalib(int serial, int index) => SendCommand($"$VOL-ADJ:GETSELPTRAC:{serial},{index};", true);
    public void GetFirmwareRev() => SendCommand($"$VOL-SYS:REV;", true);
    #endregion
    #region AutoBurn
    private IAutoburner burner;
    public void SetAutoBurning(bool workingMode)
    {
        if (workingMode && burner is null)
            burner = container.Resolve<IAutoburner>();
        burner?.SetAutoBurning(workingMode);
    }
    public void SetStepBackBlocked(bool blocked) => burner?.SetStepBackBlocked(blocked);
    public async Task<bool> SetNextStep() => await burner.SetNextStep();
    public async Task<bool> SetPreviousStep() => await burner.SetPreviousStep();
    #endregion
}