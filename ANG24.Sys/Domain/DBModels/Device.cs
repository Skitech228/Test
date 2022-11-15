using System.ComponentModel.DataAnnotations;

namespace ANG24.Sys.Domain.DBModels
{
    public class Device
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Synonym { get; set; }
        public int Zav_N { get; set; }
        public DateTime VerificationDate { get; set; }
        public DateTime NextVerificationDate { get; set; }
        public bool IsInner { get; set; }
        //public int DeviceGroupId { get; set; }
        public DeviceGroup DeviceGroup { get; set; }
        public ICollection<ResultValue> ResultValues { get; set; } = new List<ResultValue>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<DeviceParameter> DeviceParameters { get; set; } = new List<DeviceParameter>();
        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public override string ToString()
        {
            return string.Format("type: Device [Id = {0}, Name = {1}, Zav_N = {2}, DatePoverki = {3}, DateNextPoverki = {4}, DeviceGroup = {5}, IsInner = {6}, Synonym={7}]",
                Id,
                Name,
                Zav_N,
                VerificationDate,
                NextVerificationDate,
                DeviceGroup?.Name,
                IsInner,
                Synonym);
        }
    }
}
