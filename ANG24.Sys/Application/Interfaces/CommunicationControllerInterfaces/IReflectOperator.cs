using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IReflectOperator : IControllerOperator<ReflectData>
    {
        event Action<ReflectOSCData> OnDataFromOscReceived;
        Channel CurrentChannel { get; }
        ReflectMode ReflectMode { get; set; }
        Mode CurrentMode { get; set; }
        Amplitude CurrentAmplitude { get; set; }
        Resistance CurrentResistance { get; set; }
        int IDMPulse { get; set; }
        int IDMDelay { get; set; }
        /// <summary>
        /// Добавить канал рефлектометру
        /// </summary>
        /// <param name="channel">канал</param>
        void AddChannel(int channel);
        /// <summary>
        /// Удалить канал рефлектометру
        /// </summary>
        /// <param name="channel">канал</param>
        void RemoveChannel(int channel);
        /// <summary>
        /// Переключатель сглаживаюшего фильтра
        /// </summary>
        void TurnSmoother();
        /// <summary>
        /// Сравнение каналов
        /// </summary>
        /// <param name="isCompare">включение\отключение функции</param>
        /// <param name="c1">первый канал сравнения</param>
        /// <param name="c2">второй канал сравнения</param>
        void CompareChannels(bool isCompare, int c1 = 0, int c2 = 0);
        /// <summary>
        /// Запустить считывание данных с осциллографа
        /// </summary>
        void Start();
        /// <summary>
        /// Остановить считывание данных с осциллографа
        /// </summary>
        void Stop();
        /// <summary>
        /// Сместить Х канала на offset
        /// </summary>
        /// <param name="offset">отступ от нуля</param>
        void ShiftReflectGraphOffset(int offset);
    }
}