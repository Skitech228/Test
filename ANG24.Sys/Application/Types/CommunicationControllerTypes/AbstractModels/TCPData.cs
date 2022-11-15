using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels
{
    public abstract class TCPData : IControllerOperatorData
    {
        public TCPData() { }
        public abstract void ParseData(string input);
        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public string OptionalInfo { get; set; }
        public string Command { get; protected set; }
        public int ErrorCode { get; protected set; }
        public string ErrorMessage { get; protected set; }
    }
}
