using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Infrastructure.Helpers;
using Autofac;

namespace ANG24.Sys.Communication.Helpers;
public sealed class LabVoltageSafeguard : Executer, ILabVoltageSafeguard
{
    private readonly IMainControllerOperator main;
    private readonly ILabConfigurationService config;
    private readonly INotificationService notification;
    private readonly ILifetimeScope container;
    private MEAData ld;
    private MEAData lastData
    {
        get => ld;
        set
        {
            ld = value;
            this.SetVoltage(value.Module, value.Voltage, value.StableVoltage);
        }
    }
    private readonly Dictionary<string, VoltageAnalyzer> VAs = new();

    public LabVoltageSafeguard(IMainControllerOperator main,
                               ILabConfigurationService config,
                               INotificationService notification,
                               ILifetimeScope container) : base(container)
    {
        this.main = main;
        this.config = config;
        this.notification = notification;
        this.container = container;
        main.OnDataReceived += (data) =>
        {
            if (data.Module != LabModule.NoMode)
                lastData = data;
        };
        main.ModuleStateChanged += (s, ec) =>
        {
            if (s && ec == 0)
                foreach (var item in VAs)
                    item.Value.ResetConditions();
        };
        Initialize();
    }
    private void Initialize()
    {
        foreach (var item in (IEnumerable<LabModule>)config["LabComposition"])
        {
            var name = "";
            var va = new VoltageAnalyzer((int)config["MaxACVoltage"], (int)config["MaxDCVoltage"]);
            switch (item)
            {
                case LabModule.HVMAC:
                    HVMAC(va);
                    name = item.ToString();
                    break;
                case LabModule.HVMDC:
                    HVMDC(va);
                    name = item.ToString();
                    break;
                case LabModule.HVMDCHi:
                    HVMDCHi(va);
                    name = item.ToString();
                    break;
                case LabModule.JoinBurn:
                    JoinBurn(va);
                    name = item.ToString();
                    break;
                case LabModule.HVBurn:
                    HVBurn(va);
                    name = item.ToString();
                    break;
            };
            if (!string.IsNullOrEmpty(name))
            {
                va.Module = item;
                VAs.Add(name, va);
            }
        }
        void HVBurn(VoltageAnalyzer va)
        {
            _ = va.AddCondition("Over4kV", async () =>
            {
                notification.SendNotification("Оценка уровня пульсации и проверка рода тока", 5000);
                main.RegulatorDisable();
                await Task.Delay(5000);
                if (lastData.VoltageType == 1 || lastData.VoltageType == 3)
                {
                    main.PowerOff();
                    _ = notification.SendNotificationOK("Проверте выпрямитель на наличие шунта. Работа остановлена", ok: null);
                }
            })
            .AddCondition("OverMaxDCVoltage", () =>
            {
                main.RegulatorDisable();
                notification.SendNotificationOK("Превышение по подьему напряжения", null);
            })
            .AddCondition("WithinMaxDCVoltage", main.RegulatorEnable)
            .AddReferenceThatReset("OverMaxDCVoltage", "WithinMaxDCVoltage")
            .AddReferenceThatReset("WithinMaxDCVoltage", "OverMaxDCVoltage");
        }
        void HVMDC(VoltageAnalyzer va)
        {
            va.AddCondition("Over4kV", async () =>
            {
                notification.SendNotification("Оценка уровня пульсации и проверка рода тока", 5000);
                main.RegulatorDisable();
                await Task.Delay(5000);
                if (lastData.VoltageType == 1)
                {
                    notification.SendNotificationOK("Проверте выпрямитель на наличие шунта. Работа остановлена", null);
                    main.PowerOff();
                }
                if (lastData.VoltageType == 3)
                {
                    notification.SendNotificationOK("Напряжение с повышенной пульсацией", null);
                    main.PowerOff();
                }
                main.RegulatorEnable();
            })
            .AddCondition("OverMaxDCVoltage", () =>
            {
                main.RegulatorDisable();
                notification.SendNotificationOK("Превышение по подьему напряжения", null);
            })
            .AddCondition("WithinMaxDCVoltage", main.RegulatorEnable)
            .AddReferenceThatReset("OverMaxDCVoltage", "WithinMaxDCVoltage")
            .AddReferenceThatReset("WithinMaxDCVoltage", "OverMaxDCVoltage");
        }
        void HVMDCHi(VoltageAnalyzer va)
        {
            va.AddCondition("Over4kV", async () =>
            {
                notification.SendNotification("Оценка уровня пульсации и проверка рода тока", 5000);
                main.RegulatorDisable();
                await Task.Delay(5000);
                if (lastData.VoltageType == 2)
                {
                    notification.SendNotificationOK("Проверте выпрямитель на наличие шунта. Работа остановлена", null);
                    main.PowerOff();
                }
                if (lastData.VoltageType == 3)
                {
                    notification.SendNotificationOK("Напряжение с повышенной пульсацией", null);
                    main.PowerOff();
                }
                main.RegulatorEnable();
            })
            .AddCondition("OverMaxDCHIVoltage", () =>
            {
                main.RegulatorDisable();
                notification.SendNotificationOK("Превышение по подьему напряжения", null);
            })
            .AddCondition("WithinMaxDCHIVoltage", main.RegulatorEnable)
            .AddReferenceThatReset("OverMaxDCHIVoltage", "WithinMaxDCHIVoltage")
            .AddReferenceThatReset("WithinMaxDCHIVoltage", "OverMaxDCHIVoltage");
        }
        void HVMAC(VoltageAnalyzer va)
        {
            va.AddCondition("Over4kV", async () =>
            {
                notification.SendNotification("Оценка уровня пульсации и проверка рода тока", 5000);
                main.RegulatorDisable();
                await Task.Delay(5000);
                if (lastData.VoltageType == 2)
                {
                    notification.SendNotificationOK("Проверте выпрямитель на наличие шунта. Работа остановлена", null);
                    main.PowerOff();
                }
                if (lastData.VoltageType == 3)
                {
                    notification.SendNotificationOK("Напряжение с повышенной пульсацией", null);
                    main.PowerOff();
                }
                main.RegulatorEnable();
            })
              .AddCondition("Over10kV", () =>
              {
                  if ((bool)config["CompensationIsPresent"])
                  {
                      var compensation = container.Resolve<ICompensationControllerOperator>();
                      compensation.OnDataReceived += OnDataReceivedFromCompensation;
                      compensation.StartCoilSelect();
                      var noty = notification.SendNotification("Подбор компенсации", 0);

                      void OnDataReceivedFromCompensation(CompensationControllerData data)
                      {
                          if (!data.VoltageChangeNeeded)
                          {
                              if (data.CompensationIsMatched) notification.SendNotification("Компенсация подобрана", 5000);
                              else notification.SendNotification("Ошибка подбора компенсации", 5000);
                              compensation.OnDataReceived -= OnDataReceivedFromCompensation;
                          }
                      }
                  }
              })
            .AddCondition("OverMaxACVoltage", () =>
            {
                main.RegulatorDisable();
                notification.SendNotificationOK("Превышение по подьему напряжения", null);
            })
            .AddCondition("WithinMaxACVoltage", main.RegulatorEnable)
            .AddReferenceThatReset("OverMaxACVoltage", "WithinMaxACVoltage")
            .AddReferenceThatReset("WithinMaxACVoltage", "OverMaxACVoltage");
        }
        void JoinBurn(VoltageAnalyzer va)
        {
            va.AddCondition("Over4kV", async () =>
            {
                notification.SendNotification("Оценка уровня пульсации и проверка рода тока", 5000);
                main.RegulatorDisable();
                await Task.Delay(5000);
                if (lastData.VoltageType == 1)
                {
                    notification.SendNotificationOK("Проверте выпрямитель на наличие шунта. Работа остановлена", null);
                    main.PowerOff();
                }
                else if (lastData.VoltageType == 3)
                {
                    notification.SendNotificationOK("Напряжение с повышенной пульсацией", null);
                    main.PowerOff();
                }
                else
                {
                    main.RegulatorDisable();
                    var noty = notification.SendNotification("Включение модуля совместного прожига \nУстановка режима работы", 0);
                    var ret = StartJoinBurn(ref noty);
                    notification.CloseNotification(noty);
                    if (ret is not null)
                        notification.SendNotificationOK(ret, null);

                }
                string StartJoinBurn(ref int popupId)
                {
                    main.SetModuleAndDontOffCurrent(LabModule.JoinBurn);
                    Thread.Sleep(1500);
                    if (LabState.CurrentModule != LabModule.JoinBurn)
                        return "Произошла ошибка на этапе установки режима работы"; // Ошибка

                    ReplacePopUp(ref popupId, "Включение модуля совместного прожига \nЗапуск модуля");
                    main.SetCommand("POWERUP");
                    Thread.Sleep(15000);

                    ReplacePopUp(ref popupId, "Включение модуля совместного прожига \nУстановка первой ступени прожига");
                    main.SetStep(1);
                    if (!CheckStep())
                        return "Произошла ошибка на этапе установки первой ступени прожига";  // Ошибка

                    ReplacePopUp(ref popupId, "Включение модуля совместного прожига \nУстановка 50% мощности прожига");
                    main.SetPower(Application.Types.CommunicationControllerTypes.MEADataModels.Enum.BurnPower.Power50);
                    if (CheckMovingPower()) main.RegulatorEnable();
                    else
                        return "Произошла ошибка на этапе установки 50% мощности прожига"; // Ошибка

                    return null;

                    void ReplacePopUp(ref int id, string message)
                    {
                        notification.CloseNotification(id);
                        id = notification.SendNotification(message, 0);
                    }
                    bool CheckStep()
                    {
                        var sw = Stopwatch.StartNew();

                        while (sw.ElapsedMilliseconds < 10_000 || lastData.Step != 1)
                            Thread.Sleep(100);
                        sw.Reset();

                        if (lastData.Step != 1) return false;
                        return true;
                    }
                    bool CheckMovingPower()
                    {
                        var sw = Stopwatch.StartNew();

                        while (sw.ElapsedMilliseconds < 5_000 || lastData.PowerBurn != 0)
                            Thread.Sleep(100);
                        sw.Reset();
                        if (lastData.PowerBurn != 0) return false;
                        return true;
                    }
                }
            })
            .AddCondition("OverMaxJoinBurnVoltage", () =>
            {
                main.RegulatorDisable();
                notification.SendNotificationOK("Превышение по подьему напряжения", null);
            })
            .AddCondition("WithinMaxJoinBurnVoltage", main.RegulatorEnable)
            .AddReferenceThatReset("OverMaxJoinBurnVoltage", "WithinMaxJoinBurnVoltage")
            .AddReferenceThatReset("WithinMaxJoinBurnVoltage", "OverMaxJoinBurnVoltage");
        }
    }
    public void SetVoltage(LabModule module, double voltage, double altVoltage)
    {
        VAs.TryGetValue(module.ToString(), out var va);
        switch (module)
        {

            case LabModule.HVBurn:
                if (va is not null)
                    va.Voltage = voltage > altVoltage ? voltage : altVoltage;
                break;
            case LabModule.HVMAC:
            case LabModule.HVMDC:
            case LabModule.JoinBurn:
                if (va is not null)
                {
                    va.Voltage = voltage;
                    va.AltVoltage = altVoltage;
                }
                break;
            case LabModule.HVMDCHi:
                if (va is not null)
                {
                    va.Voltage = altVoltage;
                    va.AltVoltage = voltage;
                }
                break;
        }
    }

}
public class VoltageAnalyzer
{
    private double _voltage;
    private double _altVoltage;
    private readonly double maxAC;
    private readonly double maxDC;

    public LabModule Module { get; set; }
    private List<VoltageAnalyzerCondition> Conditions { get; set; } = new List<VoltageAnalyzerCondition>();
    public double Voltage
    {
        get => _voltage;
        set
        {
            _voltage = value;
            foreach (var condition in Conditions)
                condition.CheckAndInvokeIfTrueCondition(value / 1000);
        }
    }
    public double AltVoltage
    {
        get => _altVoltage;
        set => Conditions.FirstOrDefault(x => x.Name == "Over4kV")?
                .CheckAndInvokeIfTrueCondition(_altVoltage = value / 1000);
    }

    public VoltageAnalyzer(double maxAC, double maxDC)
    {
        this.maxAC = maxAC;
        this.maxDC = maxDC;
    }
    public void ResetConditions()
    {
        foreach (var item in Conditions)
            item.Reset();
    }
    public VoltageAnalyzer AddCondition(string name, Action action, Func<double, bool>? func = null)
    {
        Conditions.Add(new VoltageAnalyzerCondition
        {
            Name = name,
            Action = action,
            Condition = func is not null ? func : GetFunc(name)
        });
        Func<double, bool> GetFunc(string name) => name switch
        {
            "Over4kV" => x => x > 4.0,
            "Over10kV" => x => x > 10.0,

            "OverMaxDCVoltage" => x => x > maxDC / 1000,
            "OverMaxACVoltage" => x => x > maxAC / 1000,

            "WithinMaxACVoltage" => x => x < maxAC / 1000,
            "WithinMaxDCVoltage" => x => x < maxDC / 1000,

            "OverMaxDCHIVoltage" => x => x > 140.0,
            "WithinMaxDCHIVoltage" => x => x < 140.0,

            "OverMaxJoinBurnVoltage" => x => x > 58.5,
            "WithinMaxJoinBurnVoltage" => x => x < 58.5,
            _ => null
        };
        return this;
    }
    public VoltageAnalyzer AddReferenceThatReset(string target, string reference)
    {
        Find(target).Reference = new VoltageAnalyzerConditionReference
        {
            Target = Find(reference),
            Action = x => x.Reset()
        };
        return this;
    }
    private VoltageAnalyzerCondition Find(string Name) => Conditions.FirstOrDefault(x => x.Name == Name);
}
public class VoltageAnalyzerCondition
{
    public string Name { get; set; }
    internal bool IsPassed { get; set; } = false;
    public VoltageAnalyzerConditionReference Reference { get; set; }
    public Func<double, bool> Condition { get; set; }
    public Action Action { get; set; }
    public void Reset() => IsPassed = false;
    public void CheckAndInvokeIfTrueCondition(double value)
    {
        if (Condition?.Invoke(value) ?? false)
        {
            if (!IsPassed)
            {
                IsPassed = true;
                Action?.Invoke();
                Reference?.Action?.Invoke(Reference.Target);
            }
        }
    }
}
public class VoltageAnalyzerConditionReference
{
    //таргет зависимости
    public VoltageAnalyzerCondition Target { get; set; }
    //Действие, выполняемое с зависимостью
    public Action<VoltageAnalyzerCondition> Action { get; set; }
}
///// <summary>
///// Упорядоченный набор действий с ожиданием выполнения 
///// </summary>
//public class ActionScript
//{
//    internal Queue<Func<int>> Actions { get; set; }
//    public ActionScript(ICollection<Func<int>> acts)
//    {
//        Actions = new Queue<Func<int>>(acts);
//        Task.Factory.StartNew(() =>
//        {
//            while (Actions.Count > 0)
//                if (Actions.Dequeue()?.Invoke() != 0)
//                    break;
//        });
//    }
//}
