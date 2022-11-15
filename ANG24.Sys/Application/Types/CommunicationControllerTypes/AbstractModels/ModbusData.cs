using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using System.Text;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels
{
    public abstract class ModbusData : IControllerOperatorData
    {
        public ModbusData() { }
        public virtual void ParseByteData(byte[] input)
        {
            if (string.IsNullOrEmpty(Message))
            {
                var str = new StringBuilder();
                if (data != null)
                    foreach (var d in data)
                        str.Append($"{d} ");

                str.Remove(str.Length - 1, 1);
                Message = str.ToString();
            }
        }
        public virtual void ParseData(string message) => ParseByteData(message.Split().Select(byte.Parse).ToArray());

        protected byte[]? data;
        public bool Success { get; protected set; }
        public string? OptionalInfo { get; set; }
        public string? Message { get; protected set; }
        /// <summary>
        /// Код устройства. 
        /// </summary>
        public byte? ControllerId { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public byte? Command { get; set; }
        /// <summary>
        /// Количество байт в посылке, 
        /// Допустимые значения 5…2500.
        /// </summary>
        public ushort? Size { get; protected set; }
        /// <summary>
        /// Контрольная сумма рассчитывается как 
        /// “исключающее или” всех байт в посылке
        /// </summary>
        public ushort? CS { get; protected set; } // для 640/540 мостов - однобайтная CS
        public int ErrorCode { get; protected set; }
        public string? ErrorMessage { get; protected set; }

        protected float ToFloat(byte[] input) =>
            BitConverter.ToSingle(new[] { input[0], input[1], input[2], input[3] }, 0);
    }
}
