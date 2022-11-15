using System.ComponentModel;

namespace ANG24.Sys.Domain.DBModels
{
    public class ParameterAddition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ValueType ValueType { get; set; }
        //public int DeviceParameterId { get; set; }
        public DeviceParameter DeviceParameter { get; set; }
        public ICollection<ResultValue> ResultValues { get; set; } = new List<ResultValue>();
        public override string ToString() => $"type: ParameterAddition [id = {Id}, Name = {Name}]";
    }
    public enum ValueType
    {
        [Description("Максимальное")]
        Max,
        [Description("Минимальное")]
        Min,
        [Description("Среднее")]
        Avg,
        [Description("Пользовательское")]
        Custom
    }
}
