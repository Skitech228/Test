using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class PowerSelectorData : StringData
    {
        public string Command { get; set; }
        public int PowerType { get; private set; }

        public override void ParseData(string input)
        {
            base.ParseData(input);
            if (input.Contains("mode"))
            {
                PowerType = Convert.ToInt32(input.Split('=')[1]
                    .Replace('\r', ' ')
                    .Replace('\n', ' ')
                    .Trim());
            }

            Success = false;
        }
    }
}
