using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IBridgeCommutatorOperator : IControllerOperator<BridgeCommutatorData>
    {
        BridgeCommutatorState CurrentState { get; }
        BridgeCommutatorPhase CurrentPhase { get; }
        void SetState(BridgeCommutatorState state);
        void ReversePhase();
        void ResetPhase();
        void ResetState();
        void Reset();
    }
}
