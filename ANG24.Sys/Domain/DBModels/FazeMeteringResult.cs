namespace ANG24.Sys.Domain.DBModels
{
    public class FazeMeteringResult
    {
        public int Id { get; set; }
        public bool TestResult { get; set; }
        //public int FazeId { get; set; }
        //public int OrderId { get; set; }
        public Faze Faze { get; set; }
        public Order Order { get; set; }

    }
}
