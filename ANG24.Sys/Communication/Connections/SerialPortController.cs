using ANG24.Sys.Communication.Interfaces;
using ANG24.Sys.Communication.Types;
using System.IO.Ports;

namespace ANG24.Sys.Communication.Connections
{
    //усовершенствование версии SerialPort для удобства и надежности
    public class SerialPortController : ControllerQueue, ISerialPortController
    {
        //---------------Вспомогательные переменные-----------------
        private const byte defaultConnectAttempt = 5;
        private readonly object locker = new object();//Убрать? Убрать, скорее всего, если нет ссылок
        public SerialPort Port;
        public string Name { get; set; }
        public string PortName { get => Port.PortName; set => Port.PortName = value; }
        public int BaudRate { get => Port.BaudRate; set => Port.BaudRate = value; }

        //------------- Открытые переменные и свойства ---------------
        #region Публичные свойства
        public override bool Connected { get => Port?.IsOpen ?? false; }
        public override event Action OnDisconnected;
        public override event Action<string> OnDataReceived;
        public event Action<byte[]> OnBytesReceived;

        public SerialPortConnectionProtocol connectionProtocol { get; set; }
        #endregion
        //метод-сигнал об изменении текущей команды
        public SerialPortController()
        {
            IsSimulation = false;
            connectionProtocol = SerialPortConnectionProtocol.Normal;
            DataReciving = new AutoResetEvent(true);
            base.OnDataReceived += (s) => { OnDataReceived?.Invoke(s); }; // Для симмуляции
        }
        private readonly AutoResetEvent DataReciving;
        #region Публичные методы
        public SerialPort GetPort() => Port;
        public bool Connect() => Connect(defaultConnectAttempt);
        public override bool Connect(int AttemptCount)
        {
            if (IsSimulation)
            {
                queueWorker.DoWork += QueueWorker_DoSimulation;
                queueWorker.RunWorkerAsync();
                return true;
            }
            while (AttemptCount > 0 && !(Port?.IsOpen ?? false))
            {
                //попытки нельзя делать мгновенно
                try
                {
                    Port.Open();
                    Thread.Sleep(50); //поэтому выставляю задержки потока, чтобы дать время на процесс открытия
                    if (!Port.IsOpen && AttemptCount <= 1)
                        throw new ArgumentException($"Не удалось открыть указанный порт({Port.PortName})");//не срабатывает?
                    if (Port.IsOpen)
                    {
                        Port.DataReceived += Port_DataReceived;
                        Port.ReadTimeout = -1;
                        queueWorker.DoWork += QueueWorker_DoWork;
                        queueWorker.RunWorkerAsync();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.GetType()}: {ex.Message}");
                }
                AttemptCount--;
                Thread.Sleep(200); //и задержку между итерациями
            }
            return false;
        }
        public override void Disconnect()
        {
            if (!IsSimulation)
            {
                if (Port != null)
                {
                    Port?.Close();
                    Port.DataReceived -= Port_DataReceived;
                }
            }
            queueWorker.CancelAsync();
            queueBreaker.Set();
            OnDisconnected?.Invoke();
            queueWorker.DoWork -= QueueWorker_DoSimulation;

        }

        #endregion
        #region Приватные методы
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReciving.WaitOne();

            var res = string.Empty;
            var data = new byte[4096];
            try
            {
                if (connectionProtocol == SerialPortConnectionProtocol.Modbus)
                {
                    int temp = 0;
                    while (Port.BytesToRead > 0)// поиск кода устройства. Сжирает все байты пока не найдет нужный, так же может вызывать ошибки
                    {
                        data[0] = (byte)Port.ReadByte();
                        if (data[0] <= 2 && data[0] > 0)
                        {
                            Thread.Sleep(20);
                            temp = Port.BytesToRead + 1;
                            break;
                        }
                        //Убрать костыль, поставить ID номера устройств из controllerFinder
                        Thread.Sleep(10);
                    }
                    tryCount = 50; // ожидание 500мс
                    for (int i = 1; i < temp; i++)
                    {
                        //Thread.Sleep(5);
                        if (Port.BytesToRead > 0)
                        {
                            data[i] = (byte)Port.ReadByte();
                            if (i == 2) temp = data[i];
                        }
                        else
                        {
                            if (tryCount-- < 1) break;
                            Thread.Sleep(10);
                            i--;
                        }
                    }
                    Array.Resize(ref data, data[2]);
                }
                else
                {
                    res = Port.ReadLine();
                    Port.DiscardInBuffer(); // при использовании метода ReadLine обязательно надо чистить буффер
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException
                    || ex is TimeoutException
                    || ex is IOException)
                    Disconnect();
                Debug.WriteLine(ex.Message);
                DataReciving.Set();
            }
            if (res != string.Empty)
                Task.Factory.StartNew(() => OnDataReceived?.Invoke(res));
            if (data.Length != 0)
                Task.Factory.StartNew(() => OnBytesReceived?.Invoke(data));
            DataReciving.Set();
        }
        protected override bool WriteMessageToController(string message)
        {
            try
            {
                if (!Port.IsOpen) return false;
                //lock (locker)
                if (connectionProtocol == SerialPortConnectionProtocol.Modbus)
                {
                    byte[] data = message.Split(' ').Select(x => Convert.ToByte(x)).ToArray(); //StringToByteArray(System.Text.RegularExpressions.Regex.Replace(message, @"\s+", ""));
                                                                                               //Debug.WriteLine("out " + DateTime.Now.ToString("hh:mm:ss") + " " + BitConverter.ToString(data));
                    Port.Write(data, 0, data.Length);

                }
                else Port.Write(message);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                Disconnect();
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }
        #endregion

        public void SubscirbePortNotifications()
        {
            Port.DataReceived -= Port_DataReceived;
            Port.DataReceived += Port_DataReceived;
        }
        public void UnsubscirbePortNotifications() => Port.DataReceived -= Port_DataReceived;

        private void TriggerOnDataReceived(object sender, SerialDataReceivedEventArgs e) => gotDataFromTrigger = true;

        private bool gotDataReceivedTrigger = false;
        private bool gotDataFromTrigger = false;
        public void SetDataReceivedTrigger(bool subscribe)
        {
            if (subscribe)
            {
                gotDataReceivedTrigger = true;
                Port.DataReceived -= TriggerOnDataReceived;
                Port.DataReceived += TriggerOnDataReceived;
            }
            else
            {
                gotDataReceivedTrigger = false;
                Port.DataReceived -= TriggerOnDataReceived;
            }

        }
        public byte[] ApplyCommand(byte[] message, int delay = 100)
        {
            DataReciving.Reset();
            Debug.WriteLine(BitConverter.ToString(message, 0, message.Length));
            byte[] data = new byte[4096];
            if (Port?.IsOpen ?? false)
            {
                Port.Write(message, 0, message.Length);

                byte tryCount = 90; // 9 секунд задержка

                if (!gotDataReceivedTrigger)
                {
                    Thread.Sleep(delay);
                    while (Port?.BytesToRead == 0)
                    {
                        if (--tryCount < 1)
                        {
                            Array.Resize(ref data, data[2]);
                            return data;
                        }
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    while (gotDataFromTrigger == false && --tryCount > 1) Thread.Sleep(100);
                    gotDataFromTrigger = false;
                    Thread.Sleep(20);
                }
                int temp = 0;
                while (Port?.BytesToRead > 0)// поиск кода устройства. Сжирает все байты пока не найдет нужный, так же может вызывать ошибки
                {
                    data[0] = (byte)Port?.ReadByte();
                    if (data[0] <= 2 && data[0] > 0)
                    {
                        Thread.Sleep(20);
                        temp = Port.BytesToRead + 1;
                        break;
                    }
                    //Убрать костыль, поставить ID номера устройств из controllerFinder
                    Thread.Sleep(10);
                }
                tryCount = 50; // ожидание 500мс
                for (int i = 1; i < temp; i++)
                {
                    //Thread.Sleep(5);
                    if (Port?.BytesToRead > 0)
                    {
                        data[i] = (byte)Port?.ReadByte();
                        if (i == 2) temp = data[i];
                    }
                    else
                    {
                        if (tryCount-- < 1) break;
                        Thread.Sleep(10);
                        i--;
                    }
                }
            }
            Array.Resize(ref data, data[2]);
            DataReciving.Set();
            return data;
        }
        public string ApplyCommand(string message, int delay)
        {
            DataReciving.Reset();
            Port.WriteLine(message);
            Thread.Sleep(delay);
            var res = Port.ReadLine();
            DataReciving.Reset();
            return res;
        }
    }
}