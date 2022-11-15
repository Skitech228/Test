using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using ANG24.Sys.Application.Types.Enum;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class BridgeCommutatorOperator : StringSerialControllerOperator<BridgeCommutatorData>, IBridgeCommutatorOperator
    {
        public override event Action<BridgeCommutatorData> OnDataReceived;
        private BridgeCommutatorState _currentState = BridgeCommutatorState.Zero_State;
        private BridgeCommutatorPhase _currentPhase = BridgeCommutatorPhase.Normal;

        public BridgeCommutatorOperator() => Name = LabController.BridgeCommutator.ToString();

        public BridgeCommutatorState CurrentState => _currentState;
        public BridgeCommutatorPhase CurrentPhase => _currentPhase;
        public void SetState(BridgeCommutatorState state) => SetCommand($"SET_STATE:{(int)state};", success =>
        {
            if (success)
                _currentState = state;
        }, "State");
        public void ReversePhase() => SetCommand("FLOP_PHASE", success =>
        {
            if (success)
                if (_currentPhase == BridgeCommutatorPhase.Normal)
                    _currentPhase = BridgeCommutatorPhase.Reverse;
            _currentPhase = BridgeCommutatorPhase.Normal;
        }, "Phase");
        public void ResetPhase() => SetCommand("RESET_PHASE", x =>
        {
            if (x)
                _currentPhase = BridgeCommutatorPhase.Normal;
        }, "ResetPhase");
        public void ResetState() => SetCommand("SET_STATE:0;", x =>
        {
            if (x)
                _currentState = BridgeCommutatorState.Zero_State;
        }, "ResetState");
        public void Reset()
        {
            ResetPhase();
            ResetState();
        }
        protected override void AddDataInfo(ref BridgeCommutatorData data)
        {
            data.OptionalInfo = $"{currentCommand.CommandResponse} {CurrentPhase}";
        }
        protected override void CommandBroker(BridgeCommutatorData data)
        {
            data.CurrentPhase = CurrentPhase;
            data.CurrentState = CurrentState;
            OnDataReceived?.Invoke(data);
        }
    }
}
