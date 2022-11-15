namespace ANG24.Sys.Communication.Interfaces
{
    internal interface ISerialPortController : IController
    {
        void SubscirbePortNotifications();
        void UnsubscirbePortNotifications();
        void SetDataReceivedTrigger(bool subscribe);
        byte[] ApplyCommand(byte[] message, int delay = 100);
        string ApplyCommand(string message, int delay = 100);
        System.IO.Ports.SerialPort GetPort();
    }
}