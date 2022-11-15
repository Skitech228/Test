using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Infrastructure.Helpers;

namespace ANG24.Sys.Communication.Helpers;
public sealed class PowerProtectionSystem : Executer, IPowerProtectionSystem
{
    //defaults
    private readonly int defaultVoltage;
    private readonly int defaultCurrent;

    //prop fields
    private int voltage;
    private int current;
    private bool voltageOvervoladed;
    private int AlarmCounter;
    private readonly ILabConfigurationService config;
    private readonly IMainControllerOperator main;


    //signals
    public event Action VoltageOverload;
    public event Action VoltageOutOfOverload;
    public event Action CurrentOverload;

    //current values
    public int Voltage
    {
        get => voltage;
        set
        {
            voltage = value;
            VoltageChanged();
        }
    }
    public int Current
    {
        get => current;
        set
        {
            current = value;
            CurrentChanged();
        }
    }

    //limits
    public int VoltageLimit { get; private set; }
    public int VoltageLimit_Gist => VoltageLimit - 10;
    public int CurrentLimit { get; private set; }
    //power state
    public bool Active { get; set; } = true;
    private LabModule currentModule;

    public PowerProtectionSystem(ILabConfigurationService config, IMainControllerOperator main, Autofac.ILifetimeScope container) : base(container)
    {
        this.config = config;
        this.main = main;
        defaultVoltage = VoltageLimit = (int)config["MaxVoltageRN"];
        defaultCurrent = CurrentLimit = (int)config["MaxCurrentRN"];
        main.OnDataReceived += Main_OnDataReceived;
    }

    private void Main_OnDataReceived(MEAData data)
    {
        if (data.Module != LabModule.NoMode)
        {
            Voltage = (int)data.OutRNVoltage;
            Current = (int)data.OutRNCurrent;
            if (data.Module != currentModule)
            {
                currentModule = data.Module;
                switch (currentModule)
                {
                    case LabModule.NoMode:
                    case LabModule.Main:
                        SetDefault();
                        break;
                    case LabModule.Burn:
                        SetCurrentLimit((int)config["MaxCurrentRN_BURN"]);
                        SetVoltageLimit((int)config["MaxVoltageRN_BURN"]);
                        break;
                    case LabModule.HVBurn:
                        SetCurrentLimit((int)config["MaxCurrentRN_HVBURN"]);
                        SetVoltageLimit((int)config["MaxVoltageRN_HVBURN"]);
                        break;
                    case LabModule.GVI:
                        SetCurrentLimit((int)config["MaxCurrentRN_HVPULSE"]);
                        SetVoltageLimit((int)config["MaxVoltageRN_HVPULSE"]);
                        break;
                    case LabModule.JoinBurn:
                        SetCurrentLimit((int)config["MaxCurrentRN_JOINTBURN"]);
                        SetVoltageLimit((int)config["MaxVoltageRN_JOINTBURN"]);
                        break;
                    case LabModule.HVMAC:
                    case LabModule.HVMDCHi:
                        SetCurrentLimit((int)config["MaxCurrentRN_HVMDCHi"]);
                        SetVoltageLimit((int)config["MaxVoltageRN_HVMDCHi"]);
                        break;
                    case LabModule.LVMeas: //
                        SetVoltageLimit((int)config["MaxVoltageRN_LVM"]);
                        break;
                }
            }
        }

    }

    //manipulatios
    public void SetVoltageLimit(int voltage) => VoltageLimit = voltage;
    public void SetCurrentLimit(int current) => CurrentLimit = current;
    public void SetDefault()
    {
        VoltageLimit = defaultVoltage;
        CurrentLimit = defaultCurrent;
    }

    //reactions
    private void VoltageChanged()
    {
        if (Active)
        {
            if (Voltage > VoltageLimit)
            {
                OnVoltageOverload();
            }
            else if (Voltage < VoltageLimit_Gist)
            {
                OnVoltageOutOfOverload();
            }

        }
    }
    private void CurrentChanged()
    {
        if (Active)
        {
            if (Current > CurrentLimit)
            {
                AlarmCounter++;
                if (AlarmCounter > 2)
                    OnCurrentOverload();
            }
            else AlarmCounter = 0;
        }
    }
    private void OnVoltageOutOfOverload()
    {
        if (voltageOvervoladed)
            VoltageOutOfOverload?.Invoke();
        voltageOvervoladed = false;
    }
    private void OnVoltageOverload()
    {
        if (!voltageOvervoladed)
            VoltageOverload?.Invoke();
        voltageOvervoladed = true;
    }
    private void OnCurrentOverload()
    {
        CurrentOverload?.Invoke();
        AlarmCounter = 0;
    }
}
