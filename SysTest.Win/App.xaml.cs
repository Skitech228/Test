using ANG24.Sys;
using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SysTest.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        IContainer? container;
       
        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<SysModule>();
            builder.RegisterType<MainWindowViewModel>().SingleInstance();
            builder.RegisterType<MainWindow>().SingleInstance();

            container = builder.Build();

            MainWindow = container.Resolve<MainWindow>();
            MainWindow.Show();
            
        }
    }
}
