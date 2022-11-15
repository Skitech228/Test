using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace Application.Types.CommunicationControllerTypes.MEADataModels
{
    public class ModulePowerInfo
    {
        public ModulePowerState BurnModuleEnable { get; set; }
        public ModulePowerState HVMDCModuleEnable { get; set; }
        public ModulePowerState HVMACModuleEnable { get; set; }
        public ModulePowerState GVIModuleEnable { get; set; }
        public ModulePowerState GP500ModuleEnable { get; set; }
        public ModulePowerState MeasureModuleEnable { get; set; }
        public ModulePowerState JoinBurnModuleEnable { get; set; }
        public ModulePowerState HVBurnModuleEnable { get; set; }
        public ModulePowerState LVMModuleEnable { get; set; }
        public ModulePowerState BridgeModuleEnable { get; set; }
        public ModulePowerState SA540MoudleEnable { get; set; }
        public ModulePowerState SA640MoudleEnable { get; set; }
        public ModulePowerState VLFModuleEnable { get; set; }
        public ModulePowerState Tangent2000Enable { get; set; }

        public ModulePowerInfo(PowerInfo info)
        {
            BurnModuleEnable = new ModuleWorkAnalyser(LabModule.Burn, info).WorkState;
            HVMDCModuleEnable = new ModuleWorkAnalyser(LabModule.HVMDC, info).WorkState;
            HVMACModuleEnable = new ModuleWorkAnalyser(LabModule.HVMAC, info).WorkState;
            GVIModuleEnable = new ModuleWorkAnalyser(LabModule.GVI, info).WorkState;
            GP500ModuleEnable = new ModuleWorkAnalyser(LabModule.GP500, info).WorkState;
            MeasureModuleEnable = new ModuleWorkAnalyser(LabModule.Meas, info).WorkState;
            JoinBurnModuleEnable = new ModuleWorkAnalyser(LabModule.JoinBurn, info).WorkState;
            HVBurnModuleEnable = new ModuleWorkAnalyser(LabModule.HVBurn, info).WorkState;
            LVMModuleEnable = new ModuleWorkAnalyser(LabModule.LVMeas, info).WorkState;
            BridgeModuleEnable = new ModuleWorkAnalyser(LabModule.Bridge, info).WorkState;
            SA540MoudleEnable = new ModuleWorkAnalyser(LabModule.SA540_1, info).WorkState;
            SA540MoudleEnable = new ModuleWorkAnalyser(LabModule.SA540_3, info).WorkState;
            SA640MoudleEnable = new ModuleWorkAnalyser(LabModule.SA640, info).WorkState;
            VLFModuleEnable = new ModuleWorkAnalyser(LabModule.VLF, info).WorkState;
            SA540MoudleEnable = new ModuleWorkAnalyser(LabModule.SA540, info).WorkState;
            SA640MoudleEnable = new ModuleWorkAnalyser(LabModule.SA640, info).WorkState;
            Tangent2000Enable = new ModuleWorkAnalyser(LabModule.Tangent2000, info).WorkState;

        }
    }
}
