using ANG24.Sys.Communication.Connections;
using Autofac;
using GalaSoft.MvvmLight;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SysTest.Win.AsyncConmands;
using SysTest.Win.EntityService;
using SysTest.Win.ViewModel;

namespace SysTest.Win
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region Propertys

        private PortViewModel _Ports;
        public PortViewModel PortsContext
        {
            get => _Ports;
            set { Set(() => PortsContext, ref _Ports, value); }
        }
        #endregion
        public MainWindowViewModel(IPortService port)
        {        
            PortsContext = new PortViewModel(port);
        }
    }
}
