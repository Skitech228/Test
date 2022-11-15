using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using ANG24.Sys.Communication.Connections;
using ANG24.Sys.Communication.Interfaces;
using ANG24.Sys.Infrastructure.Logging;
using System.IO.Ports;

namespace ANG24.Sys.Communication.Operators.AbstractOperators
{
    public abstract class StringSerialControllerOperator<T> : IControllerOperator<T>, ISerialPortOperator where T : StringData, new()
    {
        public event Action OnDisconnected;
        public abstract event Action<T> OnDataReceived;
        public event Action<IControllerOperatorData> OnData;

        public string Name { get; protected set; }
        public bool Connected => Controller.Connected;
        public bool AutoLogging { get; set; } = true;
        public IMethodExecuter CurrentProcess { get; protected set; }

        protected ushort CommandsFrequency { get => Controller.CommandsFrequency; set => Controller.CommandsFrequency = value; }
        protected uint CommandAwaitInterval { get => Controller.CommandAwaitInterval; set => Controller.CommandAwaitInterval = value; }

        protected IController Controller;
        protected ControllerCommand currentCommand;
        protected AutoResetEvent commandAutoReset;

        public virtual void SetCommand(string command, Action<bool> action = null, string commandResponse = null)
        {
            var c = new ControllerCommand()
            {
                Message = command,
                CommandResponse = commandResponse,
            };
            //if (action == null)
            //    action = (success) =>
            //    {
            //        if (!success)
            //        {
            //            var data = new T();
            //            data.ParseData("");
            //            CommandBroker(data);
            //        }
            //    };
            c.OnCompleted += action;
            Controller.WriteCommand(c);
        }
        public virtual void SetCommand(string command)
        {
            if (AutoLogging)
                ControllerLogger.WriteString($"{Name}: {command}", true);
            Controller.WriteCommand(new ControllerCommand { Message = command });
        }

        public virtual void SetCommandPriority(string command)
        {
            if (AutoLogging)
                ControllerLogger.WriteString($"{Name}: {command}", true);
            Controller.WriteCommandPriority(new ControllerCommand() { Message = $"{command}" });
        }

        public virtual bool Connect() => Connect(1);
        public virtual bool Connect(int AttemptCount)
        {
            OnDataReceived -= DoAction;
            OnDataReceived += DoAction;
            return Controller?.Connect(AttemptCount) ?? false;
            void DoAction(T data) => OnData?.Invoke(data);
        }

        public void Disconnect() => Controller.Disconnect();
        public void StartQueue() => Controller.StartQueue();
        public void StopQueue() => Controller.StopQueue();
        public virtual void EmergencyStop()
        {
            Controller.EmergencyStop();
            Controller.ComamndDone(false);
        }
        public virtual void SetPort(SerialPort port)
        {
            Controller = new SerialPortController() { Port = port, Name = Name, connectionProtocol = SerialPortConnectionProtocol.Normal };
            Controller.OnDataReceived += Port_DataReceived;
            Controller.OnCurrentCommandChanged += (c) => currentCommand = c;
            Controller.OnDisconnected += () => { OnDisconnected?.Invoke(); };
        }
        protected virtual void Port_DataReceived(string response)
        {
            var tempCommand = currentCommand;
            var data = new T();
            AddDataInfo(ref data);
            data.ParseData(response);
            if (tempCommand != null)
            {
                if (!data.Success)
                {
                    Controller.CommandContinue();
                    return;
                }
                Controller.ComamndDone();
            }
            if (AutoLogging && !string.IsNullOrWhiteSpace(data.Message))
                ControllerLogger.WriteString($"{Name}: {data.Message}", false);
            CommandBroker(data);
        }
        protected virtual void AddDataInfo(ref T data) { }
        protected abstract void CommandBroker(T data);
    }
}
