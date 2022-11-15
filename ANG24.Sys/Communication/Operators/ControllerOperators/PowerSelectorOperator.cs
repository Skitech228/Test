using ANG24.Sys.Communication.Operators.AbstractOperators;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{

    public sealed class PowerSelectorOperator : StringSerialControllerOperator<PowerSelectorData>, IPowerSelectorOperator
    {
        public override event Action<PowerSelectorData> OnDataReceived;
        private Timer timer;
        public PowerSelectorOperator() => Name = Application.Types.Enum.LabController.PowerSelector.ToString();
        public void Start()
        {
            timer = new Timer(state =>
            {
                SetCommand("#GETMODE");
            });
            timer.Change(100, 1000);
        }
        public void Stop() => timer?.Dispose();
        public void Reset() => SetCommand("#SETMODE,0;");
        protected override void CommandBroker(PowerSelectorData data) => OnDataReceived?.Invoke(data);
    }
}
