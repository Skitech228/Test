using System.ComponentModel.DataAnnotations;

namespace ANG24.Sys.Domain.DBModels
{
    public class Unit
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Synonim { get; set; }
        //public int PrefixId { get; set; }
        public Prefix Prefix { get; set; }
        public ICollection<DeviceParameter> DeviceParameters { get; set; } = new List<DeviceParameter>();
        public override string ToString() => $"type: Unit [id = {Id}, Name = {Name}, Synonim = {Synonim}]";
    }
}
