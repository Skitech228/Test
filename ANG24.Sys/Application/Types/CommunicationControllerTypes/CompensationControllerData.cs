using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class CompensationControllerData : StringData
    {
        public string Command { get; set; }
        public BridgeCommutatorPhase CurrentPhase { get; set; }
        public BridgeCommutatorState CurrentState { get; set; }
        public bool VoltageChangeNeeded { get; set; }
        public bool CompensationIsMatched => Success;
        public override void ParseData(string input)
        {
            base.ParseData(input);
            if (Message.Contains("Voltage must by 15..25V"))
                VoltageChangeNeeded = true;
            if (Message.Contains("Error"))
                return;
            if (Message.Contains("Result"))
                Success = true;
        }
    }
}
