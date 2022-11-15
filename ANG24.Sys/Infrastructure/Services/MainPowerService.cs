using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using System.Diagnostics;
using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using Application.Types.CommunicationControllerTypes.MEADataModels;

namespace ANG24.Sys.Infrastructure.Services
{
    public sealed class MainPowerService : Executer, IMainPowerService
    {
        private readonly IMainControllerOperator service;
        private readonly INotificationService message;
        private LabModule _currentModule;
        private LabModule oldModule = LabModule.Main;
        private List<PowerTargets> powerTargets;
        private StartVisualState _startVisualState;
        private bool _workState;
        private MEAData lastData;

        public event Action<StartVisualState> StartStateChanged;
        public event Action<bool> WorkStateChanged;
        public LabModule CurrentModule
        {
            get => _currentModule;
            set
            {
                _currentModule = value;
                if (oldModule != value)
                {
                    powerTargets = new List<PowerTargets>();
                    switch (value)
                    {
                        case LabModule.HVMAC:
                            powerTargets.Add(PowerTargets.SVI);
                            break;

                        case LabModule.HVMDC:
                        case LabModule.HVBurn:
                            powerTargets.Add(PowerTargets.SVI);
                            powerTargets.Add(PowerTargets.MVK);
                            break;

                        case LabModule.JoinBurn:
                            powerTargets.Add(PowerTargets.SVI);
                            powerTargets.Add(PowerTargets.MVK);
                            powerTargets.Add(PowerTargets.MSK);
                            break;

                        case LabModule.Meas:
                        case LabModule.Burn:
                        case LabModule.GP500:
                        case LabModule.GVI:
                            powerTargets.Add(PowerTargets.MVK);
                            break;

                        default:
                            powerTargets.Add(PowerTargets.ModulePower);
                            break;
                    }
                    StartState = StartVisualState.Normal;
                    oldModule = value;
                }
            }
        }
        public bool WorkState
        {
            get => _workState;
            set
            {
                _workState = value;
                WorkStateChanged?.Invoke(_workState);
            }
        }
        public StartVisualState StartState
        {
            get => _startVisualState;
            set => StartStateChanged?.Invoke(_startVisualState = value);
        }
        public LabModule DisconnectedModule { get; private set; }
        public MainPowerService(IMainControllerOperator service,
                                INotificationService message,
                                Autofac.ILifetimeScope container) : base(container)
        {
            this.service = service;
            this.message = message;
            //HV Positions Init
            HVPositions.HVPosition1 = MEAData.HVSwitch1;
            HVPositions.HVPosition2 = MEAData.HVSwitch2;
            HVPositions.HVPosition3 = MEAData.HVSwitch3;
            //Main Init
            service.ModuleStateChanged += Service_ModuleStateChanged;
            service.OnDataReceived += Service_OnDataReceived;
        }
        private void Service_OnDataReceived(MEAData data)
        {
            if (data.Module != LabModule.NoMode)
                lastData = data;
            if (CurrentModule != data.Module)
            {
                CurrentModule = lastData.Module;
                CheckedModulePower(CurrentModule);
            }
            DisconnectedModule = lastData.Module;
            HVPositions.HVPosition1 = MEAData.HVSwitch1;
            HVPositions.HVPosition2 = MEAData.HVSwitch2;
            HVPositions.HVPosition3 = MEAData.HVSwitch3;

            void CheckedModulePower(LabModule CurrentModule)
            {
                if (DisconnectedModule == CurrentModule || DisconnectedModule == LabModule.NoMode)
                {
                    WorkState = false;
                    switch (CurrentModule)
                    {
                        case LabModule.HVMAC:
                            if (lastData.PowerInfoMessage.ModulePower.HVMACModuleEnable == ModulePowerState.Enable &&
                                lastData.PowerInfoMessage.ModulePower.HVBurnModuleEnable != ModulePowerState.Enable &&
                                lastData.PowerInfoMessage.ModulePower.JoinBurnModuleEnable != ModulePowerState.Enable
                                )
                                WorkState = true;
                            break;
                        case LabModule.HVMDC:
                            if (lastData.PowerInfoMessage.ModulePower.HVMDCModuleEnable == ModulePowerState.Enable &&
                                lastData.PowerInfoMessage.ModulePower.HVBurnModuleEnable != ModulePowerState.Enable &&
                                lastData.PowerInfoMessage.ModulePower.JoinBurnModuleEnable != ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.HVMDCHi:

                            break;
                        case LabModule.HVBurn:
                            if (lastData.PowerInfoMessage.ModulePower.HVBurnModuleEnable == ModulePowerState.Enable &&
                                lastData.PowerInfoMessage.ModulePower.JoinBurnModuleEnable != ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.Burn:
                            if (lastData.PowerInfoMessage.ModulePower.BurnModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.JoinBurn:
                            //if (ControllerData.PowerInfoMessage.ModulePower.JoinBurnModuleEnable == ModulePowerState.Enable)
                            WorkState = true;
                            break;
                        case LabModule.Bridge:
                            if (lastData.PowerInfoMessage.ModulePower.BridgeModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.GVI:
                            if (lastData.PowerInfoMessage.ModulePower.GVIModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.GP500:
                            if (lastData.PowerInfoMessage.ModulePower.GP500ModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.LVMeas:
                            if (lastData.PowerInfoMessage.ModulePower.LVMModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.Meas:
                            if (lastData.PowerInfoMessage.ModulePower.MeasureModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.Tangent2000:
                            if (lastData.PowerInfoMessage.ModulePower.Tangent2000Enable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.VLF:
                            if (lastData.PowerInfoMessage.ModulePower.VLFModuleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.SA540:
                        case LabModule.SA540_1:
                        case LabModule.SA540_3:
                            if (lastData.PowerInfoMessage.ModulePower.SA540MoudleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                        case LabModule.SA640:
                            if (lastData.PowerInfoMessage.ModulePower.SA640MoudleEnable == ModulePowerState.Enable)
                                WorkState = true;
                            break;
                    }
                    DisconnectedModule = LabModule.NoMode;
                }
            }
        }
        private void Service_ModuleStateChanged(bool started, int ExitCode)
        {
            var local = started && ExitCode == 0;
            WorkState = local;
            StartState = StartVisualState.Normal;
        }
        public void Run()
        {
            if (WorkState)
            {
                Debug.WriteLine("Stop Module...");
                service.PowerOff();
                StartState = StartVisualState.IsProgressed;
            }
            else
                message.SendNotificationOKCancel("Убедитесь, что все кабели размотаны, заземление надежно закреплено. Не соблюдение этих мер может вывести из строя ЭТЛ. Продолжить?", () =>
                {
                    Debug.WriteLine("Start Module...");
                    service.PowerOn(powerTargets ?? throw new ArgumentNullException());
                    StartState = StartVisualState.IsProgressed;
                }, null);
        }
    }
}
