using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IVoltageSyncroOperator : IControllerOperator<VoltageSyncroData>
    {
        void Start();
        void Stop();
        void SetMode(int mode);
    }
}
