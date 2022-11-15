//using System.Windows.Input;
//using Infrastructure.Interfaces.Services;

//namespace Infrastructure.Types
//{
//    public class Marker : BindableBase
//    {
//        private double _markerXPos;
//        private double _l;
//        //private int _dotPosition;
//        private string _description;
//        private double _vis_L;
//        private double _lCableDefautl;
//        private string _name;

//        public DelegateCommand CM_OpenDescription { get; set; }
//        public DelegateCommand CM_DropMarker { get; set; }
//        public ICommand MarkerDownCommand { get; set; }
//        public ICommand MarkerUpCommand { get; set; }
//        public ICommand MarkerMoveCommand { get; set; }

//        public Marker(IReflectOSCParameterManager refParManager)
//        {

//            CM_OpenDescription = new DelegateCommand(OpenDescription);
//            CM_DropMarker = new DelegateCommand(ClearMarker);
//            //-------------------------------------------------------------------------------
//            MarkerDownCommand = new DelegateCommand<MouseButtonEventArgs>(e => MarkerDown(e));
//            MarkerUpCommand = new DelegateCommand<MouseButtonEventArgs>(e => MarkerUp(e));
//            MarkerMoveCommand = new DelegateCommand<MouseEventArgs>(e => MarkerMove(e));
//            //-------------------------------------------------------------------------------
//            refParManager.K_Changed += () => MarkerXPosChange();
//            refParManager.L_Changed += () => MarkerXPosChange();
//            refParManager.LenMass_Changed += () => MarkerXPosChange();
//            refParManager.SR_Changed += () => MarkerXPosChange();
//            Description = string.Empty;
//        }

//        private void ClearMarker()
//        {
//            Debug.WriteLine("Clear marker: {0}", ToString());
//            EventCollection.InvokeEvent(nameof(EventCollection.OnClearMarker), this);
//        }

//        private void OpenDescription()
//        {
//            throw new NotImplementedException();
//        }

//        //private Action MarkerXPosChange => () => MarkerXPos = (int)Math.Round((double)(DotPosition / Xdpp()));
//        public Action MarkerXPosChange => () =>
//        {
//            L = L;
//        };

//        private void MarkerMove(System.Windows.Input.MouseEventArgs e)
//        {
//            if (!(e.Source as Line).IsMouseCaptured)
//            {
//                return;
//            }
//            var x = (int)e.GetPosition(e.Source as Line).X;
//            x = x < (int)(MainImageSize.Width - 45) ? x : (int)(MainImageSize.Width - 45);
//            x = x > 0 ? x : 0;
//            var DotPosition = x * Xdpp();
//            var DeltaDP = DotPosition - ZeroMarker.DotPos;
//            L = T * C * DeltaDP / 2;
//            Vis_L = T * C * DeltaDP / (2 * K_val) - LCableDefault;
//        }

//        private void MarkerUp(MouseButtonEventArgs e)
//        {
//            (e.Source as Line).ReleaseMouseCapture();
//        }

//        private void MarkerDown(MouseButtonEventArgs e)
//        {
//            (e.Source as Line).CaptureMouse();
//        }
//        public override int GetHashCode()
//        {
//            return base.GetHashCode();
//        }
//        public double MarkerXPos
//        {
//            get => _markerXPos;
//            set => SetProperty(ref _markerXPos, value);
//        }
//        public double L
//        {
//            get => _l;
//            set
//            {
//                SetProperty(ref _l, value);
//                var CT = C * T;
//                var xdpp = Xdpp();
//                MarkerXPos = 2 * value / (CT * xdpp) + ZeroMarker.DotPos / xdpp;
//                Vis_L = value / K_val - LCableDefault;
//            }
//        }
//        public double Vis_L
//        {
//            get => _vis_L;
//            set
//            {
//                SetProperty(ref _vis_L, value);
//            }
//        }
//        public double LCableDefault
//        {
//            get => _lCableDefautl;
//            set
//            {
//                SetProperty(ref _lCableDefautl, value);
//                Vis_L = L / K_val - value;
//            }
//        }
//        public string Description
//        {
//            get => _description;
//            set => SetProperty(ref _description, value);
//        }
//        public override string ToString()
//        {
//            return string.Format("{0}:\n L = {1} m", Description, Math.Round(Vis_L, 3));
//        }
//    }
//}
