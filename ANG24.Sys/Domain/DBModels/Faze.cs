namespace ANG24.Sys.Domain.DBModels
{
    public class Faze
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] ConnectionImage { get; set; }
        //public int TestObjectId { get; set; }
        public TestObject TestObject { get; set; }
        public ICollection<ResultValue> ResultValues { get; set; } = new List<ResultValue>();
        public ICollection<FazeMeteringResult> FazeMeteringResults { get; set; } = new List<FazeMeteringResult>();
        public override string ToString() => $"type: FazeTest [Id = {Id}, Name = {Name}]";
    }
}
