// See https://aka.ms/new-console-template for more information
using ANG24.Sys;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using Autofac;

internal class Program
{

    static IContainer? container;
    private static void Main(string[] args)
    {
        var builder = new ContainerBuilder();

        builder.RegisterModule<SysModule>();
        container = builder.Build();
        using (var scope = container.BeginLifetimeScope())
        {
            var oper = scope.Resolve<IControllerOperatorComposer>();
            
            oper.SearchForControllers();
        }

    }
}