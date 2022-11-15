using System.ComponentModel.DataAnnotations;

namespace ANG24.Sys.Domain.DBModels
{
    public class Module
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Synonym { get; set; }
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public override string ToString() => $"type: Module [Id = {Id}, Name = {Name}, Synonym = {Synonym}]";
    }
}
