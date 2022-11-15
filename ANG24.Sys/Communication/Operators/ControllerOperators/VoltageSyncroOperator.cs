using ANG24.Sys.Communication.Operators.AbstractOperators;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{

    public sealed class VoltageSyncroOperator : StringSerialControllerOperator<VoltageSyncroData>, IVoltageSyncroOperator
    {
        public override event Action<VoltageSyncroData> OnDataReceived;
        private Timer timer;
        public VoltageSyncroOperator() => Name = Application.Types.Enum.LabController.VoltageSyncronizer.ToString();
        public void SetMode(int mode) => SetCommand("#SETMODE," + mode + ";");
        public void Start()
        {
            timer = new Timer(state =>
            {
                SetCommand("#GETMODE");
            });
            timer.Change(0, 1000);
        }
        public void Stop() => timer?.Dispose();
        protected override void CommandBroker(VoltageSyncroData data)
        {

            OnDataReceived?.Invoke(data);
        }
    }
}
