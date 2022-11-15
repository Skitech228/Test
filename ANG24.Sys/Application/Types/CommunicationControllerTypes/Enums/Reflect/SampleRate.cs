using System.ComponentModel;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect
{
    public enum SampleRate : int
    {
        [Description("1 ГГц")]
        Hz_1G = 1000000000,
        [Description("500 МГц")]
        Hz_500M = 500000000,
        [Description("250 МГц")]
        Hz_250M = 250000000,
        [Description("125 МГц")]
        Hz_125M = 125000000,
        [Description("50 МГц")]
        Hz_50M = 50000000,
        [Description("25 МГц")]
        Hz_25M = 25000000,
        [Description("12.5 МГц")]
        Hz_12_5M = 12500000,
        [Description("5 МГц")]
        Hz_5M = 5000000,
        [Description("2.5 МГц")]
        Hz_2_5M = 2500000,
        [Description("1.25 МГц")]
        Hz_1_25M = 1250000,
        [Description("500 кГц")]
        Hz_500K = 500000,
        [Description("250 кГц")]
        Hz_250K = 250000,
        [Description("125 кГц")]
        Hz_125K = 125000,
        [Description("50 кГц")]
        Hz_50K = 50000,
        [Description("25 кГц")]
        Hz_25K = 25000,
        [Description("12.5 кГц")]
        Hz_12_5K = 12500,
        [Description("5 кГц")]
        Hz_5K = 5000,
        [Description("2.5 кГц")]
        Hz_2_5K = 2500,
        [Description("1.25 кГц")]
        Hz_1_25K = 1250,
        [Description("500 Гц")]
        Hz_500 = 500,
        [Description("250 Гц")]
        Hz_250 = 250,
        [Description("125 Гц")]
        Hz_125 = 125,
        [Description("50 Гц")]
        Hz_50 = 50,
        [Description("25 Гц")]
        Hz_25 = 25
    }
}
