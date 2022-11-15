using ANG24.Sys.Communication.Operators.AbstractOperators;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class SA7100ControllerOperator : TCPControllerOperator<SA7100Data>, ISA7100ControllerOperator
    {
        public override string Name => Application.Types.Enum.LabController.SA7100.ToString();
        public bool IsPowerOn { get; private set; }

        public event Action OnPower;
        public override event Action<SA7100Data> OnDataReceived;

        private short tag = 0;
        private int Avg; //Накопление
        #region Дополнительная информация об объекте измерений и самом измерении
        private string ObjectName;
        private string ObjectNote;
        private string FactoryNumber;
        private string ReleaseDate;
        //private readonly string Scheme; //Инверт\прям\В/В коммутатор не подключен
        //private readonly string PhaseChanging; //Вкл/Выкл
        //private readonly string SubMeas;
        #endregion
        public SA7100ControllerOperator() : base() { Initialization(); }
        private void Initialization() => SetPort(8841, "localhost", 0x0A);
        protected override void CommandBroker(SA7100Data data)
        {
            if (data.Command == "measure")
            {
                data.ObjectName = ObjectName;
                data.ObjectNote = ObjectNote;
                data.FactoryNumber = FactoryNumber;
                data.ReleaseDate = ReleaseDate;

                data.Avg = Avg == 1 ? "Авто" : Avg.ToString();
                //data.Scheme = Scheme ?? "В/В коммутатор не подключен";
                //data.PhaseChanging = PhaseChanging ?? "Выкл";
                //data.SubMeas = SubMeas ?? "Авто";
            }
            if (data.Payload == "OperationalState" || data.State == "OperationalState") { IsPowerOn = true; OnPower?.Invoke(); }
            if (data.Payload == "PowerOffState") { IsPowerOn = false; OnPower?.Invoke(); }
            OnDataReceived?.Invoke(data);
        }

        public override void EmergencyStop()
        {
            PowerOff();
            Thread.Sleep(100);
            Controller.EmergencyStop();
        }
        public void SendMessage(string message) => AddTag(message);
        public void SetTestObjectInfo(string ObjectName, string ObjectNote, string FactoryNumber, string ReleaseDate)
        {
            this.ObjectName = ObjectName;
            this.ObjectNote = ObjectNote;
            this.FactoryNumber = FactoryNumber;
            this.ReleaseDate = ReleaseDate;
        }
        public void CancelCurrentCommand() => SetCommandPriority($"cancel target={tag - 1}");
        public void CurrentState() => SetCommandPriority("state state");
        public void NextState() => SetCommandPriority("state state.next");
        public void PowerOn() => AddTag("power on");
        public void PowerOff() => SetCommandPriority("power off");
        public void SetLicense(string key) => SetCommandPriority($"license add, key={key}");
        public void SetCoeffsFromCU() => SetCommandPriority("coeffs src=cb");
        public void MeasureCX(int avg, double c0, double tg0)
        {
            Avg = avg;
            if (avg > 99) avg = 99;
            if (tg0 > 1) tg0 = 1;
            if (c0 == 0 || tg0 == 0)
                AddTag($"measure what=cx,avg={avg}");
            else
                AddTag($"measure what=cx,avg={avg},tg0={tg0},c0={c0}");
        }
        public void MeasureUX(double c0, double tg0)
        {
            if (c0 == 0 || tg0 == 0)
                AddTag("measure what=ux");
            else
                AddTag($"measure what=ux,tg0={tg0},c0={c0}");
        }
        public void MeasureRX(int uset) => AddTag($"measure what=rx,uset={uset}");
        private void AddTag(string str) => SetCommandPriority($"{str},tag={tag++}");
        public void ChangeMeasScheme(bool isReverse) => AddTag($"measure what=rx,uset={isReverse}"); //сделать рабочий вариант
    }
}