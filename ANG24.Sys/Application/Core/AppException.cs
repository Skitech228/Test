namespace ANG24.Sys.Application.Core
{
    public struct AppException
    {
        public AppException(int statusCode, string message, string details = "")
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Details = details;

        }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
