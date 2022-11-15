using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class BridgeCommutatorData : StringData
    {
        public string Command { get; set; }
        public BridgeCommutatorPhase CurrentPhase { get; set; }
        public BridgeCommutatorState CurrentState { get; set; }
        public override void ParseData(string input)
        {
            var opt = OptionalInfo.Split();
            if (opt is not null)
            {
                Command = opt[0];
                CurrentPhase = (BridgeCommutatorPhase)System.Enum.Parse(typeof(BridgeCommutatorPhase), opt[1]);
            }
            base.ParseData(input);
            Success = false;
            if (!int.TryParse(input.Split('=')[1], out int state))
                return;

            switch (Command)
            {
                case "Phase":
                    if ((state & 0x08) == (int)CurrentPhase) return;
                    break;
                case "ResetState":
                    if (state != 0) return;
                    break;
                case "ResetPhase":
                    if ((state & 0x08) != 0) return;
                    break;
                case "State":
                    if ((state & 0x01) != 0x01) return;
                    break;
                default:
                    break;
            }
            Success = true;
        }
    }
}
