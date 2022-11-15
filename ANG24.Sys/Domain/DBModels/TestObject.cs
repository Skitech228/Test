namespace ANG24.Sys.Domain.DBModels
{
    public class TestObject
    {
        public int Id { get; set; }
        public string Mark { get; set; }
        public string Work_U { get; set; }
        public string DiametrSize { get; set; }
        public double? Lehght { get; set; }
        public ICollection<Faze> Fazes { get; set; } = new List<Faze>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public override string ToString() => $"type: TestObject [id = {Id}, Mark = {Mark}, Work_U = {Work_U}, DiameterSize = {DiametrSize}, Lenght = {Lehght ?? 0}]";
    }
}
