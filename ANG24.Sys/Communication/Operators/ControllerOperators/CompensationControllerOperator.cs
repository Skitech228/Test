using ANG24.Sys.Application.Types.Enum;
using ANG24.Sys.Communication.Interfaces;

namespace ANG24.Sys.Communication.Operators.ControllerOperators;

public sealed class CompensationControllerOperator : StringSerialControllerOperator<CompensationControllerData>, ICompensationControllerOperator
{
    public override event Action<CompensationControllerData> OnDataReceived;
    private readonly IMainControllerOperator service;

    public CompensationControllerOperator(IMainControllerOperator service)
    {
        Name = LabController.Compensation.ToString();
        this.service = service;
    }
    public void StartCoilSelect() => SetCommand("#START_COIL_SELECT");
    public void ResetCombination() => SetCommand("#SET_COIL_COMBINATION:0;");
    protected override void CommandBroker(CompensationControllerData data)
    {
        if (data.VoltageChangeNeeded)
        {
            var vol = GetVoltageCommand();
            while (vol < 15.0 || vol > 25.0)
            {
                vol = GetVoltageCommand();
                Thread.Sleep(50);
                if (vol < 15.0) service.VoltageUp();
                if (vol > 25.0) service.VoltageDown();
                Thread.Sleep(300);
            }
        }
        OnDataReceived?.Invoke(data);
    }
    private int GetVoltageCommand()
    {
        int vol = 0;
        var port = Controller as ISerialPortController;
        try
        {
            var req = port.ApplyCommand("#GET_VOLTAGE").Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (req.Contains("Voltage="))
                vol = int.Parse(req.Split('=')[1]);
        }
        catch (TimeoutException) { }
        return vol;
    }
}
