namespace ANG24.Sys.Domain.DBModels
{
    public class LogData
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        //public int SessionId { get; set; }
        public Session Session { get; set; }
    }

}
