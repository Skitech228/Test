using System.ComponentModel.DataAnnotations;

namespace ANG24.Sys.Domain.DBModels
{
    public class EnergyObject
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public override string ToString()
        {
            return string.Format("type: EnergyObject [Id = {0}, Name = {1}]", Id, Name);
        }
    }
}
