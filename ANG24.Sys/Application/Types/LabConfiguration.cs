using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.Enum;

namespace ANG24.Sys.Application.Types
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LabConfigAttribute : Attribute { } //для новых классов не забывать прописывать этот атрибут

    /* ВАЖНО!
     * IMPORTANT!
     * ATTENTION!
     * 
     * При добавлении новых элементов не забывай добавлять дефолтное значение
     * При добавлении новых классов требуется использование атрибута [LabConfig]
     *  иначе не будет работать автопарсер в словарь значений
     * 
     * Не использовать названия, которые уже есть где-то в конфиге (если они есть в других саб классах конфига)
     */
    public class LabConfiguration
    {
        public string Version { get; set; } = "2.0";
        public int SerialNumber { get; set; } = 0;
        public bool DebugMode { get; set; } = false;
        public bool LVMThreePhaze { get; set; } = false;
        public bool ThermoVision { get; set; } = false;
        /// <summary>
        /// определяет тип лаборатории: 0 -- безфазная(по умолчанию), 1 -- однофазная, 3 -- трехфазная
        /// </summary>
        public FazeType FazeType { get; set; } = 0;
        /// <summary>
        /// Определяет максимальное напряжение в режиме испытаний переменным напряжением [кВ] (по умолчанию -- 0)
        /// </summary>
        public int MaxACVoltage { get; set; } = 0;
        /// <summary>
        /// Определяет максимальное напряжение в режиме испытаний постоянным напряжением [кВ] (по умолчанию -- 0)
        /// </summary>
        public int MaxDCVoltage { get; set; } = 0;
        /// <summary>
        /// Указывает, присутствует ли контроллер компенсации (по умолчанию -- false).
        /// </summary>
        public bool CompensationIsPresent { get; set; } = false;
        /// <summary>
        /// Указывает отображать ли методы прожига (по умолчанию -- false)
        /// </summary>
        public bool MethodsBurn { get; set; }
        /// <summary>
        /// Указывает отображать ручное использование ИДМ (по умолчанию -- false)
        /// </summary>
        public bool ManualIDM { get; set; } = false;
        public bool IsMDPOn { get; set; } = false;
        /// <summary>
        /// Определяет тип протокола (По умолчанию default);
        /// </summary>
        public ProtocolType Protocol { get; set; } = ProtocolType.Default;
        public IList<LabModule> LabComposition { get; set; } = new List<LabModule>();
        public IList<LabController> LabControllers { get; set; } = new List<LabController>();
        [LabConfig]
        public Params Parameters { get; set; } = new Params();
        [LabConfig]
        public Options OptionalParams { get; set; } = new Options();
        [LabConfig]
        public ExternalSoftwareParams ExternalSoftwareParameters { get; set; } = new ExternalSoftwareParams();
        public class Options
        {
            public bool SVICalib { get; set; }
            public bool ReflectAsIntegratedModule { get; set; }
            public bool AutoBurnIsOn { get; set; }
            public bool AutoRise { get; set; }
            public bool ObserveIDM { get; set; }
        }
        public class Params
        {
            public double MinKV { get; set; } = 0.4;
            public double MaxKV { get; set; } = 0.6;
            public double HVPULSESTEP1 { get; set; } = 1.12;
            public double HVPULSESTEP2 { get; set; } = 2.28;
            public double HVPULSESTEP3 { get; set; } = 4.96;
            public double KJSTEP1 { get; set; } = 1.12;
            public double KJSTEP2 { get; set; } = 2.28;
            public double KJSTEP3 { get; set; } = 4.96;
            public double ReflectCalibKoefKU { get; set; } = 1;
            [LabConfig]
            public CurrentProtected CurrentProtectedParams { get; set; } = new CurrentProtected();
            [LabConfig]
            public VoltageProtected VoltageProtectedParams { get; set; } = new VoltageProtected();
            [LabConfig]
            public AutoBurn AutoBurnParams { get; set; } = new AutoBurn();
            public class CurrentProtected
            {
                /// <summary>
                /// Текст кнопки "защита по току" макс
                /// </summary>
                public short CurrentProtectText { get; set; } = 100;
                public int MaxCurrentRN { get; set; } = 260;
                public int MaxCurrentRN_BURN { get; set; } = 260;
                public int MaxCurrentRN_HVBURN { get; set; } = 260;
                public int MaxCurrentRN_HVPULSE { get; set; } = 260;
                public int MaxCurrentRN_JOINTBURN { get; set; } = 260;
                public int MaxCurrentRN_HVMDCHi { get; set; } = 260;
            }
            public class VoltageProtected
            {
                public int MaxVoltageRN { get; set; } = 220;
                public int MaxVoltageRN_BURN { get; set; } = 220;
                public int MaxVoltageRN_HVBURN { get; set; } = 220;
                public int MaxVoltageRN_HVPULSE { get; set; } = 220;
                public int MaxVoltageRN_JOINTBURN { get; set; } = 220;
                public int MaxVoltageRN_HVMDCHi { get; set; } = 220;
                public int MaxVoltageRN_LVM { get; set; } = 220;
            }
            public class AutoBurn
            {
                public int BurnTime { get; set; } = 15;
                public int BurnIStepUpPercent { get; set; } = 40;
                public int BurnIStepDownPercent { get; set; } = 10;
                public int BurnVlotagePercent { get; set; } = 50;
            }
        }
        public class ExternalSoftwareParams
        {
            [LabConfig]
            public Paths SoftwarePaths { get; set; } = new Paths();
            [LabConfig]
            public Mods SoftwareMods { get; set; } = new Mods();
            public class Paths
            {
                public string SA7100SoftFilePath { get; set; } = "C:\\Program Files\\CA7100\\hvbridge7100.exe";
                public string SA540SoftFilePath { get; set; } = @"C:\Program Files (x86)\CA540_ETL\CA540_ETL.exe";
                public string SA640SoftFilePath { get; set; } = @"C:\Program Files (x86)\CA640_ETL\CA640_ETL.exe";
                public string VLFSoftFilePath { get; set; } = @"C:\b2CC v3.65 ServicePartner\ControlCenter.App.exe";
                public string LVMeasSoftFilePath { get; set; } = "";
                public string MeasSoftFilePath { get; set; } = "";
            }
            public class Mods
            {
                public SoftMode SA7100Mode { get; set; } = SoftMode.External;
                public SoftMode SA540Mode { get; set; } = SoftMode.External;
                public SoftMode SA640Mode { get; set; } = SoftMode.External;
                public SoftMode VLFMode { get; set; } = SoftMode.External;
                public SoftMode LVMeasMode { get; set; } = SoftMode.None;
                public SoftMode MeasMode { get; set; } = SoftMode.None;
                public enum SoftMode
                {
                    None,
                    External,
                    Internal
                }
            }
        }
    }
}
