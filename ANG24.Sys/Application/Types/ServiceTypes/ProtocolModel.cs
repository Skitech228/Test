//using Application.Helpers;
//using Application.Interfaces.Services;

//namespace Application.Types.ServiceTypes
//{
//    public class ProtocolModel : INPC
//    {
//        private string _viewModuleName;
//        private string _module;
//        private string _pathProtocol;
//        private string _fileName;
//        public DelegateCommand OpenProtocolCommand { get; set; }
//        public DelegateCommand SelectProtocolCommand { get; set; }

//        private readonly IProtocolService protocolService;
//        public string ViewModuleName
//        {
//            get => _viewModuleName;
//            set => SetProperty(ref _viewModuleName, value);
//        }
//        public string Module
//        {
//            get => _module;
//            set
//            {
//                SetProperty(ref _module, value);
//                ViewModuleName = GetViewModuleName(value);
//            }
//        }
//        public string PathProtocol
//        {
//            get => _pathProtocol;
//            set => SetProperty(ref _pathProtocol, value);
//        }
//        public string FileName
//        {
//            get => _fileName;
//            set => SetProperty(ref _fileName, value);
//        }
//        public ProtocolModel(IProtocolService protocolService)
//        {
//            OpenProtocolCommand = new DelegateCommand(OpenProtocol);
//            SelectProtocolCommand = new DelegateCommand(SelectProtocol);
//            this.protocolService = protocolService;
//        }
//        private void SelectProtocol() => protocolService.ChangeActiveProtocol(this);


//        private bool _active;
//        public bool Active
//        {
//            get => _active;
//            set => SetProperty(ref _active, value);
//        }
//        private void OpenProtocol()
//        {
//            protocolService.EditDependencyProtocol(Module);
//        }
//        private string GetViewModuleName(string module)
//        {
//            var viewModuleName = string.Empty;
//            switch (module)
//            {
//                case "HVMAC":
//                    viewModuleName = "Испытания переменным напряжением";
//                    break;
//                case "HVMDC":
//                    viewModuleName = "Испытания постоянным напряжением";
//                    break;
//                case "SA640":
//                    viewModuleName = "Мост СА640";
//                    break;
//                case "SA540":
//                    viewModuleName = "Мост СА540";
//                    break;
//                case "SA7100":
//                case "Bridge":
//                    viewModuleName = "Мост переменного тока";
//                    break;
//                default:
//                    viewModuleName = $"{module}*";
//                    break;
//            }
//            return viewModuleName;
//        }
//    }
//}
