namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base
{
    public interface IControllerOperatorData
    {
        public string? Message { get; }
        public string? OptionalInfo { get; set; }
        public bool Success { get; }
        public int ErrorCode { get; }
        public string? ErrorMessage { get; }
    }
}
