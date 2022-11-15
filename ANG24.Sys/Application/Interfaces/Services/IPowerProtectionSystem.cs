namespace ANG24.Sys.Application.Interfaces.Services;

public interface IPowerProtectionSystem : IExecuteableCreator
{
    event Action VoltageOverload;
    event Action VoltageOutOfOverload;
    event Action CurrentOverload;
    int Voltage { get; set; }
    int Current { get; set; }
    bool Active { get; set; }
    void SetVoltageLimit(int voltageLimit);
    void SetCurrentLimit(int current);
    void SetDefault();
}
