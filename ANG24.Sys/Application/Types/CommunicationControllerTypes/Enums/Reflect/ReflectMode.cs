using System.ComponentModel;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect
{
    public enum ReflectMode : int
    {
        [Description("Без режима")]
        NoMode = 0,
        [Description("Импульсный")]
        Impulse = 1,
        [Description("Ждущий")]
        Continuous = 2
    }
}
