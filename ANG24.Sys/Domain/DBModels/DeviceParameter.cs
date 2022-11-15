using System.ComponentModel.DataAnnotations;

namespace ANG24.Sys.Domain.DBModels
{
    public class DeviceParameter
    {
        public int Id { get; set; }
        [Required]
        public string Parameter { get; set; }
        public string Synonym { get; set; }
        //public int UnitId { get; set; }
        //public int DeviceId { get; set; }
        public Unit Unit { get; set; }
        public Device Device { get; set; }
        public ICollection<ResultValue> ResultValues { get; set; } = new List<ResultValue>();
        public ICollection<ParameterAddition> ParameterAdditions { get; set; } = new List<ParameterAddition>();
        public override string ToString()
        {
            return string.Format("type: DeviceParameter [Name = {0} , Unit = {1}, Id = {2}, Synonym = {3}]", Parameter, Unit?.Name, Id, Synonym);
        }
    }
}
