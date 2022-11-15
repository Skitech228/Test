using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IThermoVisionControlManager : IControllerOperator<ThermoData>
    {
        event Action<UDPThermoPacket> DataReceived;
        bool Binded { get; set; }
        bool DataCollectingStart { get; set; }
        bool FindAndConnect();
        bool BindDevice();
        void StartCollect();
        void StopCollect(bool Exit = false);
        void SetExclusions(int[] indexes);
    }
}
