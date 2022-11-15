using System.ComponentModel;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect
{
    public enum Mode : int
    {
        [Description("Импульсный")]
        Impulse = 1,
        [Description("ИДМ")]
        IDM = 2,
        [Description("Волна по току")]
        ICE = 3,
        [Description("Волна по напряжению")]
        Decay = 4
    }
}
