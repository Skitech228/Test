using System.ComponentModel;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect
{
    public enum Amplitude : int
    {
        [Description("30")]
        V30 = 30,
        [Description("60")]
        V60 = 60,
        [Description("90")]
        V90 = 90
    }
}
