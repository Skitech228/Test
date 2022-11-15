using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class VoltageRegulatorData : StringData
    {
        public int VoltageLevel { get; set; }
        public double RMS { get; set; }
        public static double RMS1 { get; set; }
        public int GS { get; set; } // Сделать парсинг магнитов
        //public int T1 { get; set; }
        //public int T2 { get; set; }
        //public int T3 { get; set; }
        public int TMax { get; set; }
        public override void ParseData(string input)
        {
            base.ParseData(input);
            if (Message.Contains("RMS"))
            {
                var items = Message.Split('=')[1].Split(' ');
                if (double.TryParse(items[0], out var rms)) RMS = rms;
                if (int.TryParse(items[2], out var gs)) GS = gs;
            }
            if (Message.Contains("T1="))
            {
                var items = Message.Trim(' ').Split(';');
                TMax = int.Parse(items[3].Split('=')[1]);
                //T1 = int.Parse(items[0].Split('=')[1]);
                //T2 = int.Parse(items[1].Split('=')[1]);
                //T3 = int.Parse(items[2].Split('=')[1]);
            }
            if (Message.Contains("Level"))
                if (int.TryParse(Message.Trim(' ').Split('=')[1], out var level)) VoltageLevel = level;
        }
    }
}
