using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.Enum;
using ANG24.Sys.Communication.Interfaces;
using Autofac;

namespace ANG24.Sys.Communication.Operators
{
    public sealed class ControllerOperatorFactory
    {
        private readonly ControllerFinder finder;
        private readonly ILifetimeScope provider;

        public ControllerOperatorFactory(ILifetimeScope provider, ControllerFinder finder)
        {
            this.finder = finder;
            this.provider = provider;
        }

        public IControllerOperator<IControllerOperatorData> Create(LabController name)
        {
            IControllerOperator<IControllerOperatorData> controllerOperator = name switch
            {
                LabController.MainController => provider.Resolve<IMainControllerOperator>(),
                LabController.Reflect => provider.Resolve<IReflectOperator>(),
                LabController.SA540 => provider.Resolve<ISA540ControllerOperator>(),
                LabController.SA640 => provider.Resolve<ISA640ControllerOperator>(),
                LabController.SA7100 => provider.Resolve<ISA7100ControllerOperator>(),
                LabController.BridgeCommutator => provider.Resolve<IBridgeCommutatorOperator>(),
                LabController.Compensation => provider.Resolve<ICompensationControllerOperator>(),
                LabController.PowerSelector => provider.Resolve<IPowerSelectorOperator>(),
                LabController.VoltageSyncronizer => provider.Resolve<IVoltageSyncroOperator>(),
                LabController.VLF => provider.Resolve<IVoltageRegulatorOperator>(),
                LabController.GP500 => provider.Resolve<IGP500ControllerOperator>(),
                LabController.ThermoVision => provider.Resolve<IThermoVisionControlManager>(),
                _ => null
            };
            if (controllerOperator != null && controllerOperator is ISerialPortOperator)
                (controllerOperator as ISerialPortOperator).SetPort(finder.FindPort(name.ToString()));
            return controllerOperator;
        }
    }

}
