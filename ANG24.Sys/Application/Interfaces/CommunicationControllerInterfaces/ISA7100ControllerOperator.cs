using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface ISA7100ControllerOperator : IControllerOperator<SA7100Data>
    {
        event Action OnDisconnected;
        event Action OnPower;
        bool IsPowerOn { get; }
        #region Команды
        /// <summary>
        /// Отправить команду на контроллер
        /// </summary>
        /// <param name="message">команда</param>
        void SendMessage(string message);
        void SetTestObjectInfo(string ObjectName, string ObjectNote, string FactoryNumber, string ReleaseDate);
        /// <summary>
        /// Включение питания
        /// </summary>
        void PowerOn();
        /// <summary>
        /// Отключение питания
        /// </summary>
        void PowerOff();
        /// <summary>
        /// Отменяет выполнени последней отправленной команды
        /// </summary>
        void CancelCurrentCommand();
        /// <summary>
        /// Установка лицензионного ключа
        /// </summary>
        /// <param name="key"></param>
        void SetLicense(string key);
        /// <summary>
        /// Установка градуировочных коэффициентов 
        /// </summary>
        void SetCoeffsFromCU();
        /// <summary>
        /// Получение текущего состояния измерительной системы
        /// </summary>
        void CurrentState();
        /// <summary>
        /// Получение следующего состояния измерительной системы, если оно известно
        /// </summary>
        void NextState();
        /// <summary>
        /// Измерение Cx и Tgx
        /// </summary>
        /// <param name="avg">Количество усреднений при измерении</param>
        /// <param name="c0">Емкость эталонного конденсатора в фарадах</param>
        /// <param name="tg0">Тангенс эталонного конденсатора</param>
        void MeasureCX(int avg, double c0 = 0, double tg0 = 0);
        /// <summary>
        /// Измерение Ux, Fq
        /// </summary>
        /// <param name="c0">Емкость эталонного конденсатора в фарадах</param>
        /// <param name="tg0">Тангенс эталонного конденсатора</param>
        void MeasureUX(double c0 = 0, double tg0 = 0);
        /// <summary>
        /// Измерение Rx
        /// </summary>
        /// <param name="uset">	Устанавливаемое напряжение в вольтах (100 ... 2500) (</param>
        void MeasureRX(int uset);
        void ChangeMeasScheme(bool isReverse);
        #endregion
    }
}
