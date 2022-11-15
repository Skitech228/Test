using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using ANG24.Sys.Communication.Connections;
using ANG24.Sys.Communication.Interfaces;
using ANG24.Sys.Infrastructure.Logging;
using System.IO.Ports;
using System.Text;

namespace ANG24.Sys.Communication.Operators.AbstractOperators
{
    public abstract class ModBusControllerOperator<T> : IControllerOperator<T>, ISerialPortOperator where T : ModbusData, new()
    {
        public bool Connected => Controller.Connected;
        public ushort CommandsFrequency { get => Controller.CommandsFrequency; set => Controller.CommandsFrequency = value; }
        public uint CommandAwaitInterval { get => Controller.CommandAwaitInterval; set => Controller.CommandAwaitInterval = value; }
        public IMethodExecuter CurrentProcess { get; protected set; }
        public bool AutoLogging { get; set; } = false;
        public string Name { get; protected set; }

        public event Action OnDisconnected;
        public abstract event Action<T> OnDataReceived;
        public event Action<IControllerOperatorData> OnData;

        protected IController Controller;
        protected ControllerCommand currentCommand;
        protected AutoResetEvent commandAutoReset;

        public virtual void SetCommand(string command, Action<bool> action = null, string commandResponse = null)
        {
            if (AutoLogging)
                ControllerLogger.WriteString($"{Name}: {command}", true);
            var messageArray = command.Split(' ');
            var c = new ControllerCommand()
            {
                Message = AddCS(messageArray),
                Command = messageArray[1],
                CommandResponse = commandResponse,
            };
            if (action == null)
                action = (success) =>
                {
                    if (!success)
                    {
                        var data = new T();
                        data.ParseByteData(new byte[] { 2, 0, 255, 253 });
                        CommandBroker(data);
                    }
                };
            c.OnCompleted += action;
            Controller.WriteCommand(c);
        }
        public virtual void SetCommandPriority(string command)
        {
            if (AutoLogging)
                ControllerLogger.WriteString($"{Name}: {command}", true);
            Controller.WriteCommandPriority(new ControllerCommand() { Message = $"{command}" });
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
        public virtual void SetPort(SerialPort port)
        {
            Controller = new SerialPortController()
            {
                Port = port,
                Name = Name,
                connectionProtocol = SerialPortConnectionProtocol.Modbus,
            };
            (Controller as SerialPortController).OnBytesReceived += Port_BytesReceived;
            Controller.OnCurrentCommandChanged += (c) => currentCommand = c;
            Controller.OnDisconnected += () => { OnDisconnected?.Invoke(); };
        }
        protected virtual void Port_BytesReceived(byte[] response)
        {
            var tempCommand = currentCommand;
            var data = new T();
            data.ParseByteData(response);
            if (tempCommand != null)
            {
                if (byte.TryParse(tempCommand.Command, out byte b) && b == data.Command)
                    Controller.ComamndDone();
                else if (data.Command == 0) // Ответ при возникновении ошибки 
                {
                    EmergencyStop();
                    Debug.WriteLine("Ошибка от контроллера");
                }
            }
            if (AutoLogging && !string.IsNullOrWhiteSpace(data.Message))
                ControllerLogger.WriteString($"{Name}: {data.Message}", false);
            CommandBroker(data);
        }
        protected abstract void CommandBroker(T data);
        protected string AddCS(string[] str)
        {
            byte cs = 0;
            foreach (var item in str)
                cs ^= byte.Parse(item);
            StringBuilder stringg = new StringBuilder();
            foreach (var item in str)
                stringg.Append(item + " ");
            stringg.Append(cs);
            return stringg.ToString();
        }
    }
}
