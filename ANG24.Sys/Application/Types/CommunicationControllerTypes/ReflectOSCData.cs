using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public struct ReflectOSCData
    {
        public ChartPoint[] ReflectData { get; set; }
        public int Gs { get; set; }
        public int Lehght { get; set; }
        public Channel Channel { get; set; }
    }
    public struct ChartPoint
    {
        //public ChartPoint()
        //{
        //    X = 0;
        //    Channels = new ushort[6];  // 6 каналов всего
        //}
        //public ushort[] Channels { get; set; } // когда-нибудь потом
        public int X { get; set; }
        public ushort? Channel1 { get; set; }
        public ushort? Channel2 { get; set; }
        public ushort? Channel3 { get; set; }
        public ushort? Channel41 { get; set; } // arc
        public ushort? Channel42 { get; set; } // base
        public ushort? Channel5 { get; set; }
        public ushort? Channel6 { get; set; }
        public ushort? MatchedChannel { get; set; }
        public override string ToString() => X.ToString();
    }
}
