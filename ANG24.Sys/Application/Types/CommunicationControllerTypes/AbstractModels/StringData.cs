using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels
{
    public class StringData : IControllerOperatorData
    {
        public StringData() { }
        public virtual void ParseData(string input) => Message = input.Replace('\r', ' ').Replace('\n', ' ').Trim();
        public bool Success { get; protected set; } = true;
        public string Message { get; protected set; } = string.Empty;
        public int ErrorCode { get; protected set; }
        public string ErrorMessage { get; protected set; }
        public string OptionalInfo { get; set; }
    }
}
