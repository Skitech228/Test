namespace ANG24.Sys.Domain.DBModels
{
    public class Session
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<LogData> LogDatas { get; set; } = new List<LogData>();
    }
}
