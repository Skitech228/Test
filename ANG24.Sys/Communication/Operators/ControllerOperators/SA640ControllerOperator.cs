#define NOTTEST
using ANG24.Sys.Communication.Operators.AbstractOperators;
using ANG24.Sys.Application.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Text;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class SA640ControllerOperator : ModBusControllerOperator<SA640Data>, ISA640ControllerOperator
    {
        public event Action<SA640Transformer.Phase> OnNextPhaseMeasStarted;
        public event Action<bool> OnStateChanged;
        public event Action OnTransformerChanged;
        public override event Action<SA640Data> OnDataReceived;
        public event Action<int> OnNextDemagCycleStarted;

        public const int DEVICEID = 2;
        private int mode = 0; // Условный enum
        private readonly int TransformerIndex = 0;
        private ObservableCollection<SA640Transformer> Transformers { get; set; }
        private Task currentTask;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        private readonly AutoResetEvent ChangePhase = new(true);
        private readonly AutoResetEvent NextTest = new(false);

        private bool isNextDemagCycleRequested;
        private bool isStable = false;
        private int stableCounter = 0;
        private double curr_test = 0;
        private int delay = 1;
        private int stableSeconds = 0;

        private DateTime timer; //заменить на stopwatch
        private int totalSeconds = 0;
        private readonly int Meas_Aver = 50;
        private readonly INotificationService message;
        private SA640Transformer.Phase currentPhase;
        private List<(int, SA640Transformer.Phase)> order;

        public SA640ControllerOperator(INotificationService message) : base()
        {
            Name = Application.Types.Enum.LabController.SA640.ToString();
            Transformers = new ObservableCollection<SA640Transformer>() { new SA640Transformer(3) };
            order = new List<(int, SA640Transformer.Phase)>();
            ChangeOrderMeasR(0); // авто режим сортировки
            tokenSource = new CancellationTokenSource();
            this.message = message;


            //Task.Factory.StartNew(() =>
            //{
            //    double rr = 5, ri = 5, ru = 5;
            //    while (true)
            //    {
            //        Thread.Sleep(CommandsFrequency);
            //        OnDataReceived?.Invoke(new SA640Data()
            //        {
            //            FR = rr += (float)Math.Round((float)Random.Shared.Next(-10, 11) / 100, 2),
            //            GI = ri += (float)Math.Round((float)Random.Shared.Next(-10, 11) / 100, 2),
            //            GU = ru += (float)Math.Round((float)Random.Shared.Next(-10, 11) / 100, 2),
            //            Command = 56
            //        });
            //    }
            //});
        }
        public override void EmergencyStop()
        {
            Controller.EmergencyStop();
            Controller.ComamndDone(false);
            mode = 0;
            tokenSource.Cancel();
            NextTest.Set();
            ChangePhase.Set();

            OnStateChanged?.Invoke(false);
        }
        protected override void CommandBroker(SA640Data data)
        {
            switch (data.Command)
            {
                case 0x38:
                    if (mode == 1) Meas();//Измерение сопротивления
                    if (mode == 2) Demag();//Размагничивание
                    if (mode == 3)//Дисчардж
                    {
                        if (data.State == 0) mode = 0;
                        NextTest.Set();
                    }//Дисчардж
                    break;
                case 0x00:
                    break;
            }
            if (data.Command == 0)
            {
                switch (data.ErrorCode)
                {
                    case 1:
                        message.SendNotificationOK("Перегрузка! Подано большое напряжение", null);
                        break;
                    case 2:
                        message.SendNotificationOK("Перегрузка! Подан большой ток", null);
                        break;
                    case 4:
                        message.SendNotificationOK("Выполняется разряд индуктивности!", null);
                        break;
                    case 5:
                        message.SendNotificationOK("Превышен ток источника!", null);
                        break;
                    case 6:
                        message.SendNotificationOK("Перегрев! Превышена температура прибора", null);
                        break;
                    case 255:
                        message.SendNotificationOK("Нет связи с контроллером", null);
                        break;
                    default:
                        message.SendNotificationOK("Неопознанная ошибка от контроллера", null);
                        break;
                }
            }
            OnDataReceived?.Invoke(data);
            void Meas()
            {
                if (!isStable)
                {
                    totalSeconds = (int)(DateTime.Now - timer).TotalSeconds;

                    if (Math.Abs(100 * (data.GI - curr_test) / curr_test) < 10.0)
                    {
                        if (totalSeconds - stableSeconds > 0)
                        {
                            stableCounter++;
                            stableSeconds = totalSeconds;
                        }
                    }
                    else stableCounter = 0;
                    if (stableCounter > 20
                        || totalSeconds > 70
                        || totalSeconds > 15 && Math.Abs(data.GR) > 1000
                        || totalSeconds > 25 && Math.Abs(data.GR) > 200)
                    {
                        isStable = true;//Ток стабильный                         
                    }
                    currentPhase.U = data.GU;
                    currentPhase.R = data.FR;
                    currentPhase.I = data.GI;
                    data.FR = 0;
                }
                else
                {
                    currentPhase.U = data.FU;
                    currentPhase.R = data.FR;
                    currentPhase.I = data.FI;
                }
                NextTest.Set();
            }
            void Demag()
            {

                if (!isStable)
                {
                    if (Math.Abs(curr_test) * 0.95 < data.GI)
                        stableCounter++;

                    if (stableCounter >= 15)
                        isStable = true;
                }
                else if (timer.AddSeconds(delay) <= DateTime.Now) isStable = false;
                if (curr_test < 0)
                {
                    data.FU = -data.FU;
                    data.FI = -data.FI;
                    data.GU = -data.GU;
                    data.GI = -data.GI;
                }
                NextTest.Set();
            }
        }
        public void MeasR(double PercentTestI, int changeMode, bool reverseDirection, bool isFast, SA640Transformer.Phase SinglePhase = null)//кнопка "установка тока"
        {
            StartNewTask(() =>
                {
                    foreach (var phase in order)
                    {
                        ChangePhase.WaitOne();
                        var aver = Meas_Aver;
                        mode = 1;
                        timer = DateTime.Now;

                        if (changeMode != 3)
                            currentPhase = phase.Item2;
                        else
                            currentPhase = SinglePhase;
                        OnNextPhaseMeasStarted?.Invoke(currentPhase);
                        curr_test = currentPhase.Coil.NominalI * (PercentTestI / 100);
                        StartMeas(GetCom(), curr_test, isFast);
                        while (!isStable) // установка тока
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest();
                            NextTest.WaitOne();
                        }

                        if (isFast) aver /= 2;
                        if (currentPhase.R > 50.0e-3) aver /= 2;
                        for (int i = 0; i < aver; i++)
                        {

                            token.ThrowIfCancellationRequested();
                            SingleTest();
                            NextTest.WaitOne();
                        }
                        mode = 3;
                        isStable = false;
                        StartDischarge();
                        while (mode == 3)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest();
                            NextTest.WaitOne();
                        }
                        StopDischarge();
                        if (changeMode == 3)
                        {
                            stableSeconds = 0;
                            isStable = false;
                            totalSeconds = 0;
                            stableCounter = 0;
                            break;
                        }
                        if (!reverseDirection)
                            DirectSwitchPhase();
                        else
                            ReverseSwitchPhase();
                        stableSeconds = 0;
                        totalSeconds = 0;
                        stableCounter = 0;
                    }
                    ChangePhase.Set();
                });
            void DirectSwitchPhase()
            {
                var switchType = currentPhase.Coil.SwitchType;
                if (currentPhase.Index >= order.Count) return;
                var nextPhase = order[currentPhase.Index].Item2;
                if (changeMode != 0) switchType = (byte)changeMode;
                switch (switchType)
                {//немного не правильно работают эвенты
                    case 0:
                        if (currentPhase.Name == "CA" || currentPhase.Name == "CN")
                            OnNextCoil(nextPhase);
                        else ChangePhase.Set();
                        break;
                    case 1:
                        if (currentPhase.Name != "CA" && currentPhase.Name != "CN") ChangePhase.Set();
                        else if (currentPhase.Position == currentPhase.Coil.NPositions - 1)
                            OnNextCoil(nextPhase);
                        else
                            OnNextPosition(nextPhase);
                        break;
                    case 2:
                        if (currentPhase.Position == currentPhase.Coil.NPositions - 1 && (currentPhase.Name == "CA" || currentPhase.Name == "CN"))
                        {
                            OnNextCoil(nextPhase);
                            break;
                        }
                        if (!(currentPhase.Position == currentPhase.Coil.NPositions - 1)) //если позиция не последняя
                            OnNextPosition(nextPhase);
                        else
                            OnNextCoil(nextPhase);
                        break;
                }
            }//РЕФАКТОРИНГ В ПЕРВУЮ ОЧЕРЕДЬ!
            void ReverseSwitchPhase()
            {
                var switchType = currentPhase.Coil.SwitchType;
                if (currentPhase.Index >= order.Count) return;
                var nextPhase = order[currentPhase.Index].Item2;
                if (changeMode != 0) switchType = (byte)changeMode;
                switch (switchType)
                {
                    case 0:
                        if (currentPhase.Name == "AB" || currentPhase.Name == "AN")
                            OnNextCoil(nextPhase);
                        else ChangePhase.Set();
                        break;
                    case 1:
                        if (currentPhase.Name != "AB" && currentPhase.Name != "AN") ChangePhase.Set();
                        else if (currentPhase.Position == 0)
                            OnNextCoil(nextPhase);
                        else
                            OnNextPosition(nextPhase);
                        break;
                    case 2:
                        if (currentPhase.Position == 0 && (currentPhase.Name == "AB" || currentPhase.Name == "AN"))
                        {
                            OnNextCoil(nextPhase);
                            break;
                        }
                        if (!(currentPhase.Position == 0)) //если позиция не последняя
                            OnNextPosition(nextPhase);
                        else OnNextCoil(nextPhase);
                        break;
                }
            }
            void OnNextCoil(SA640Transformer.Phase phase)
            {
                message.SendNotificationOKCancel($"Подключите измерительный кабель к обмотке {phase.Coil} в позицию {phase.Position + 1}", () => ChangePhase.Set(), EmergencyStop);
            }
            void OnNextPosition(SA640Transformer.Phase phase)
            {
                message.SendNotificationOKCancel($"Установите переключатель положений в позицию {phase.Position + 1}", () => ChangePhase.Set(), EmergencyStop);
            }
            int GetCom()
            {
                int com = 0;
                switch (currentPhase.Name)
                {
                    case "AB": com = 1; break;
                    case "BC": com = 2; break;
                    case "CA": com = 3; break;
                    case "AN": com = 4; break;
                    case "BN": com = 5; break;
                    case "CN": com = 6; break;
                }
                return com;
            }
        }
        public void Demagnetization(DemagMode demagMode, int ITime, int coil, double Ifirst, double Ilast)
        {
            StartNewTask(() =>
            {
                delay = ITime;
                var currListForDemag = GetCylesForDemagnitization(Ifirst, Ilast);
                int com = 1,
                    nPhases = Transformers[TransformerIndex].PhaseCount;
                if (nPhases == 1) com = 4; // Для однофазных трансформаторов положение коммутатора - AN

                if (demagMode > (DemagMode)1)
                {
                    nPhases = 1;
                    com = (int)demagMode - 1;
                }
                if (Transformers[TransformerIndex].Coils[coil].Сonnection.ToLower().EndsWith("n")) com += 3;

                for (int i = 0; i < nPhases; i++, com++)
                {
                    OnNextPhaseMeasStarted?.Invoke(Transformers[TransformerIndex].Coils[coil].Positions[0].Phases[i]);
                    for (int j = 0; j < currListForDemag.Length; j++)
                    {
                        mode = 2;
                        curr_test = currListForDemag[j];
                        OnNextDemagCycleStarted?.Invoke((byte)j);
                        StartMeas((byte)(curr_test > 0 ? com : com + 6), Math.Abs(curr_test), false);

                        while (!isStable)
                        {
                            if (isNextDemagCycleRequested) break;
                            token.ThrowIfCancellationRequested();
                            SingleTest();
                            NextTest.WaitOne();
                        }
                        timer = DateTime.Now;
                        while (isStable) //Удержание тока
                        {
                            if (isNextDemagCycleRequested) break;
                            token.ThrowIfCancellationRequested();
                            SingleTest();
                            NextTest.WaitOne();
                        }
                        mode = 3;
                        StartDischarge();
                        while (mode == 3)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest();
                            NextTest.WaitOne();
                        }
                        StopDischarge();
                        stableCounter = 0;
                        isNextDemagCycleRequested = false;
                    }
                }
            });
        }

        #region Commands
        public void ConnectToMeas(Action<bool> action = null) => SetCommand($"{DEVICEID} 31 5 0", action);//0x01 0x1F (ushort)5 CS
        /// <summary>
        /// Команда 0x31 “Запуск процесса измерения” (запуск установки тока)
        /// </summary>
        /// <param name="com">
        /// положение коммутатора
        /// 0 - выключено;
        /// 1 - A-B;		2 - B-C;		3 - C-A;
        /// 4 - A-N;		5 - B-N;		6 - C-N;
        /// 7 - B-A;		8 - C-B;		9 - A-C;
        /// 10 - N-A;	    11 - N-B;	    12 - N-C
        /// </param>
        /// <param name="current">тестовый ток (ток, на котором будет проводится измерение)</param>
        /// <param name="isFast">Запуск процесса измерения с быстрой установкой тока</param>
        /// <summary>
        /// Команда 0x38 “Единичное измерение”
        /// </summary>
        private void StartMeas(int com, double current, bool isFast, Action<bool> action = null)
        {
            StringBuilder str = new($"{DEVICEID} {(isFast ? 57 : 49)} 10 0 {com}");
            foreach (var item in BitConverter.GetBytes(current))
                str.Append($" {item}");
            SetCommand(str.ToString(), action);
        }//0x01 (isFast?0x39:0x31) 10 сom current Volt CS
        private void SingleTest(Action<bool> action = null) => SetCommand($"{DEVICEID} 56 5 0", action);//0x01 0x38 5 CS
        /// <summary>
        /// Команда 0x33 “Запуск разряда индуктивности”
        /// </summary>
        private void StartDischarge(Action<bool> action = null) => SetCommand($"{DEVICEID} 51 5 0", action);//0x01 0x33 5 CS
        /// <summary>
        /// Команда 0x34 “Окончание разряда индуктивности” (выключение коммутатора)
        /// </summary>
        private void StopDischarge(Action<bool> action = null) => SetCommand($"{DEVICEID} 52 5 0", action);//0x01 0x34 5 CS
        #endregion
        #region Help methods
        public override void SetPort(System.IO.Ports.SerialPort port)
        {
            base.SetPort(port);
            Controller.CommandAwaitInterval = 2000;
            Controller.WriteTryCount = 3;
        }
        /// <summary>
        /// Создает асинхронную задачу для отправки данных в порт
        /// </summary>
        /// <param name="action"></param>
        private void StartNewTask(Action action)
        {
            if (tokenSource != null) tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            if (currentTask?.IsCompleted ?? true || currentTask?.Status != TaskStatus.Running)
            {
                OnStateChanged?.Invoke(true);
                currentTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        action.Invoke();
                        OnStateChanged?.Invoke(false);
                    }
                    catch
                    {
                        OnStateChanged?.Invoke(false);
                    }
                }, token);
            }
        }
        public SA640Transformer GetTransformer() => Transformers[TransformerIndex];
        public void SetNewTransformer(SA640Transformer transformer)
        {
            Transformers.Clear();
            Transformers.Add(transformer);
            OnTransformerChanged?.Invoke();
        }
        public void ChangeOrderMeasR(int mode, bool reverseDirection = false)
        {
            order.Clear();
            int count = 1;
            short CoilsCount = (short)Transformers[TransformerIndex].Coils.Count;
            short start = (short)(reverseDirection ? -CoilsCount + 1 : 0);
            short end = (short)(reverseDirection ? 1 : CoilsCount);
            switch (mode)
            {
                case 0:
                    for (short i = start; i < end; i++)
                        if (Transformers[TransformerIndex].Coils[Math.Abs(i)].SwitchType == 1)
                            ChangeOrderPhase(Math.Abs(i));
                        else ChangeOrderSwitch(Math.Abs(i));

                    break;
                case 1: //Фазы
                    for (short i = start; i < end; i++)
                        ChangeOrderPhase(Math.Abs(i));
                    break;
                case 2: //Переключатель
                    for (short i = start; i < end; i++)
                        ChangeOrderSwitch(Math.Abs(i));
                    break;
            }

            void ChangeOrderPhase(short coil)
            {
                short startC = (short)(reverseDirection ? -Transformers[TransformerIndex].Coils[coil].NPositions + 1 : 0);
                short endC = (short)(reverseDirection ? 1 : Transformers[TransformerIndex].Coils[coil].NPositions);
                for (short j = startC; j < endC; j++)
                {
                    if (Transformers[TransformerIndex].PhaseCount == 3)
                    {
                        for (short i = (short)(reverseDirection ? -2 : 0); i < (short)(reverseDirection ? 1 : 3); i++)
                            Transformers[TransformerIndex].Coils[coil].Positions[Math.Abs(j)].Phases[Math.Abs(i)].Index = count++;
                    }
                    else
                        Transformers[TransformerIndex].Coils[coil].Positions[j].Phases[0].Index = count++;
                }
            }
            void ChangeOrderSwitch(short coil)
            {
                short tempPos = Transformers[TransformerIndex].Coils[coil].NPositions;
                if (tempPos == 1) tempPos = 0;
                short startPosition = (short)(reverseDirection ? -tempPos + 1 : 0);
                short endPosition = (short)(reverseDirection ? 1 : Transformers[TransformerIndex].Coils[coil].NPositions);
                short startPhase = (short)(reverseDirection ? -Transformers[TransformerIndex].PhaseCount + 1 : 0);
                short endPhase = (short)(reverseDirection ? 1 : Transformers[TransformerIndex].PhaseCount);
                for (short f = startPhase; f < endPhase; f++)
                    if (tempPos != 0)
                        for (short j = startPosition; j < endPosition; j++)
                            Transformers[TransformerIndex].Coils[coil].Positions[Math.Abs(j)].Phases[Math.Abs(f)].Index = count++;
                    else Transformers[TransformerIndex].Coils[coil].Positions[0].Phases[Math.Abs(f)].Index = count++;
            }
            foreach (var coil in Transformers[TransformerIndex].Coils)
            {
                foreach (var Position in coil.Positions)
                    foreach (var Phase in Position.Phases)
                        order.Add((Phase.Index, Phase));
            }
            order = order.OrderBy(x => x.Item1).ToList();
        }
        public void NextDemagCycle() => isNextDemagCycleRequested = true;
        public double[] GetCylesForDemagnitization(double Ifirst, double Ilast)
        {
            var currListForDemag = new double[30];
            int nCycles = 0;
            double cur_round, polar = 1;
            Ifirst = Meas_CurrRound(Ifirst);
            Ilast = Meas_CurrRound(Ilast);
            for (double cur = Ifirst; cur >= 0.1 || nCycles >= 30; cur *= 0.60F)
            {
                cur_round = Meas_CurrRound(cur);
                if (cur_round < Ilast) cur_round = Ilast;

                currListForDemag[nCycles++] = polar * cur_round;

                if (cur_round <= Ilast) break;
                polar = -polar;
            }
            Array.Resize(ref currListForDemag, nCycles);
            return currListForDemag;
            double Meas_CurrRound(double cur)
            {//Округление тока
                double ret = cur > 25 ? 25 : cur; //Ограничение максимума на 25А
                ret = cur < 0.1 ? 0.1F : cur;//Ограничение минимума на 0.1А
                if (cur >= 5)
                    ret = (float)Math.Round(cur + 0.5);
                else if (cur >= 1)
                {
                    cur *= 10;
                    ret = (float)Math.Round(cur + 0.5);
                    ret /= 10;
                }
                else
                {
                    cur *= 100;
                    ret = (float)Math.Round(cur + 0.5);
                    ret /= 100;
                }
                return ret;
            }
        }
        #endregion
    }
}
