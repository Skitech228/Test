namespace ANG24.Sys.Domain.DBModels
{
    public class ResultValue
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsServiceValue { get; set; }
        //public int DeviceParameterId { get; set; }
        //public int DeviceId { get; set; }
        //public int FazeId { get; set; }
        //public int OrderId { get; set; }
        //public int ParameterAdditionId { get; set; }
        public DeviceParameter DeviceParameter { get; set; }
        public Device Device { get; set; }
        public Faze Faze { get; set; }
        public Order Order { get; set; }
        public ParameterAddition ParameterAddition { get; set; }
        public override string ToString() => $"type: ResultValue [id = {Id}, Value = {Value}, isServiceValue = {IsServiceValue}]";
    }
}
