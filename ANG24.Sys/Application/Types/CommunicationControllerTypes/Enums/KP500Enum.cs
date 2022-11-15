namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums
{
    public enum KP500Enum
    {
    }
    public enum KP500FrequencyEnum : byte
    {
        Frq_480 = 0,
        Frq_1069 = 1,
        Frq_9796 = 2,
        Frq_fix = 3,
        Frq_auto = 4
    }

    public enum KP500Resistance : byte
    {
        Resis_05 = 0,
        Resis_1 = 16,
        Resis_2 = 32,
        Resis_4 = 48,
        Resis_8 = 64,
        Resis_16 = 80,
        Resis_32 = 96,
        Resis_64 = 112,
        Resis_128 = 128,
        Resis_256 = 144
    }

    public enum KP500ModeImp : byte
    {
        Nep = 0,
        Imp = 1,
        IM2 = 2,
        IM3 = 3
    }

    public enum KP500SetMCH2 : byte
    {
        Set12 = 0,
        Set23 = 1,
        Set13 = 2
    }
}
