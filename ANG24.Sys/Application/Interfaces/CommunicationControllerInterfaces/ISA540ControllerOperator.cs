using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface ISA540ControllerOperator : IControllerOperator<SA540Data>
    {
        event Action OnDisconnected;
        event Action<bool> OnStateChanged;
        event Action SendResult;
        event Action Clear;
        event Action<string> MeasStopped;
        string Mode { get; set; }
        void ClearMeas();
        void StopSC_KT();
        void StopIEN();
        void SetCommand(string? command, Action<bool>? a = null, string? commandResponse = null);
        /// <summary>
        /// Команда 0xFF “Установить связь с измерительным блоком”
        /// </summary>
        void ConnectToMeas();

        /// <summary>
        /// Установка напряжения XX
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
		/// 0 – однофазный трансформатор;
		/// 1 – трехфазный трансформатор;</param>
        /// <param name="Source">встроенный или внешний источник
		/// 0 – внешний источник;
		/// 1 – внутренний источник;</param>
        /// <param name="Triang">(1 или 0) схема соединения обмоток низкого напряжения (только одно из значений должно быть 1 остальные должны быть 0)</param>
        /// <param name="StarN">(1 или 0) схема соединения обмоток низкого напряжения (только одно из значений должно быть 1 остальные должны быть 0)</param>
        /// <param name="Star">(1 или 0) схема соединения обмоток низкого напряжения (только одно из значений должно быть 1 остальные должны быть 0)</param>
        /// <param name="Fase">
        /// отводы трансформаторов, которые будут обмерятся
		/// 0 – a-b;
		/// 1 – b-c;
		/// 2 – c-a</param>
        /// <param name="Volt">напряжение в вольтах, на котором будет проводится измерение (обычно 40, 100, 220, 380)</param>
        void InstallVoltage_IE(int TFases, int Source, int Triang, int StarN, int Star, int Fase, double Volt);
        /// <summary>
        /// Измерение в режиме XX
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        /// <param name="Source">встроенный или внешний источник
        /// 0 – внешний источник;
        /// 1 – внутренний источник;</param>
        /// <param name="Triang">(1 или 0) схема соединения обмоток низкого напряжения (только одно из значений должно быть 1 остальные должны быть 0)</param>
        /// <param name="StarN">(1 или 0) схема соединения обмоток низкого напряжения (только одно из значений должно быть 1 остальные должны быть 0)</param>
        /// <param name="Star">(1 или 0) схема соединения обмоток низкого напряжения (только одно из значений должно быть 1 остальные должны быть 0)</param>
        /// <param name="Fase">
        /// отводы трансформаторов, которые будут обмерятся
        /// 0 – a-b;
        /// 1 – b-c;
        /// 2 – c-a</param>
        /// <param name="Volt">напряжение в вольтах, на котором будет проводится измерение (обычно 40, 100, 220, 380)</param>
        /// <param name="N"> Количество измерений</param>
        /// <param name="Pz_XX">Заводское значение мощности потерь, где индекс номер фазы</param>
        void Meas_IE(int TFases, int Source, int Triang, int StarN, int Star, int Fase, double Volt, int N, double[] Pz_XX);
        /// <summary>
        /// Установка напряжения КЗ
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        /// <param name="Fase">
        /// отводы трансформаторов, которые будут обмерятся
        /// 0 – a-b;
        /// 1 – b-c;
        /// 2 – c-a</param>
        void InstallVoltage_SC(int TFases);
        /// <summary>
        /// Измерение в режиме КЗ
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        /// <param name="Order">Очередность измерений 
        /// 0 - последоавтельно;
        /// 1 - параллельно</param>
        /// <param name="N">Количество измерений</param>
        /// <param name="Zb_KZ">Базовое значение полного сопротивления</param>
        void Meas_SC(int TFases, int Order, int N, double[] Zb_KZ);
        /// <summary>
        /// Установка напряжения КТ
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        /// <param name="Source">встроенный или внешний источник
        /// 0 – внешний источник;
        /// 1 – внутренний источник;</param>
        /// <param name="Volt">напряжение в вольтах, на котором будет проводится измерение (обычно 40, 100, 220, 380)</param>
        void InstallVoltage_KT(int TFases, int Source, double Volt);
        /// <summary>
        ///  Измерение в режиме КТ
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        /// <param name="Source">встроенный или внешний источник
        /// 0 – внешний источник;
        /// 1 – внутренний источник;</param>
        /// <param name="Order">Очередность измерений 
        /// 0 - последоавтельно;
        /// 1 - параллельно</param>
        /// <param name="Volt">напряжение в вольтах, на котором будет проводится измерение (обычно 40, 100, 220, 380)</param>
        /// <param name="N">Количество измерений</param>
        /// <param name="Kz_KT">Заводское значение коэффициента трансформации, где индекс номер фазы</param>
        void Meas_KT(int TFases, int Source, int Order, double Volt, int N, double Kz_KT);
        /// <summary>
        /// Установка напряжения ХХН
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        void InstalVoltage_IEN(int TFases);
        /// <summary>
        /// Измерение в режиме ХХН
        /// </summary>
        /// <param name="TFases">количество фаз трансформатора
        /// 0 – однофазный трансформатор;
        /// 1 – трехфазный трансформатор;</param>
        /// <param name="Order">Очередность измерений 
        /// 0 - последоавтельно;
        /// 1 - параллельно</param>
        /// <param name="N">Количество измерений</param>
        /// <param name="Volt">напряжение в вольтах, на котором будет проводится измерение (обычно 260, 400)</param>
        void Meas_IEN(int TFases, int Order, int N, double Volt);
    }
}
