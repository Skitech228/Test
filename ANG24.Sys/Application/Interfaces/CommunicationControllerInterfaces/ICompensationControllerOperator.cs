using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface ICompensationControllerOperator : IControllerOperator<CompensationControllerData>
    {
        void StartCoilSelect();
        void ResetCombination();
    }
}
