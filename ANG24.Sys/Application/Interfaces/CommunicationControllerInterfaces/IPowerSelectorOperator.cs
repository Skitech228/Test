using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IPowerSelectorOperator : IControllerOperator<PowerSelectorData>
    {
        void Reset();
        void Start();
        void Stop();
    }
}
