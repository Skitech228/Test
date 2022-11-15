using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Communication.Helpers;
using ANG24.Sys.Communication.Operators;
using ANG24.Sys.Communication.Operators.AbstractOperators;
using ANG24.Sys.Communication.Operators.ControllerOperators;
using Autofac;

namespace ANG24.Sys.Communication
{
    public sealed class ControllersConfigurator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register((c) => ControllerFinder.CreateNew(c.Resolve<INotificationService>()).Result).SingleInstance();

            builder.RegisterType<MEADirector>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MainControllerSimulator>().Named<IMainControllerOperator>("Simulation").SingleInstance();
            builder.RegisterType<MainControllerOperator>().Named<IMainControllerOperator>("Real").SingleInstance();


            //builder.RegisterType<SA540ControllerOperator>().As<ISA540ControllerOperator>().SingleInstance();
            //builder.RegisterType<SA640ControllerOperator>().As<ISA640ControllerOperator>().SingleInstance();
            //builder.RegisterType<SA7100ControllerOperator>().As<ISA7100ControllerOperator>().SingleInstance();
            builder.RegisterType<GP500ControllerOperator>().As<IGP500ControllerOperator>().SingleInstance();
            builder.RegisterType<ThermoVisionControlManager>().As<IThermoVisionControlManager>().SingleInstance();
            builder.RegisterType<ReflectOperator>().As<IReflectOperator>().SingleInstance();
            builder.RegisterType<CompensationControllerOperator>().As<ICompensationControllerOperator>().SingleInstance();
            builder.RegisterType<VoltageRegulatorOperator>().As<IVoltageRegulatorOperator>().SingleInstance();
            builder.RegisterType<BridgeCommutatorOperator>().As<IBridgeCommutatorOperator>().SingleInstance();
            builder.RegisterType<PowerSelectorOperator>().As<IPowerSelectorOperator>().SingleInstance();
            builder.RegisterType<VoltageSyncroOperator>().As<IVoltageSyncroOperator>().SingleInstance();


            builder.RegisterType<ControllerOperatorFactory>();
            builder.RegisterType<ControllerOperatorComposer>().As<IControllerOperatorComposer>().SingleInstance().AutoActivate();

            builder.RegisterType<PowerProtectionSystem>().As<IPowerProtectionSystem>().SingleInstance().AutoActivate();
            builder.RegisterType<LabVoltageSafeguard>().As<ILabVoltageSafeguard>().SingleInstance().AutoActivate();
        }
    }
}
