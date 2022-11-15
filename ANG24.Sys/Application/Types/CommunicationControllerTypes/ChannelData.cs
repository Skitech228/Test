namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public class ChannelData
    {
        public string ChannelName { get; set; }
        public string Description { get; set; }
        public int ChannelNumber { get; set; }
        public int lenMass { get; set; }
        public string ChartColor { get; set; }
        public double SR_LastData { get; set; }
        public ushort[] Data { get; set; }
    }
}
