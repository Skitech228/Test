namespace ANG24.Sys.Domain.DBModels
{
    public class DeviceGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public override string ToString() => $"type: DeviceGroup [Name = {Name} id = {Id}]";
    }
}
