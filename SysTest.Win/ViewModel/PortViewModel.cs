#region Using derectives

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ANG24.Sys.Communication.Connections;
using GalaSoft.MvvmLight;
using Prism.Commands;
using SysTest.Win.AsyncConmands;
using SysTest.Win.Database.Entity;
using SysTest.Win.EntityService;
using static Unity.Storage.RegistrationSet;

#endregion

namespace SysTest.Win.ViewModel
{
    public class PortViewModel : ViewModelBase
    {
        #region Propertys
        private readonly IPortService _portService;
        private bool _isEditMode;  
        private ObservableCollection<TCPController> _portsTCP;
        private ObservableCollection<BTController> _portsBT;
        private ObservableCollection<PortEntity> _ports;
        private PortEntity? _selectedPort;
        private DelegateCommand _addPortCommand;
        private AsyncRelayCommand _removePortRelayCommand;
        private AsyncRelayCommand _applyPortChangesRelayCommand;
        private DelegateCommand _changeEditModeCommand;
        private AsyncRelayCommand _reloadPortsRelayCommand;
        private string _elementsVisibility;

                public ObservableCollection<PortEntity> Ports
        {
            get => _ports;
            set => Set(ref _ports, value);
        }

        private string _actionSuccessVisibility;

        public string ActionSuccessVisibility
        {
            get => _actionSuccessVisibility;
            set => Set(ref _actionSuccessVisibility, value);
        }

        private string _actionSuccess;

        public string ActionSuccess
        {
            get => _actionSuccess;
            set => Set(ref _actionSuccess, value);
        }

                private string _commandSuccessVisibility;

        public string CommandSuccessVisibility
        {
            get => _commandSuccessVisibility;
            set => Set(ref _commandSuccessVisibility, value);
        }

        private string _commandSuccess;

        public string CommandSuccess
        {
            get => _commandSuccess;
            set => Set(ref _commandSuccess, value);
        }

        private string? _comandReqest;

        public string? ComandReqest
        {
            get => _comandReqest;
            set => Set(ref _comandReqest, value);
        }

        private string? _selectedElementComandReqest;

        public string? SelectedElementComandReqest
        {
            get => _selectedElementComandReqest;
            set => Set(ref _selectedElementComandReqest, value);
        }

        private string _selectedProtocol;

        public string SelectedProtocol
        {
            get => _selectedProtocol;
            set => Set(ref _selectedProtocol, value);
        }

        private bool _isDisconnected;

        public bool IsDisconnected
        {
            get => _isDisconnected;
            set => Set(ref _isDisconnected, value);
        }

        private string _inputPort;

        public string InputPort
        {
            get => _inputPort;
            set => Set(ref _inputPort, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => Set(ref _isEditMode, value);
        }

        public string ElementsVisibility
        {
            get => _elementsVisibility;
            set => Set(ref _elementsVisibility, value);
        }

        public PortEntity? SelectedPort
        {
            get 
            { 
                return _selectedPort; 
            }
            set
            {
                Set(ref _selectedPort, value);
                if(SelectedPort!=null)
                    {
                        ElementsVisibility="Visible";
                        if(SelectedPort.Entity.IsConnected)
                            IsDisconnected=false;
                        else
                            IsDisconnected=true;
                    }
            }
        }
        #endregion
        public PortViewModel(IPortService portService)
        {
            _portService = portService;
            Ports = new ObservableCollection<PortEntity>();
            Dispatcher.CurrentDispatcher.InvokeAsync(async () => await ReloadPortsAsync());
            ElementsVisibility = "Hidden";
            SelectedElementComandReqest=String.Empty;
            ComandReqest=String.Empty;
            SelectedProtocol = "TCP";
            ActionSuccessVisibility="Hidden";
            CommandSuccessVisibility = "Hidden";
            _portsTCP = new ObservableCollection<TCPController>();
            _portsBT = new ObservableCollection<BTController>();
        }

        #region Commands

        private AsyncRelayCommand _chandeSelectedProtocolToBTCommand;
        public AsyncRelayCommand ChandeSelectedProtocolToBT => _chandeSelectedProtocolToBTCommand ??= new AsyncRelayCommand(OnChandeSelectedProtocolToBTCommandExecuted);

        private async Task OnChandeSelectedProtocolToBTCommandExecuted()
        {
            if(SelectedProtocol!="BT")
            {
                SelectedProtocol="BT";
                await GetBTPortsAsync();
            }
        }

        private AsyncRelayCommand _chandeSelectedProtocolToTCPCommand;
        public AsyncRelayCommand ChandeSelectedProtocolToTCP => _chandeSelectedProtocolToTCPCommand ??= new AsyncRelayCommand(OnChandeSelectedProtocolToTCPCommandExecuted);

        private async Task OnChandeSelectedProtocolToTCPCommandExecuted()
        {
            SelectedProtocol="TCP";
            await GetTCPPortsAsync();
        }

        private AsyncRelayCommand _chandeSelectedProtocolToConnectedCommand;
        public AsyncRelayCommand ChandeSelectedProtocolToConnected => _chandeSelectedProtocolToConnectedCommand ??= new AsyncRelayCommand(OnChandeSelectedProtocolToConnectedCommandExecuted);

        private async Task OnChandeSelectedProtocolToConnectedCommandExecuted()
        {
            await GetConnectedPortsAsync();
        }

        private AsyncRelayCommand _chandeSelectedProtocolToAllCommand;
        public AsyncRelayCommand ChandeSelectedProtocolToAll => _chandeSelectedProtocolToAllCommand ??= new AsyncRelayCommand(OnChandeSelectedProtocolToAllCommandExecuted);

        private async Task OnChandeSelectedProtocolToAllCommandExecuted()
        {
            SelectedProtocol="TCP";
            await ReloadPortsAsync();
        }

        private AsyncRelayCommand _setCommand;
        public AsyncRelayCommand SetCommand => _setCommand ??= new AsyncRelayCommand(OnSetCommandExecuted);

        private async Task OnSetCommandExecuted()
        {
            if(InputPort!=null)
            {                
                if(Ports.Where(x=>x.Entity.HostName==InputPort/*.Split(':')[0]).Count()!=0*/).Count()!=0 &&
                    Ports.Where(x=>x.Entity.HostName==InputPort).First().Entity.IsConnected==true)
                {
                    var port=Ports.Where(x=>x.Entity.HostName==InputPort).First();
                        if(port.Entity.Protocol=="TCP")
                        {
                            _portsTCP.Where(x=>x.HostName==port.Entity.HostName &&
                                        x.PortNum==port.Entity.PortNum &&
                                        x.StringEndByte==port.Entity.StringEndByte).Last().SetCommand(ComandReqest);
                        }
                        else
                        {
                            _portsBT.Where(x=>x.HostName==port.Entity.HostName &&
                                        x.PortNum==port.Entity.PortNum &&
                                        x.StringEndByte==port.Entity.StringEndByte).Last().SetCommand(ComandReqest);
                        }
                        CommandSuccess="Успешно";
                        CommandSuccessVisibility="Visible";
                        await Task.Run(() =>
                        {
                            Thread.Sleep(2500);
                            CommandSuccessVisibility = "Hidden";
                        });
                } 
                else
                {
                    CommandSuccess="Нет порта с таким названием или порт не подключён";
                    CommandSuccessVisibility="Visible";
                    await Task.Run(() =>
                                        {
                                            Thread.Sleep(2500);
                                            CommandSuccessVisibility = "Hidden";
                                        });
                }
            }
            else
            { 
                CommandSuccess="Введите название порта";
                CommandSuccessVisibility="Visible";
                await Task.Run(() =>
                {
                    Thread.Sleep(2500);
                    CommandSuccessVisibility = "Hidden";
                });
            }
        }

        private AsyncRelayCommand _setSelectedPortCommand;
        public AsyncRelayCommand SetSelectedPortCommand => _setSelectedPortCommand ??= new AsyncRelayCommand(OnSetSelectedPortCommandExecuted);

        private async Task OnSetSelectedPortCommandExecuted()
        {
            if(SelectedPort.Entity.Protocol=="TCP")
            {
                _portsTCP.Where(x=>x.HostName==SelectedPort.Entity.HostName &&
                            x.PortNum==SelectedPort.Entity.PortNum &&
                            x.StringEndByte==SelectedPort.Entity.StringEndByte).Last().SetCommand(SelectedElementComandReqest);
            }
            else
            {
                _portsBT.Where(x=>x.HostName==SelectedPort.Entity.HostName &&
                            x.PortNum==SelectedPort.Entity.PortNum &&
                            x.StringEndByte==SelectedPort.Entity.StringEndByte).Last().SetCommand(SelectedElementComandReqest);
            }
            ActionSuccess="Успешно";
            ActionSuccessVisibility="Visible";
            await Task.Run(() =>
            {
                Thread.Sleep(2500);
                ActionSuccessVisibility = "Hidden";
            });
        }

        private AsyncRelayCommand _appCloseCommand;
        public AsyncRelayCommand AppClose => _appCloseCommand ??= new AsyncRelayCommand(OnAppCloseCommandExecuted);

        private async Task OnAppCloseCommandExecuted()
        {
            try
            {
            foreach (var item in _portsTCP)
            {
                try
                {
                var port=Ports.Where(x=>x.Entity.HostName==item.HostName &&
                            x.Entity.PortNum==item.PortNum &&
                            x.Entity.StringEndByte==item.StringEndByte).Last();
                port.Entity.IsConnected=false;
                await _portService.UpdateAsync(port.Entity);
                _portsTCP.Remove(item);
                } catch(Exception e) {}
            }
            foreach (var item in _portsBT)
            {
                try{
                var port=Ports.Where(x=>x.Entity.HostName==item.HostName &&
                            x.Entity.PortNum==item.PortNum &&
                            x.Entity.StringEndByte==item.StringEndByte).Last();
                port.Entity.IsConnected=false;
                await _portService.UpdateAsync(port.Entity);
                _portsBT.Remove(item);
                }catch(Exception e){}
            }
            }
            catch{App.Current.Shutdown();}
            App.Current.Shutdown();
        }

        private AsyncRelayCommand _connectToPortCommand;
        public AsyncRelayCommand ConnectToPort => _connectToPortCommand ??= new AsyncRelayCommand(OnConnectToPortCommandExecuted);

        private async Task OnConnectToPortCommandExecuted()
        {
            if(SelectedPort.Entity.Protocol=="TCP")
            {
                _portsTCP.Add(new TCPController()
                {
                    HostName=SelectedPort.Entity.HostName,
                    PortNum=(ushort)SelectedPort.Entity.PortNum,
                    StringEndByte=(byte)SelectedPort.Entity.StringEndByte
                });
                if(_portsTCP.Last().Connect(5))
                {
                    ActionSuccess="Успешное подключение";
                    SelectedPort.Entity.IsConnected=true;
                    IsDisconnected=false;
                    await _portService.UpdateAsync(SelectedPort.Entity);
                    ActionSuccessVisibility="Visible";
                    await Task.Run(() =>
                               {
                                   Thread.Sleep(2500);
                                   ActionSuccessVisibility = "Hidden";
                               });
                    await ReloadSelectedProtocolPortsAsync();
                }
                else
                {
                    _portsTCP.Remove(_portsTCP.Last());
                    ActionSuccess="Неудачное подключение";
                    ActionSuccessVisibility="Visible";
                    await Task.Run(() =>
                               {
                                   Thread.Sleep(2500);
                                   ActionSuccessVisibility = "Hidden";
                               });
                }
            }
            else
            {
                _portsBT.Add(new BTController()
                {
                    HostName=SelectedPort.Entity.HostName,
                    PortNum=(ushort)SelectedPort.Entity.PortNum,
                    StringEndByte=(byte)SelectedPort.Entity.StringEndByte
                });
                 if(_portsBT.Last().Connect(5))
                {
                    ActionSuccess="Успешное подключение";
                    SelectedPort.Entity.IsConnected=true;
                    IsDisconnected=false;
                    ActionSuccessVisibility="Visible";
                    await _portService.UpdateAsync(SelectedPort.Entity);
                    await Task.Run(() =>
                               {
                                   Thread.Sleep(2500);
                                   ActionSuccessVisibility = "Hidden";
                               });
                    await ReloadSelectedProtocolPortsAsync();
                }
                else
                {
                    _portsBT.Remove(_portsBT.Last());
                    ActionSuccess="Неудачное подключение";
                    ActionSuccessVisibility="Visible";
                    await Task.Run(() =>
                               {
                                   Thread.Sleep(2500);
                                   ActionSuccessVisibility = "Hidden";
                               });
                }
            }
        }

        private AsyncRelayCommand _disconnectFromPortCommand;
        public AsyncRelayCommand DisconnectFromPort => _disconnectFromPortCommand ??= new AsyncRelayCommand(OnDisconnectFromPortCommandExecuted);

        private async Task OnDisconnectFromPortCommandExecuted()
        {
            if(SelectedPort.Entity.Protocol=="TCP")
            {
                _portsTCP.Where(x=>x.HostName==SelectedPort.Entity.HostName &&
                            x.PortNum==SelectedPort.Entity.PortNum &&
                            x.StringEndByte==SelectedPort.Entity.StringEndByte).Last().Disconnect();
                ActionSuccess="Отключено";
                SelectedPort.Entity.IsConnected=false;
                IsDisconnected=true;
                ActionSuccessVisibility="Visible";
                await _portService.UpdateAsync(SelectedPort.Entity);
                await Task.Run(() =>
                            {
                                Thread.Sleep(2500);
                                ActionSuccessVisibility = "Hidden";
                            });
                
                await ReloadSelectedProtocolPortsAsync();
            }
            else
            {
                _portsBT.Where(x=>x.HostName==SelectedPort.Entity.HostName &&
                            x.PortNum==SelectedPort.Entity.PortNum &&
                            x.StringEndByte==SelectedPort.Entity.StringEndByte).Last().Disconnect();
                ActionSuccess="Отключён";
                SelectedPort.Entity.IsConnected=false;
                IsDisconnected=true;
                ActionSuccessVisibility="Visible";
                await Task.Run(() =>
                            {
                                Thread.Sleep(2500);
                                ActionSuccessVisibility = "Hidden";
                            });
            }
        }

        public DelegateCommand AddCommand => _addPortCommand ??= new DelegateCommand(OnAddPortCommandExecuted);

        public AsyncRelayCommand RemoveCommand =>
                _removePortRelayCommand ??= new AsyncRelayCommand(OnRemovePortCommandExecuted);

        public AsyncRelayCommand ApplyChangesCommand => _applyPortChangesRelayCommand ??=
                                                                new
                                                                        AsyncRelayCommand(OnApplyPortChangesCommandExecuted);

        public DelegateCommand ChangeEditModeCommand =>
                _changeEditModeCommand ??= new DelegateCommand(OnChangeEditModeCommandExecuted,
                                                               CanManipulateOnPort)
                        .ObservesProperty(() => SelectedPort);

        public AsyncRelayCommand ReloadCommand =>
                _reloadPortsRelayCommand ??= new AsyncRelayCommand(ReloadPortsAsync);

        private AsyncRelayCommand _reloadSelectedProtocolPortsRelayCommand;
        public AsyncRelayCommand ReloadSelectedProtocolPortsCommand =>
                _reloadSelectedProtocolPortsRelayCommand ??= new AsyncRelayCommand(ReloadSelectedProtocolPortsAsync);

        private bool CanManipulateOnPort() => SelectedPort is not null;

        private void OnChangeEditModeCommandExecuted() => IsEditMode = !IsEditMode;

        private void OnAddPortCommandExecuted()
        {
            Ports.Insert(0,
                            new PortEntity(new Database.Entity.Port
                            {
                                                      PortNum = null,
                                                      HostName = String.Empty,
                                                      StringEndByte = null,
                                                      IsConnected = false,
                                                      Protocol=SelectedProtocol
                                              }));

            SelectedPort = Ports.First();
            ElementsVisibility = "Visible";
        }

        private async Task OnRemovePortCommandExecuted()
        {      
            ElementsVisibility = "Hidden";
            if (SelectedPort.Entity.PortId == 0)
                Ports.Remove(SelectedPort);

            await _portService.RemoveAsync(SelectedPort.Entity);
            Ports.Remove(SelectedPort);
            SelectedPort = null;
        }

        private async Task OnApplyPortChangesCommandExecuted()
        {
            if(SelectedPort.Entity.HostName!=null && SelectedPort.Entity.PortNum!=null &&
                SelectedPort.Entity.StringEndByte!=null)
            { 
                if (SelectedPort.Entity.PortId == 0)
                { 
                    if(Ports.Where(x=>x.Entity.HostName==SelectedPort.Entity.HostName &&
                                    x.Entity.PortNum==SelectedPort.Entity.PortNum &&
                                    x.Entity.StringEndByte==SelectedPort.Entity.StringEndByte &&
                                    x.Entity.Protocol==SelectedPort.Entity.Protocol).Count()==1)
                    {
                        await _portService.AddAsync(SelectedPort.Entity);
                        ActionSuccess="Успешное добавление";
                        ActionSuccessVisibility="Visible";
                        await Task.Run(() =>
                                   {
                                       Thread.Sleep(2500);
                                       ActionSuccessVisibility = "Hidden";
                                   });
                        }
                    else
                    {
                        ActionSuccess="Порт уже существует";
                        ActionSuccessVisibility="Visible";
                        await Task.Run(() =>
                                   {
                                       Thread.Sleep(2500);
                                       ActionSuccessVisibility = "Hidden";
                                   });
                    }
                }
                else
                { 
                    if(Ports.Where(x=>x.Entity.HostName==SelectedPort.Entity.HostName &&
                            x.Entity.PortNum==SelectedPort.Entity.PortNum &&
                            x.Entity.StringEndByte==SelectedPort.Entity.StringEndByte &&
                            x.Entity.Protocol==SelectedPort.Entity.Protocol).Count()==1)
                    {
                        await _portService.UpdateAsync(SelectedPort.Entity);
                        ActionSuccess="Успешное обновление";
                        ActionSuccessVisibility="Visible";
                        await Task.Run(() =>
                                   {
                                       Thread.Sleep(2500);
                                       ActionSuccessVisibility = "Hidden";
                                   });
                    }
                    
                    else
                    {
                        ActionSuccess="Порт уже существует";
                        ActionSuccessVisibility="Visible";
                        await Task.Run(() =>
                                   {
                                       Thread.Sleep(2500);
                                       ActionSuccessVisibility = "Hidden";
                                   });
                    }
                }

                await ReloadSelectedProtocolPortsAsync();
            }
            else
            {
                    ActionSuccess="Данные некорректны";
                    ActionSuccessVisibility="Visible";
                    await Task.Run(() =>
                               {
                                   Thread.Sleep(2500);
                                   ActionSuccessVisibility = "Hidden";
                               });
            }
        }

        private async Task ReloadSelectedProtocolPortsAsync()
        {
            switch (SelectedProtocol)
            {
                case "BT":
                    await GetBTPortsAsync();
                    break;
                case "TCP":
                    await GetTCPPortsAsync();
                    break;
                default:
                    await ReloadPortsAsync();
                    break;
            }
        }

        private async Task ReloadPortsAsync()
        {
            
            ElementsVisibility = "Hidden";
            Ports.Clear();
            var dbSales = await _portService.GetAllAsync();

            foreach (var sale in dbSales)
                Ports.Add(new PortEntity(sale));
        }

        private async Task GetTCPPortsAsync()
        {
            Ports.Clear();         
            ElementsVisibility = "Hidden";
            var dbSales = await _portService.GetAllTCPPortsAsync();

            foreach (var sale in dbSales)
                Ports.Add(new PortEntity(sale));
        }

        private async Task GetConnectedPortsAsync()
        {
            Ports.Clear();         
            ElementsVisibility = "Hidden";
            var dbSales = await _portService.GetAllConnectedPortsAsync();

            foreach (var sale in dbSales)
                Ports.Add(new PortEntity(sale));
        }

        private async Task GetBTPortsAsync()
        {
            Ports.Clear();
            var dbSales = await _portService.GetAllBTPortsAsync();

            foreach (var sale in dbSales)
                Ports.Add(new PortEntity(sale));
        }
        #endregion
    }
}