#define MAIN_DEBUG

using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class MEAData : StringData
    {
        #region данные конфигурации лабаратории
        public double MinKV { private get; set; }
        public double MaxKV { private get; set; }
        public bool SVI_Power_On { private get; set; }
        public FazeType FazeType { private get; set; }
        #endregion

        private int oldBatteryVoltage;
        public int Step_Faze { get; set; }
        public int Step { get; set; }
        public double Voltage { get; set; } = 99;
        public double Current { get; set; }
        public double BurnCurrent => 100 * (Current / 290.0);
        public double BurnCurrentStep => Step switch
        {
            1 => Current / 290.0 * 34,
            2 => Current / 290.0 * 65,
            3 => Current / 290.0 * 130,
            4 => Current / 290.0 * 260,
            5 => Current / 290.0 * 700,
            6 => Current / 290.0 * 2400,
            7 => Current / 290.0 * 9100,
            _ => 0.0,
        };
        public double BurnVoltage
        {
            get
            {
                var res = Step switch
                {
                    1 => (Voltage - Current * 0.431) * 68.18,
                    2 => (Voltage - Current * 0.431) * 36.36,
                    3 => (Voltage - Current * 0.431) * 18.18,
                    4 => (Voltage - Current * 0.431) * 9.09,
                    5 => (Voltage - Current * 0.788) * 3.4,
                    6 => (Voltage - Current * 0.788) * 1,
                    7 => (Voltage - Current * 0.788) * 0.263,
                    _ => 0
                };
                return res < 0 ? 0 : res;
            }
        }
        public double TempTransformator { get; set; }
        public double PowerOutValue { get; set; }
        public double StableVoltage { get; set; }
        public double StableCurrent { get; set; }
        public double OutRNVoltage { get; set; }
        public double OutRNCurrent { get; set; }
        public int CurrentProtect { get; set; }
        public int FireDelay { get; set; }
        public int CountFire { get; set; }
        public int BatteryVoltage { get; set; }
        public int VoltageType => GetVoltageType(Voltage, StableVoltage);
        public bool ConnectionEnable { get; set; }
        public string ErrorCode1 { get; set; }
        public string ErrorCode2 { get; set; }
        public string ErrorCode3 { get; set; }
        public string ErrorCode4 { get; set; }
        public LabModule Module { get; set; }
        public WorkMode WorkMode { get; set; }
        public GVIFireMode FireMode { get; set; }
        public BurnPower PowerBurn { get; set; }
        public static HVSwitchState HVSwitch1 { get; set; }
        public static HVSwitchState HVSwitch2 { get; set; }
        public static HVSwitchState HVSwitch3 { get; set; }
        public MKZStateInfo MKZState { get; set; }
        public GVIErrorInfo GVIErrorMessage { get; set; }
        public PowerInfo PowerInfoMessage { get; set; }
        public MEAData() { }
        public override void ParseData(string receivedData)
        {
            var opt = OptionalInfo?.Split();
            if (opt is not null)
            {
                MinKV = double.Parse(opt[0]);
                MaxKV = double.Parse(opt[1]);
                SVI_Power_On = bool.Parse(opt[2]);
                FazeType = (FazeType)System.Enum.Parse(typeof(FazeType), opt[3]);
            }
            var vals = receivedData.Trim('\r', '\n')
                                   .Trim()
                                   .Split(',');

            vals[0] = vals[0].Replace("\0", "");
            switch (vals.Length)
            {
                case 4:
                    if (receivedData.StartsWith("HV_SWITCH"))
                    {
                        HVSwitch1 = GetHVSwitch(vals[1]);
                        HVSwitch2 = GetHVSwitch(vals[2]);
                        HVSwitch3 = GetHVSwitch(vals[3]);
                    }
                    else Message = receivedData;  // Данные для калибровки СВИ
                    break;
                case 19:
                    Module = GetCurrentModule(vals[0]);
                    Step_Faze = GetPhase(vals[1]);
                    WorkMode = GetWorkMode(vals[2]);
                    Step = GetStep(vals[3]);
                    Current = Convert.ToDouble(vals[5]);
                    if ((Module == LabModule.HVBurn
                     || Module == LabModule.HVMAC
                     || Module == LabModule.HVMDC
                     || Module == LabModule.JoinBurn)
                        && !SVI_Power_On)
                    {
                        Voltage = 0.0;
                        StableVoltage = 0.0;
                    }
                    else
                    {
                        Voltage = Convert.ToDouble(vals[4]);
                        StableVoltage = Convert.ToDouble(vals[10]);
                    }
                    StableCurrent = Convert.ToDouble(vals[11]);
                    FireMode = GetFireMode(vals[5]);
                    BatteryVoltage = GetBatteryVoltage(Convert.ToInt32(vals[6]));
                    GVIErrorMessage = new GVIErrorInfo(vals[6]);
                    TempTransformator = Convert.ToDouble(vals[6]);
                    PowerOutValue = Convert.ToDouble(vals[7]);
                    FireDelay = Convert.ToInt32(vals[7]);
                    PowerBurn = GetPowerBurn(vals[7]);
                    CurrentProtect = Convert.ToInt32(vals[7]);
                    CountFire = Convert.ToInt32(vals[8]);
                    PowerInfoMessage = new PowerInfo(vals[9]);

                    ConnectionEnable = GetConnectionInfo(vals[12]);
                    ErrorCode1 = vals[12];
                    ErrorCode2 = vals[13];
                    ErrorCode3 = vals[14];
                    ErrorCode4 = vals[15];
                    MKZState = new MKZStateInfo(vals[16], FazeType);
                    OutRNVoltage = Convert.ToDouble(vals[17]);
                    OutRNCurrent = Convert.ToDouble(vals[18]);
                    Message = receivedData;
                    break;
                default:
                    if (receivedData.StartsWith("VOL")) Message = receivedData; // Данные для калибровки СВИ
                    else Message = vals[0];
                    break;
            }
#if MAIN_DEBUG
            Console.WriteLine(receivedData);
#endif
            HVSwitchState GetHVSwitch(string value)
            {
                try
                {
                    value = value[4..];
                    if (!string.IsNullOrEmpty(value)) value = value[..1];
                }
                catch { }

                return value switch
                {
                    "1" => HVSwitchState.HVM,
                    "2" => HVSwitchState.Burn,
                    "3" => HVSwitchState.GVI,
                    "4" => HVSwitchState.Meg,
                    "5" => HVSwitchState.Ground,
                    "0" => HVSwitchState.NoMode,
                    _ => HVSwitchState.Empty,
                };
            }
            LabModule GetCurrentModule(string value)
            {
                return value switch
                {
                    "MAIN" => LabModule.Main,
                    "HVMAC" => LabModule.HVMAC,
                    "HVMDC" => LabModule.HVMDC,
                    "BURN" => LabModule.Burn,
                    "JOINTBURN" => LabModule.JoinBurn,
                    "HVBURN" => LabModule.HVBurn,
                    "HVPULSE" => LabModule.GVI,
                    "MEAS" => LabModule.Meas,
                    "GP500" => LabModule.GP500,
                    "LVM" => LabModule.LVMeas,
                    "Tangent2000" => LabModule.Tangent2000,
                    "VLF" => LabModule.VLF,
                    "SA640" => LabModule.SA640,
                    "SA540_1" => LabModule.SA540_1,
                    "SA540_3" => LabModule.SA540_3,
                    "BRIDGE" => LabModule.Bridge,
                    _ => LabModule.NoMode,
                };
            }
            int GetPhase(string value)
            {
                return value switch
                {
                    "PHASE1" => 1,
                    "PHASE2" => 2,
                    "PHASE3" => 3,
                    _ => 0,
                };
            }
            WorkMode GetWorkMode(string value)
            {
                return value switch
                {
                    "HAND" => WorkMode.Manual,
                    "AUTO" => WorkMode.Auto,
                    _ => WorkMode.NoMode,
                };
            }
            int GetStep(string value)
            {
                return value switch
                {
                    "ST1" => 1,
                    "ST2" => 2,
                    "ST3" => 3,
                    "ST4" => 4,
                    "ST5" => 5,
                    "ST6" => 6,
                    "ST7" => 7,
                    _ => 0,
                };
            }
            GVIFireMode GetFireMode(string value)
            {
                return value switch
                {
                    "1" => GVIFireMode.Single,
                    "2" => GVIFireMode.Multiple,
                    "3" => GVIFireMode.InfinityMultiple,
                    _ => GVIFireMode.NoFireMode,
                };
            }
            int GetBatteryVoltage(int value)
            {
                int range = 0;
                if (oldBatteryVoltage > value)
                {
                    oldBatteryVoltage = value;
                    if (value >= 1300) range = 4;
                    if (value >= 1100) range = 3;
                    if (value >= 850) range = 2;
                    if (value >= 600) range = 1;
                }
                else
                {
                    if (value >= 1450) range = 4;
                    if (value >= 1150) range = 3;
                    if (value >= 900) range = 2;
                    if (value >= 650) range = 1;
                }
                return range;
            }
            BurnPower GetPowerBurn(string value)
            {
                return value switch
                {
                    "50" => BurnPower.Power50,
                    "100" => BurnPower.Power100,
                    _ => BurnPower.NoPower,
                };
            }
            bool GetConnectionInfo(string value)
            {
                int.TryParse(value, out var info);
                return (info & 0x0100) == 0;
            }
        }
        public void ParseParamsToMessage()
        {
            Message = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                GetCurrentModule(Module),
                GetPhase(Step_Faze),
                GetWorkMode(WorkMode),
                GetStep(Step),
                Voltage,
                Current,
                GetFireMode(FireMode),
                0,
                0,
                CountFire,
                0,
                StableVoltage,
                StableCurrent,
                0,
                0,
                0,
                0,
                OutRNVoltage,
                OutRNCurrent
                );
            string GetCurrentModule(LabModule value) => value switch
            {
                LabModule.Main => "MAIN",
                LabModule.HVMAC => "HVMAC",
                LabModule.HVMDC => "HVMDC",
                LabModule.Burn => "BURN",
                LabModule.JoinBurn => "JOINTBURN",
                LabModule.HVBurn => "HVBURN",
                LabModule.GVI => "HVPULSE",
                LabModule.Meas => "MEAS",
                LabModule.GP500 => "GP500",
                LabModule.LVMeas => "LVM",
                LabModule.Tangent2000 => "Tangent2000",
                LabModule.VLF => "VLF",
                LabModule.SA640 => "SA640",
                LabModule.SA540_1 => "SA540_1",
                LabModule.SA540_3 => "SA540_3",
                LabModule.Bridge => "BRIDGE",
                _ => "NoMode",
            };
            string GetPhase(int value) => value switch
            {
                1 => "PHASE1",
                2 => "PHASE2",
                3 => "PHASE3",
                _ => "PHASE0"
            };
            string GetWorkMode(WorkMode value) => value switch
            {
                WorkMode.Manual => "HAND",
                WorkMode.Auto => "AUTO",
                _ => "noone",
            };
            string GetStep(int value) => value switch
            {
                1 => "ST1",
                2 => "ST2",
                3 => "ST3",
                4 => "ST4",
                5 => "ST5",
                6 => "ST6",
                7 => "ST7",
                _ => "ST0",
            };
            int GetFireMode(GVIFireMode value) => value switch
            {
                GVIFireMode.Single => 1,
                GVIFireMode.Multiple => 2,
                GVIFireMode.InfinityMultiple => 3,
                _ => 0
            };
        }



        private int GetVoltageType(double VoltageAC, double VoltageDC)
        {
            // 1 - Переменка; 2 - Постоянка; 3 - Колебания
            int result = 0;
            var cal = Math.Abs((VoltageAC - VoltageDC) / (VoltageAC + VoltageDC));
            if (cal > MaxKV) result = 1;
            if (cal < MinKV) result = 2;
            if (cal >= MinKV && cal <= MaxKV) result = 3;
            return result;
        }
    }
}
