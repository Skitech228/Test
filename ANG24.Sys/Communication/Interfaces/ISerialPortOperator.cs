using System.IO.Ports;

namespace ANG24.Sys.Communication.Interfaces
{
    public interface ISerialPortOperator
    {
        void SetPort(SerialPort port);
    }
}