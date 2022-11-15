namespace ANG24.Sys.Domain.DBModels
{
    public class ModuleStage
    {
        public int Id { get; set; }
        public string ModuleName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public int UserId { get; set; }
        public User User { get; set; }
    }
}
