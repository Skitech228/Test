using ANG24.Sys.Communication.Connections;
using ANG24.Sys.Communication.Helpers;
using System.Net.Sockets;
using System.Text;

namespace ANG24.Sys.Communication.Connections
{
    public class TCPController : ControllerQueue //Добавить выбрасывание исключений
    {
        public ushort PortNum { get; set; } = 8841;
        public string HostName { get; set; } = "localhost";
        public byte StringEndByte { get; set; } = 0;
        public override event Action OnDisconnected;
        public override event Action<string> OnDataReceived;
        public override bool Connected => client?.Connected ?? false;

        protected Task listener;
        protected TcpClient client;
        protected NetworkStream stream;
        protected ControllerCommand currentCommand;
        protected CancellationTokenSource tokenSource;
        private CancellationToken token;

        public TCPController() => OnCurrentCommandChanged += (c) => currentCommand = c;
        private void Listen()
        {
            byte[] data = new byte[256];
            StringBuilder response = new StringBuilder();
            while (client?.Connected ?? false)
            {

                token.ThrowIfCancellationRequested();
                if (client?.Available > 0)
                    if (StringEndByte == 0)
                    {
                        while (stream.DataAvailable)
                            response.Append(Encoding.UTF8.GetString(data, 0,
                                            stream.Read(data, 0, data.Length)));
                        OnDataReceived?.Invoke(response.ToString());
                        response.Clear();
                    }
                    else
                    {
                        int i = 0;
                        byte b;
                        string str;
                        while (stream.DataAvailable)
                        {
                            b = (byte)stream.ReadByte();
                            data[i++] = b;
                            if (b == StringEndByte || i == 256) // если был заполнен последний байт буфера
                            {
                                str = Encoding.UTF8.GetString(data, 0, i);
                                OnDataReceived?.Invoke(str);
                                data = new byte[256];
                                i = 0;
                            }
                        }
                    }
                Thread.Sleep(CommandsFrequency);
            }
            Debug.WriteLine("listnerTCP is out");
        }
        public void SetCommand(string command, Action<bool>? a = null)
        {
            var c = new ControllerCommand() { Message = $"{command}" };
            c.OnCompleted += a;
            WriteCommand(c);
        }
        public override bool Connect(int AttemptCount)
        {
            if (IsSimulation)
            {
                queueWorker.DoWork += QueueWorker_DoSimulation;
                queueWorker.RunWorkerAsync();
                return true;
            }
            while (!client?.Connected ?? false || AttemptCount-- > 0)
                try
                {
                    client = new TcpClient(HostName, PortNum);
                    stream = client.GetStream();
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    listener = new Task(Listen, token);
                    listener.Start();
                    queueWorker.DoWork += QueueWorker_DoWork;
                    queueWorker.RunWorkerAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return false;
                }
            return true;
        }
        public override void Disconnect()
        {
            tokenSource.Cancel();
            //WriteMessageToController("Disconnect"); //TODO:убрать
            client?.Client?.Disconnect(false);
            stream?.Close();
            client?.Close();
            //↑много танцев с бубном

            queueWorker?.CancelAsync();
            queueBreaker.Set();
            queueWorker.DoWork -= QueueWorker_DoWork;

            OnDisconnected?.Invoke();
        }
        protected override bool WriteMessageToController(string message)
        {
            try
            {
                var s = message + (char)StringEndByte;
                var bytes = Encoding.UTF8.GetBytes(s);
                stream?.Write(bytes, 0, bytes.Length);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
