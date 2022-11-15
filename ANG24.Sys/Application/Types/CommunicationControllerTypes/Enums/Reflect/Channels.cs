using System.ComponentModel;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect
{
    public enum Channel : int
    {
        [Description("Канал L1")]
        Channel1 = 1,
        [Description("Канал L2")]
        Channel2 = 2,
        [Description("Канал L3")]
        Channel3 = 3,
        [Description("Канал ARC")]
        IDMChannel = 4,
        [Description("Канал Decay")]
        DecayChannel = 5,
        [Description("Канал ICE")]
        ICEChannel = 6
    }
}
