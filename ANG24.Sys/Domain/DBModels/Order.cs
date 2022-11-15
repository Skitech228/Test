namespace ANG24.Sys.Domain.DBModels
{
    public class Order
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public string FinishDescription { get; set; }
        //public int EnergyObjectId { get; set; }
        //public int TestTargetId { get; set; }
        //public int OrderTypeId { get; set; }
        //public int TestObjectId { get; set; }
        public EnergyObject EnergyObject { get; set; }
        public TestTarget TestTarget { get; set; }
        public OrderType OrderType { get; set; }
        public TestObject TestObject { get; set; }
        public ICollection<ResultValue> ResultValues { get; set; } = new List<ResultValue>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<FazeMeteringResult> FazeMeteringResults { get; set; } = new List<FazeMeteringResult>();

        public override string ToString() => $"type: Order [Id = {Id}, Number = {Number}, CreateDate = {CreateDate}]";
    }
}
