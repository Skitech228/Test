using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Communication.Operators.ControllerOperators;
using ANG24.Sys.Communication.Operators;
using ANG24.Sys.Infrastructure.Services;
using Autofac.Core;
using Autofac.Core.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ANG24.Sys.Infrastructure;
using ANG24.Sys.Communication;

namespace ANG24.Sys
{
    public class SysModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<LabServicesConfigurator>();
            builder.RegisterModule<ControllersConfigurator>();
            base.Load(builder);
        }
    }
}
