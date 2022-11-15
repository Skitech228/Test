namespace ANG24.Sys.Application.Types.ServiceTypes;

public struct Point
{
    public double X { get; set; }
    public double Y { get; set; }
    public override string ToString() => $"{X} | {Y}";
}
