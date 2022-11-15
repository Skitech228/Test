using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using ANG24.Sys.Communication.Connections;
using ANG24.Sys.Communication.Interfaces;
using ANG24.Sys.Infrastructure.Logging;

namespace ANG24.Sys.Communication.Operators.AbstractOperators
{
    public abstract class TCPControllerOperator<T> : IControllerOperator<T> where T : TCPData, new()
    {
        protected IController Controller;
        protected ControllerCommand currentCommand;
        public event Action OnDisconnected;
        public abstract event Action<T> OnDataReceived;
        public event Action<IControllerOperatorData> OnData;

        public bool AutoLogging { get; set; } = true;
        public abstract string Name { get; }
        public IMethodExecuter CurrentProcess { get; protected set; }
        public bool Connected => Controller.Connected;
        public ushort CommandsFrequency { get => Controller.CommandsFrequency; set => Controller.CommandsFrequency = value; }

        public TCPControllerOperator()
        {
            Controller = new TCPController();
            Controller.OnDataReceived += TCP_DataReceived;
            CommandsFrequency = 200;
            Controller.OnCurrentCommandChanged += (c) => currentCommand = c;
            Controller.OnDisconnected += () => { OnDisconnected?.Invoke(); };
        }
        public bool Connect() => Controller.Connect(1);
        public bool Connect(int AttemptCount)
        {
            OnDataReceived -= DoAction;
            OnDataReceived += DoAction;
            return Controller?.Connect(AttemptCount) ?? false;
            void DoAction(T data) => OnData?.Invoke(data);
        }

        public void Disconnect() => Controller.Disconnect();
        public void StartQueue() => Controller.StartQueue();
        public void StopQueue() => Controller.StopQueue();
        public abstract void EmergencyStop();

        private void TCP_DataReceived(string response)
        {
            var tempCommand = currentCommand;
            var data = new T();
            data.ParseData(response);
            if (tempCommand != null)
            {
                if (tempCommand.Command == data.Command)
                    Controller.ComamndDone();
                else if (data.Command == "error") // Ответ при возникновении ошибки 
                {
                    Controller.ComamndDone();
                }
            }
            if (AutoLogging && !string.IsNullOrWhiteSpace(data.Message))
                ControllerLogger.WriteString($"{Name}: {data.Message}", false);
            CommandBroker(data);
        }
        internal void SetPort(ushort portNum, string hostName, byte stringEndByte = 0)
        {
            (Controller as TCPController).PortNum = portNum;
            (Controller as TCPController).HostName = hostName;
            (Controller as TCPController).StringEndByte = stringEndByte;
        }

        protected abstract void CommandBroker(T data);
        protected virtual void SetCommand(string command, Action<bool> a = null)
        {
            if (AutoLogging)
                ControllerLogger.WriteString($"{Name}: {command}", true);
            var c = new ControllerCommand() { Message = $"{command}", Command = command.Split(' ')[0] };
            c.OnCompleted += a;
            Controller.WriteCommand(c);
        }
        protected virtual void SetCommandPriority(string command)
        {
            if (AutoLogging)
                ControllerLogger.WriteString($"{Name}: {command}", true);
            Controller.WriteCommandPriority(new ControllerCommand() { Message = $"{command}", Command = command.Split(' ')[0] });
        }
    }
}
