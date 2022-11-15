using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using ANG24.Sys.Infrastructure.Logging;
using Autofac;
using System.Text;

namespace ANG24.Sys.Communication.Helpers
{
    public sealed class MainControllerSimulator : IMainControllerOperator
    {
        private static double outRNVoltage;
        private MEAData lastData;
        private bool hvTrigger = false;
        private short voltageHoldCount = 1; // удержание напряжения
        private readonly short defaultCounter = 45; // 5s
        private readonly ILabConfigurationService config;
        private readonly ILifetimeScope container;
        private readonly IMainPowerService mainPowerService;
        private readonly INotificationService notification;
        private readonly double minKV;
        private readonly double maxKV;
        private static bool successBurning;
        private bool plus = true;
        private bool isRegulatorEnabled = true;

        public event Action<bool, int> ModuleStateChanged;
        public event Action<MEAData> OnDataReceived;
        public event Action<IControllerOperatorData> OnData;
        #region Fields
        public LabModuleStates LabModuleState { get; private set; }
        public static LabModule Module { get; set; } = LabModule.Main;
        public static int Step_Faze { get; set; } = 1;
        public WorkMode WorkMode { get; set; } = WorkMode.Manual;
        public static int Step { get; set; }
        public static double Voltage { get; set; }
        public static double Current { get; set; }
        public static GVIFireMode FireMode { get; set; }
        public static int BatteryVoltage { get; set; } = 4;
        //GVI Error Message
        public static bool GVI_EM_ChargerSet { get; set; }
        public static bool GVI_EM_ChargerReset { get; set; }
        public static bool GVI_EM_MeasureSet { get; set; }
        public static bool GVI_EM_MeasureReset { get; set; }
        public static bool GVI_EM_LeavingCapacitor { get; set; }
        public static bool GVI_EM_ShutingCapacitor { get; set; }
        public static bool GVI_EM_LeavingCable { get; set; }
        public static bool GVI_EM_ShutingCable { get; set; }
        public static bool GVI_EM_LeavingCap2Cab { get; set; }
        public static bool GVI_EM_ShutingCap2Cab { get; set; }
        public static bool GVI_EM_HiTemp { get; set; }
        public static bool GVI_EM_HiTemp2 { get; set; }
        public static bool GVI_EM_OverCap { get; set; }
        public static bool GVI_EM_OverReg { get; set; }
        public static bool GVI_EM_CriticalErrorCapShoot { get; set; }
        public static bool GVI_EM_CriticalErrorCabShoot { get; set; }
        public static double TempTransformator { get; set; }
        public static double PowerOutValue { get; set; }
        public static int CurrentProtect { get; set; }
        public static BurnPower PowerBurn { get; set; }
        public static int FireDelay { get; set; }
        public static int CountFire { get; set; }
        //Power info message
        public static bool PI_RNParking { get; set; }
        public static bool PI_RNManualEnable { get; set; }
        public static bool PI_HVMEnable { get; set; }
        public static bool PI_GVIEnable { get; set; }
        public static bool PI_BurnEnable { get; set; }
        public static bool PI_GP500Enable { get; set; }
        public static bool PI_LVMEnable { get; set; }
        public static bool PI_MeasEnable { get; set; }
        public static bool PI_JoinBurnEnable { get; set; }
        /// <summary>
        /// Рабочая земля от MKZ испытаний
        /// </summary>
        public static bool PI_KM1_MKZEnable { get; set; }
        public static bool PI_IDMEnable { get; set; }
        public static bool PI_ProtectedDrosselEnable { get; set; }
        /// <summary>
        /// Рабочая земля от MKZ ГП-500
        /// </summary>
        public static bool PI_KM3_MKZEnable { get; set; }
        public static bool PI_MVKUp { get; set; }
        public static bool PI_MSKEnable { get; set; }
        public static bool PI_SVIPowerEnable { get; set; }
        public static bool PI_SA640Enable { get; set; }
        public static bool PI_SA540Enable { get; set; }
        public static bool PI_Tangent2000Enable { get; set; }
        public static bool PI_VLFEnable { get; set; }
        /// <summary>
        /// Указывает на что, что включена защита на 100(200)mA, если показывает false -- указывает на то, что включена защита 20mA
        /// </summary>
        public static bool PI_CurrentProtection100Enable { get; set; }
        public static bool PI_VREnable { get; set; }
        public static bool PI_BridgeEnable { get; set; }
        public static bool PI_VoltageUpFlag { get; set; }
        public static bool PI_VoltageDownFlag { get; set; }
        public static double StableVoltage { get; set; }
        public static double StableCurrent { get; set; }
        public static int VoltageType { get; set; }
        public static bool ConnectionEnable { get; set; } = true;
        public static string ErrorCode1 { get; set; }
        public static string ErrorCode2 { get; set; }
        public static string ErrorCode3 { get; set; }
        public static string ErrorCode4 { get; set; }
        public static bool MKZ_MKZError { get; set; }
        public static bool MKZ_DoorLeft { get; set; }
        public static bool MKZ_DoorRight { get; set; }
        public static bool MKZ_DangerousPotencial { get; set; }
        public static bool MKZ_Ground { get; set; }
        public static bool MKZ_SafeKey { get; set; }
        public static bool MKZ_Stop { get; set; }
        public static bool IsAutoVoltage { get; set; }

        public static double OutRNVoltage
        {
            get => outRNVoltage;
            set
            {
                outRNVoltage = value;
                if (value > 2)
                {
                    switch (Module)
                    {
                        case LabModule.HVMAC:
                        case LabModule.HVMDC:
                        case LabModule.HVMDCHi:
                        case LabModule.JoinBurn:
                            Voltage = Random.Shared.Next((int)((value - 2) * 500), (int)((value + 2) * 500));
                            StableVoltage = Random.Shared.Next((int)((value - 2) * 500), (int)((value + 2) * 500));
                            Current = Random.Shared.Next(0, 290);
                            break;
                        case LabModule.HVBurn:
                            Voltage = Random.Shared.Next((int)((value - 2) * 500), (int)((value + 2) * 500));
                            StableVoltage = Random.Shared.Next((int)((value - 2) * 500), (int)((value + 2) * 500));
                            Current = Random.Shared.Next(199999);
                            break;
                        case LabModule.Burn:
                            Voltage = value;
                            if (successBurning)
                                Current = Random.Shared.Next((int)(Voltage * 0.9), (int)(Voltage * 1.2));
                            else
                                Current = Random.Shared.Next(0, (int)(Voltage * 0.25));
                            break;
                        case LabModule.GVI:
                            Voltage = Random.Shared.Next((int)((value - 2) * 40), (int)((value + 2) * 40));
                            Step = 1;
                            break;
                        default: Voltage = value * 420; break;
                    }
                    //StableVoltage = Random.Shared.Next((int)((value - 2) * 500), (int)((value + 2) * 500)); // для образования постоянки
                }
            }
        }
        public static double OutRNCurrent { get; set; }
        public string Message { get; set; } = string.Empty;
        public static HVSwitchState HVSwitch1 { get; set; }
        public static HVSwitchState HVSwitch2 { get; set; }
        public static HVSwitchState HVSwitch3 { get; set; }

        public string Name => "MeaSimulation";
        public bool Connected => true;
        public IMethodExecuter CurrentProcess => null;
        #endregion
        public MainControllerSimulator(ILabConfigurationService config,
            IDebugMenu debug,
            ILifetimeScope container,
            IMainPowerService mainPowerService,
            INotificationService notification)
        {
            this.config = config;
            this.container = container;
            this.mainPowerService = mainPowerService;
            this.notification = notification;
            minKV = (double)config["MinKV"];
            maxKV = (double)config["MaxKV"];
            debug.OnFlagChanged += Debug_OnFlagChanged;
            OnDataReceived += (data) => OnData?.Invoke(data);
            Task.Factory.StartNew(() =>
            {
                while (true) // Симуляция будет длится до перезапуска сервера 
                    try //добавить CancellationToken
                    {
                        PulseData();
                        Thread.Sleep(200); // до 5 вызовов в секунду
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
            }, creationOptions: TaskCreationOptions.LongRunning);
        }

        private void Debug_OnFlagChanged(string flag, bool state)
        {
            if (flag != "all")
                GetType().GetMethod($"set_{flag}")?.Invoke(this, new object[] { state });
            else
            {
                PI_RNParking = state;
                PI_RNManualEnable = state;
                PI_HVMEnable = state;
                PI_GVIEnable = state;
                PI_BurnEnable = state;
                PI_GP500Enable = state;
                PI_LVMEnable = state;
                PI_MeasEnable = state;
                PI_JoinBurnEnable = state;
                PI_VREnable = state;
                PI_KM1_MKZEnable = state;
                PI_KM3_MKZEnable = state;
                PI_IDMEnable = state;
                PI_ProtectedDrosselEnable = state;
                PI_MVKUp = state;
                PI_MSKEnable = state;
                PI_SVIPowerEnable = state;
                PI_BridgeEnable = state;
                PI_SA540Enable = state;
                PI_SA640Enable = state;
                PI_CurrentProtection100Enable = state;
                PI_VLFEnable = state;
                PI_Tangent2000Enable = state;

                MKZ_DoorLeft = state;
                MKZ_DoorRight = state;
                MKZ_SafeKey = state;
                MKZ_Stop = state;
                MKZ_DangerousPotencial = state;
                MKZ_Ground = state;
            }
        }

        public MEAData PulseData()
        {
            var data = new MEAData
            {
                Module = Module,
                Step_Faze = Step_Faze,
                WorkMode = WorkMode,
                Step = Step,
                Voltage = Voltage,
                Current = Current,
                FireMode = FireMode,
                BatteryVoltage = BatteryVoltage,
                GVIErrorMessage = new GVIErrorInfo
                {
                    ChargerSet = GVI_EM_ChargerSet,
                    ChargerReset = GVI_EM_ChargerReset,
                    MeasureSet = GVI_EM_MeasureSet,
                    MeasureReset = GVI_EM_MeasureReset,
                    LeavingCapacitor = GVI_EM_LeavingCapacitor,
                    ShutingCapacitor = GVI_EM_ShutingCapacitor,
                    LeavingCable = GVI_EM_LeavingCable,
                    ShutingCable = GVI_EM_ShutingCable,
                    LeavingCap2Cab = GVI_EM_LeavingCap2Cab,
                    ShutingCap2Cab = GVI_EM_ShutingCap2Cab,
                    HiTemp = GVI_EM_HiTemp,
                    HiTemp2 = GVI_EM_HiTemp2,
                    OverCap = GVI_EM_OverCap,
                    OverReg = GVI_EM_OverReg,
                    CriticalErrorCapShoot = GVI_EM_CriticalErrorCapShoot,
                    CriticalErrorCabShoot = GVI_EM_CriticalErrorCabShoot
                },
                TempTransformator = TempTransformator,
                PowerOutValue = PowerOutValue,
                CurrentProtect = CurrentProtect,
                PowerBurn = PowerBurn,
                FireDelay = FireDelay,
                CountFire = CountFire,
                PowerInfoMessage = new PowerInfo
                {
                    RNParking = PI_RNParking,
                    RNManualEnable = PI_RNManualEnable,
                    HVMEnable = PI_HVMEnable,
                    GVIEnable = PI_GVIEnable,
                    BurnEnable = PI_BurnEnable,
                    GP500Enable = PI_GP500Enable,
                    LVMEnable = PI_LVMEnable,
                    MeasEnable = PI_MeasEnable,
                    JoinBurnEnable = PI_JoinBurnEnable,
                    KM1_MKZEnable = PI_KM1_MKZEnable,
                    KM3_MKZEnable = PI_KM3_MKZEnable,
                    IDMEnable = PI_IDMEnable,
                    ProtectedDrosselEnable = PI_ProtectedDrosselEnable,
                    MVKUp = PI_MVKUp,
                    MSKEnable = PI_MSKEnable,
                    SVIPowerEnable = PI_SVIPowerEnable,
                    CurrentProtection100Enable = PI_CurrentProtection100Enable,
                    VREnable = PI_VREnable,
                    BridgeEnable = PI_BridgeEnable,
                    VLFEnable = PI_VLFEnable,
                    SA540Enable = PI_SA540Enable,
                    SA640Enable = PI_SA640Enable,
                    Tangent2000Enable = PI_Tangent2000Enable,
                },
                StableVoltage = StableVoltage,
                StableCurrent = StableCurrent,
                ConnectionEnable = ConnectionEnable,
                ErrorCode1 = ErrorCode1,
                ErrorCode2 = ErrorCode2,
                ErrorCode3 = ErrorCode3,
                ErrorCode4 = ErrorCode4,
                MKZState = new MKZStateInfo((FazeType)config["FazeType"])
                {
                    DoorLeft = MKZ_DoorLeft,
                    DoorRight = MKZ_DoorRight,
                    DangerousPotencial = MKZ_DangerousPotencial,
                    SafeKey = MKZ_SafeKey,
                    Ground = MKZ_Ground,
                    Stop = MKZ_Stop
                },
                OutRNVoltage = OutRNVoltage, // напряжение латтра
                OutRNCurrent = OutRNCurrent // ток нагрузки латтра
            };
            MEAData.HVSwitch1 = HVSwitchState.HVM;
            MEAData.HVSwitch2 = HVSwitchState.Ground;
            MEAData.HVSwitch3 = HVSwitchState.Ground;

            if (IsAutoVoltage)
            {
                if (Module == LabModule.Burn)
                {
                    if (OutRNVoltage <= 120 && hvTrigger)
                    {
                        if (voltageHoldCount-- > 0)
                            if (OutRNVoltage <= 0) OutRNVoltage++;
                            else
                                OutRNVoltage += Random.Shared.Next(-1, 2);
                        else
                        {
                            voltageHoldCount = defaultCounter;
                            plus = true;
                            VoltageUp();
                        }
                    }
                    else if (outRNVoltage >= 240)
                    {
                        if (voltageHoldCount-- > 0)
                            OutRNVoltage += Random.Shared.Next(-1, 2);
                        else
                        {
                            voltageHoldCount = defaultCounter;
                            plus = false;
                            hvTrigger = true;
                            VoltageDown();
                            VoltageDown();
                            VoltageDown();
                        }
                    }
                    else
                    {
                        if (plus) VoltageUp();
                        else VoltageDown();
                    }
                }
                else
                {
                    if (OutRNVoltage <= 10) plus = true;
                    else if (OutRNVoltage >= 240) plus = false;

                    if (plus) VoltageUp();
                    else VoltageDown();
                }
            }
            data.Voltage = Voltage;
            data.Current = Current;
            data.OptionalInfo = $"{minKV} {maxKV} {true} {3}";

            lastData = data;
            data.ParseParamsToMessage();
            OnDataReceived?.Invoke(data);
            return data;
        }
        public void VoltageUp()
        {
            if (PI_RNManualEnable)
            {
                if (OutRNVoltage < 251.0)
                    OutRNVoltage += Random.Shared.Next(1, 3);
            }
        }
        public void VoltageDown()
        {
            if (OutRNVoltage > 0.0)
                OutRNVoltage -= Random.Shared.Next(1, 3);
        }
        public void SetModule(LabModule module) => Module = module;
        public void ResetModule() => Module = LabModule.Main;
        public void SetStep(int step)
        {
            new Timer(state =>
            {
                if (step == 0) Step = step;

                { // шанс не переключится на нужную ступень

                    var success = Random.Shared.Next(1, step + 1) > step / 2;
                    Thread.Sleep(10);
                    var successBurning = Random.Shared.Next(1, step + 1) > step / 2;
                    if (success)
                    {
                        Step = step;
                        if (successBurning)
                            MainControllerSimulator.successBurning = true;

                        MainControllerSimulator.successBurning = false;
                    }
                }
            }).Change(2000, Timeout.Infinite);
        }
        public void GVI_OneFire() { }
        public void GVI_LineFire(int count) { }
        public void GVI_FireStop() { }
        public void GVI_SetDelay(int delay) { }
        public void SetPower(BurnPower power) => PowerBurn = power;
        public bool PowerUp()
        {
            ModulePowerState res = ModulePowerState.Disable;
            switch (Module)
            {
                case LabModule.HVMAC:
                case LabModule.HVMDCHi:
                    res = lastData.PowerInfoMessage.ModulePower.HVMACModuleEnable;
                    break;
                case LabModule.HVMDC:
                    res = lastData.PowerInfoMessage.ModulePower.HVMDCModuleEnable;
                    break;
                case LabModule.HVBurn:
                    res = lastData.PowerInfoMessage.ModulePower.HVBurnModuleEnable;
                    break;
                case LabModule.Burn:
                    res = lastData.PowerInfoMessage.ModulePower.BurnModuleEnable;
                    break;
                case LabModule.Bridge:
                    res = lastData.PowerInfoMessage.ModulePower.BridgeModuleEnable;
                    break;
                case LabModule.GP500:
                    res = lastData.PowerInfoMessage.ModulePower.GP500ModuleEnable;
                    break;
                case LabModule.GVI:
                    res = lastData.PowerInfoMessage.ModulePower.GVIModuleEnable;
                    break;
                case LabModule.JoinBurn:
                    res = lastData.PowerInfoMessage.ModulePower.JoinBurnModuleEnable;
                    break;
                case LabModule.LVMeas:
                    res = lastData.PowerInfoMessage.ModulePower.LVMModuleEnable;
                    break;
                case LabModule.Meas:
                    res = lastData.PowerInfoMessage.ModulePower.MeasureModuleEnable;
                    break;
                case LabModule.SA540_1:
                    res = lastData.PowerInfoMessage.ModulePower.SA540MoudleEnable;
                    break;
                case LabModule.SA540_3:
                    res = lastData.PowerInfoMessage.ModulePower.SA540MoudleEnable;
                    break;
                case LabModule.SA640:
                    res = lastData.PowerInfoMessage.ModulePower.SA640MoudleEnable;
                    break;
                case LabModule.VLF:
                    res = lastData.PowerInfoMessage.ModulePower.VLFModuleEnable;
                    break;
                case LabModule.Tangent2000:
                    res = lastData.PowerInfoMessage.ModulePower.Tangent2000Enable;
                    break;
            }

            if (res == ModulePowerState.Enable) return true;
            return false;
        }
        public bool PowerDown()
        {
            OutRNVoltage = 0;
            return true;
        }
        public void SetCommand(string message) { }
        public void SetModuleAndDontOffCurrent(LabModule module) => SetModule(module);
        public void PowerOn(IEnumerable<PowerTargets> powerTargets)
        {
            if ((FazeType)config["FazeType"] == FazeType.ThreeFaze && !lastData.MKZState.MKZError
             || lastData.Module != LabModule.GP500 && lastData.MKZState.Ground && !lastData.MKZState.MKZError
             || !lastData.MKZState.MKZError)
                SuccessPowerOn(powerTargets);
            else
                FailPowerOn();

            void SuccessPowerOn(IEnumerable<PowerTargets> powerTargets)
            {
                var startTargets = new Dictionary<PowerTargets, TargetState>();

                new Timer(state =>
                {
                    LabModuleState = LabModuleStates.Starting;
                    if (PowerUp())
                    {
                        ModuleStateChanged?.Invoke(true, 0);
                        LabModuleState = LabModuleStates.Running;
                        if (!lastData.PowerInfoMessage.RNManualEnable) RegulatorEnable();
                    }
                    else
                    {
                        ModuleStateChanged?.Invoke(true, -1);
                        LabModuleState = LabModuleStates.Stop;
                    }
                }).Change(1000, Timeout.Infinite);
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
                notification.SendNotificationOK($"Предупреждения системы безопасности:\n{stringResMKZ.ToString()}", null);
                new Timer(state =>
                {
                    ModuleStateChanged?.Invoke(true, -1);
                }).Change(1000, Timeout.Infinite);
            }
        }
        public void PowerOff()
        {
            if (LabModuleState != LabModuleStates.Stopping)
                new Timer(state =>
                {
                    LabModuleState = LabModuleStates.Stopping;
                    if (PowerDown())
                    {
                        ModuleStateChanged?.Invoke(false, 0);
                        LabModuleState = LabModuleStates.Stop;
                        if (lastData.PowerInfoMessage.RNManualEnable) RegulatorDisable();
                    }
                    else ModuleStateChanged?.Invoke(false, -1);
                    LabModuleState = LabModuleStates.Stop;
                }).Change(1000, Timeout.Infinite);
        }
        public FazeType GetFazeType() => FazeType.NoFaze;
        public void RegulatorDisable()
        {
            if (isRegulatorEnabled)
            {
                PI_RNManualEnable = false;
                ControllerLogger.WriteString("Notify: Voltage Regulator Disabled");
                isRegulatorEnabled = false;
            }
        }
        public void RegulatorEnable()
        {
            if (!isRegulatorEnabled)
            {
                PI_RNManualEnable = true;
                ControllerLogger.WriteString("Notify: Voltage Regulator Enabled");
                isRegulatorEnabled = true;
            }
        }
        public void GetTrial() { }
        public void EnterKeys(int key1, int key2, int key3, int key4) { }
        public bool Connect() => true;
        public bool Connect(int AttemptCount) => true;
        public void Disconnect() { }
        public void StartQueue() { }
        public void StopQueue() { }
        public void EmergencyStop() { }
        public void GetDateCalib() { }
        public void SetDateCalib(int year, int mouth, int day) { }
        public void GetSerialNumCalib() { }
        public void SetSerialNumCalib(int serial) { }
        public void GetLevelZeroPointCalib() { }
        public void SetLevelZeroPointCalib(int delta) { }
        public void AddPointDCCalib(int serial, int voltage) { }
        public void AddPointACCalib(int serial, int voltage) { }
        public void DeleteAllPointsCalib(int serial) { }
        public void GetPointCountCalib(int serial) { }
        public void RemovePointDCCalib(int serial, int index) { }
        public void RemovePointACCalib(int serial, int index) { }
        public void EditPointDCCalib(int serial, int index, int voltage) { }
        public void EditPointACCalib(int serial, int index, int voltage) { }
        public void GetKoefDCCalib(int serial, int index) { }
        public void GetKoefACCalib(int serial, int index) { }
        public void GetFirmwareRev() { }

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

        public void RunLab() => mainPowerService.Run();
        #endregion
    }
}
