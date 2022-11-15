namespace ANG24.Sys.Application.Types
{
    public struct Method
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public string[] Types { get; set; }
        public override string ToString() => $"{ReturnType} {Name}";
    }
    public struct OperatorMethod
    {
        public string OperatorName { get; set; }
        public Method? Method { get; set; }
        public object ResponseForContinie { get; set; }
        public bool IsAsync { get; set; }
    }
}
