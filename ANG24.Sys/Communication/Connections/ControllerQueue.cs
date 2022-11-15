using ANG24.Sys.Communication.Helpers;
using ANG24.Sys.Communication.Interfaces;
using System.ComponentModel;

namespace ANG24.Sys.Communication.Connections
{
    /// <summary>
    /// Управление очередью команд
    /// </summary>
    public abstract class ControllerQueue : IController
    {
        protected byte tryCount;
        protected BackgroundWorker queueWorker;
        protected readonly AutoResetEvent queueBreaker;
        private readonly Queue<ControllerCommand> commandQueue;
        private readonly AutoResetEvent frequencyBreaker;
        private readonly ManualResetEvent queuePauser;
        private readonly System.Timers.Timer timer;
        private readonly System.Timers.Timer frequencyTimer;
        protected ControllerQueue()
        {
            commandQueue = new Queue<ControllerCommand>();
            queueBreaker = new AutoResetEvent(false);
            frequencyBreaker = new AutoResetEvent(true);
            queuePauser = new ManualResetEvent(true);
            queueWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            timer = new System.Timers.Timer(CommandAwaitInterval) { AutoReset = false };
            timer.Elapsed += (p, e) =>
            {
                if (tryCount > 0)
                    CommandContinue();
            };
            frequencyTimer = new System.Timers.Timer(CommandsFrequency) { AutoReset = true };
            frequencyTimer.Elapsed += (p, e) =>
            { // каждые CommandsFrequency mc даёт возможность отправки данных в контроллер
                frequencyBreaker.Set();
            };

        }
        protected ControllerCommand _currentCommand;

        //------------- Открытые переменные и свойства ---------------
        #region Публичные свойства
        public event Action<ControllerCommand> OnCurrentCommandChanged;
        public abstract event Action OnDisconnected;
        public virtual event Action<string> OnDataReceived;

        public ushort CommandsFrequency { get; set; } = 200;
        public byte WriteTryCount { get; set; } = 5;
        public uint CommandAwaitInterval { get; set; } = 6000;
        public ControllerCommand CurrentCommand
        {
            get => _currentCommand;
            set
            {
                _currentCommand = value;
                OnCurrentCommandChanged?.Invoke(value);
            }
        }
        public bool IsSimulation { get; set; } = false;
        public abstract bool Connected { get; }


        #endregion
        #region Публичные методы
        public virtual void ComamndDone(bool done)
        {
            timer.Close();
            if (CurrentCommand != null)
                CurrentCommand.CommandDone = done;
            tryCount = WriteTryCount;
            if (commandQueue.Count != 0)
            {
                commandQueue.Dequeue().Complete();
                queueBreaker.Set();
            }
            else CurrentCommand = null;
        }
        public virtual byte CommandContinue()
        {
            if (commandQueue.Count > 0)
                queueBreaker.Set();
            return tryCount;
        }
        public void WriteCommand(ControllerCommand command)
        {
            if (IsSimulation || commandQueue.Count > 10) return;
            commandQueue.Enqueue(command);
            tryCount = WriteTryCount;
            if (commandQueue.Count == 1) queueBreaker.Set();
        }
        public void WriteCommandPriority(ControllerCommand command)
        {
            if (IsSimulation) return;
            gotPrioritetCommand = true;
            Thread.Sleep(CommandsFrequency);
            WriteMessageToController(command.Message);
            gotPrioritetCommand = false;
            queueBreaker.Set();
        }
        public void EmergencyStop() // не работает
        {
            commandQueue.Clear();
            queueBreaker.Set();
            Thread.Sleep(CommandsFrequency);
            queuePauser.Reset();
        }
        public abstract bool Connect(int AttemptCount = 5);
        public abstract void Disconnect();
        public virtual async Task<bool> ConnectAsync() => await Task.Factory.StartNew(() => Connect());
        public void StartQueue() => queuePauser.Set();
        public void StopQueue() => queuePauser.Reset();
        #endregion
        #region Приватные методы
        protected virtual void QueueWorker_DoSimulation(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            while (!worker.CancellationPending)
            {
                Thread.Sleep(500);
                OnDataReceived?.Invoke("");
            }
        }
        //фоновый процесс обработки очереди команд на отправку
        protected virtual void QueueWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //проверить все варианты выключения воркера, может в неподходящий момент упасть
            var worker = sender as BackgroundWorker;
            tryCount = WriteTryCount;
            var s = Stopwatch.StartNew();
            frequencyTimer.Interval = CommandsFrequency;
            frequencyTimer.Start();

            // Ты опустился на столько глубоко в эту кроличью нору
            // что обратного пути уже нет.
            // Эта строка служит предупреждением для всех, 
            // кто решил вторгнуться во владения СМЮ и потрогать своими шаловливыми пальчиками
            // очередь команд...
            /// не надо
            bool isBool = bool.Parse(bool.TrueString);
            ushort errorsCount = ushort.Parse(ushort.MinValue.ToString());


            while (!worker.CancellationPending)
            {
                /*
                 * процесс от подачи команды в порт до получения 
                 * ответа 1-тактовый, детерминированный:
                 * 0. (*ожидание сигнала о начале следующей итерации*)
                 * 1. Получение текущей команды в очереди
                 * 2. 
                 */
                //Ожидание до сигнала                

                queueBreaker.WaitOne();
                queuePauser.WaitOne();
                if (worker.CancellationPending) continue;
                if (gotPrioritetCommand)
                {
                    timer.Close();
                    continue;
                }
                if (!CurrentCommand?.CommandDone ?? false)
                    if (--tryCount <= 0) ComamndDone(false);
                try
                {
                    if (commandQueue.Count != 0)
                    {
                        CurrentCommand = commandQueue.Peek();
                        frequencyBreaker.WaitOne();
                        WriteMessageToController(CurrentCommand?.Message);
                        Debug.WriteLine(CurrentCommand?.Message);
                        timer.Close();
                        timer.Start();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    isBool = bool.Parse(bool.FalseString);
                }
            }
            e.Cancel = true;
            if (isBool == bool.Parse(bool.FalseString))
                Debug.WriteLine($"Во время работы {GetType} возникали ошибки ({errorsCount})");
            //throw new Exception("Сгорел сарай - гори и хата!");
            return;
        }
        #endregion
        protected bool gotPrioritetCommand = false;
        protected abstract bool WriteMessageToController(string message);
    }
}
