using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels
{
    public class MKZStateInfo
    {
        public MKZStateInfo(FazeType fazeType) => this.fazeType = fazeType;
        private readonly FazeType fazeType;
        public bool MKZError
        {
            get
            {
                if (!Stop || !SafeKey || !DangerousPotencial || !DoorLeft || !DoorRight)
                    if (fazeType == FazeType.ThreeFaze && !Ground) return true;
                return false;
            }
        }
        public bool DoorLeft { get; set; }
        public bool DoorRight { get; set; }
        public bool DangerousPotencial { get; set; }
        public bool Ground { get; set; }
        public bool SafeKey { get; set; }
        public bool Stop { get; set; }
        public MKZStateInfo(string InputData, FazeType fazeType)
        {
            this.fazeType = fazeType;
            var i = int.Parse(InputData);
            if ((i & 0x01) != 0) Stop = true;
            if ((i & 0x02) != 0) SafeKey = true;
            if ((i & 0x04) != 0) DangerousPotencial = true;
            if ((i & 0x08) != 0) Ground = true;
            if ((i & 0x10) != 0) DoorLeft = true;
            if ((i & 0x20) != 0) DoorRight = true;
        }
    }
}
