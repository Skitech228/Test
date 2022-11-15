namespace ANG24.Sys.Domain.DBModels
{
    public class Prefix
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CutName { get; set; }
        public double? Factor { get; set; }
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public override string ToString() => $"type: Prefix [Id = {Id}, Name = {Name}, CutName = {CutName}, Factor = {Factor}]";
    }
}
