using ANG24.Sys;
using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Prism.Ioc;
using Prism.Unity;
using System.Threading.Tasks;
using System.Windows;
using SysTest.Win.Database;
using SysTest.Win.EntityService;

namespace SysTest.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    { 
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ApplicationContext>(() =>
                                                                    {
                                                                        var context = new ApplicationContext();

                                                                        return context;
                                                                    });

            containerRegistry.RegisterScoped<IPortService, PortService>();
        }
        protected override Window CreateShell() => Container.Resolve<MainWindow>();
    }
}
