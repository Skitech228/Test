using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;

namespace ANG24.Sys.Communication.Operators.AbstractOperators
{
    //Null object pattern
    public class NullOperator : IControllerOperator<StringData>
    {
        public NullOperator()
        {
            OnDataReceived?.Invoke(null);
            OnDataReceived += (data) => OnData?.Invoke(data);
        }

        public string Name => "null";
        public bool Connected => false;
        public IMethodExecuter CurrentProcess => null;

        public event Action<IControllerOperatorData> OnData;

        public event Action<StringData> OnDataReceived;
        public bool Connect() => false;
        public bool Connect(int AttemptCount) => false;
        public void Disconnect() { }
        public void EmergencyStop() { }
        public void StartQueue() { }
        public void StopQueue() { }
    }
}
