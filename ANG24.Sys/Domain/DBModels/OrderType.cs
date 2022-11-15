namespace ANG24.Sys.Domain.DBModels
{
    public class OrderType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public override string ToString() => $"type: OrderType [Id = {Id}, Name = {Name}]";
    }
}
