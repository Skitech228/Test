using System.ComponentModel.DataAnnotations;

namespace ANG24.Sys.Domain.DBModels
{
    public class TestTarget
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public override string ToString() => $"type: TestTarget [id = {Id}, Name = {Name}]";
    }
}
