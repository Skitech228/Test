using ANG24.Sys.Infrastructure.DBRepositories;
using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Infrastructure.Services;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Autofac;

namespace ANG24.Sys.Infrastructure
{
    public sealed class LabServicesConfigurator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LabConfigurationService>().As<ILabConfigurationService>().SingleInstance(); // конфигурация лабаратории (бывший labConfig)
            builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();
            builder.RegisterType<MainPowerService>().As<IMainPowerService>().SingleInstance();
            builder.RegisterType<TimeWorkService>().As<ITimeWorkService>().SingleInstance();
            builder.RegisterType<Autoburn>().As<IAutoburner>().SingleInstance();
            builder.RegisterType<SVICalibrationService>().As<ISVICalibrationService>().SingleInstance();

            builder.RegisterType<CustomerRepository>().As<ICustomerRepository>();
            builder.RegisterType<DeviceGroupRepository>().As<IDeviceGroupRepository>();
            builder.RegisterType<DeviceParameterRepository>().As<IDeviceParameterRepository>();
            builder.RegisterType<DeviceRepository>().As<IDeviceRepository>();
            builder.RegisterType<EnergyObjectRepository>().As<IEnergyObjectRepository>();
            builder.RegisterType<FazeMeteringResultRepository>().As<IFazeMeteringResultRepository>();
            builder.RegisterType<FazeRepository>().As<IFazeRepository>();
            builder.RegisterType<LogDataRepository>().As<ILogDataRepository>();
            builder.RegisterType<ModuleRepository>().As<IModuleRepository>();
            builder.RegisterType<OrderRepository>().As<IOrderRepository>();
            builder.RegisterType<OrderTypeRepository>().As<IOrderTypeRepository>();
            builder.RegisterType<ParameterAdditionRepository>().As<IParameterAdditionRepository>();
            builder.RegisterType<PrefixRepository>().As<IPrefixRepository>();
            builder.RegisterType<ResultValueRepository>().As<IResultValueRepository>();
            builder.RegisterType<TestObjectRepository>().As<ITestObjectRepository>();
            builder.RegisterType<TestTargetRepository>().As<ITestTargetRepository>();
            builder.RegisterType<UnitRepository>().As<IUnitRepository>();
            builder.RegisterType<UserRepository>().As<IUserRepository>();

            builder.RegisterType<WindowsProcessCreator>().As<IProcessCreator>().SingleInstance();
            builder.RegisterType<DebugMenu>().As<IDebugMenu>().SingleInstance();
        }
    }
}
