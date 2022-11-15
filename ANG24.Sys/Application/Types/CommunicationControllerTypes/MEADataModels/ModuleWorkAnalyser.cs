using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace Application.Types.CommunicationControllerTypes.MEADataModels
{
    public class ModuleWorkAnalyser
    {
        public ModulePowerState WorkState { get; set; }
        private List<bool> TargetFlags { get; set; }
        private bool MainTarget { get; set; }
        public ModuleWorkAnalyser(LabModule CurrentModule, PowerInfo powerInfo)
        {
            switch (CurrentModule)
            {
                case LabModule.Bridge:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.BridgeEnable,
                        powerInfo.HVMEnable
                    };
                    MainTarget = powerInfo.BridgeEnable;
                    break;
                case LabModule.Burn:
                    TargetFlags = new List<bool>
                   {
                       powerInfo.BurnEnable,
                       powerInfo.MVKUp,
                       powerInfo.VREnable
                   };
                    MainTarget = powerInfo.BurnEnable;
                    break;
                case LabModule.GP500:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.MVKUp,
                        powerInfo.GP500Enable,
                        powerInfo.KM3_MKZEnable
                    };
                    MainTarget = powerInfo.GP500Enable;
                    break;
                case LabModule.GVI:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.MVKUp,
                        powerInfo.GVIEnable,
                        powerInfo.VREnable
                    };
                    MainTarget = powerInfo.GVIEnable;
                    break;
                case LabModule.HVBurn:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.MVKUp,
                        powerInfo.SVIPowerEnable,
                        powerInfo.ProtectedDrosselEnable,
                        powerInfo.VREnable,
                        powerInfo.HVMEnable
                    };
                    MainTarget = powerInfo.HVMEnable;
                    break;
                case LabModule.HVMAC:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.HVMEnable,
                        powerInfo.SVIPowerEnable,
                        powerInfo.VREnable,
                        powerInfo.KM1_MKZEnable
                    };
                    MainTarget = powerInfo.HVMEnable;
                    break;
                case LabModule.HVMDC:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.HVMEnable,
                        powerInfo.MVKUp,
                        powerInfo.SVIPowerEnable,
                        powerInfo.VREnable,
                        powerInfo.KM1_MKZEnable
                    };
                    MainTarget = powerInfo.HVMEnable;
                    break;
                case LabModule.JoinBurn:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.MVKUp,
                        powerInfo.SVIPowerEnable,
                        powerInfo.MSKEnable,
                        powerInfo.ProtectedDrosselEnable,
                        powerInfo.VREnable,
                        powerInfo.HVMEnable,
                        powerInfo.JoinBurnEnable
                    };
                    MainTarget = powerInfo.JoinBurnEnable;
                    break;
                case LabModule.LVMeas:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.LVMEnable,
                        powerInfo.VREnable
                    };
                    MainTarget = powerInfo.LVMEnable;
                    break;
                case LabModule.Meas:
                    TargetFlags = new List<bool>
                    {
                        powerInfo.MVKUp,
                        powerInfo.MeasEnable
                    };
                    MainTarget = powerInfo.MeasEnable;
                    break;
                default:
                    TargetFlags = new List<bool>();
                    MainTarget = false;
                    break;
            }
            if (!TargetFlags.Contains(false)) WorkState = ModulePowerState.Enable;
            else if (MainTarget == true) WorkState = ModulePowerState.EnableFail;
            else WorkState = ModulePowerState.Disable;
        }

    }
}
