using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class VoltageSyncroData : StringData
    {
        public string Command { get; set; }
        public int VoltageType { get; private set; }
        public override void ParseData(string input)
        {
            base.ParseData(input);
            if (input.Contains("RNSMODE"))
            {
                VoltageType = Convert.ToInt32(input.Split(':')[1]);
                Success = true;
            }
        }
    }
}
