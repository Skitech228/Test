using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface ISA640ControllerOperator : IControllerOperator<SA640Data>
    {
        event Action<int> OnNextDemagCycleStarted;
        event Action OnDisconnected;
        event Action OnTransformerChanged;
        event Action<SA640Transformer.Phase> OnNextPhaseMeasStarted;
        event Action<bool> OnStateChanged;

        /// <summary>
        /// Запуск алгоритма измерения сопротивления обмоток
        /// </summary>
        /// <param name="PercentTestI">Процент </param>
        /// <param name="mode">Режим измерения
        /// 0 - Автоматический
        /// 1 - Фазы
        /// 2 - Переключатель
        /// 3 - Единичный (ОБЯЗАТЕЛЬНО ОТПРАВИТЬ ФАЗУ ПОСЛЕДНИМ ПАРАМЕТРОМ)
        /// </param>
        /// <param name="reverseDirection">Обратное направление измерений</param>
        /// <param name="isFast">Быстрое измерения сопротивления</param>
        /// <param name="Phase">фаза для единичного измерения</param>
        void MeasR(double PercentTestI, int mode, bool reverseDirection = false, bool isFast = false, SA640Transformer.Phase? Phase = null);
        /// <summary>
        /// Размагнитизация
        /// </summary>
        /// <param name="demagMode">Режим размагнитизации
        /// 0 - Автоматический
        /// 1 - Фаза A
        /// 2 - Фаза B
        /// 3 - Фаза C
        /// </param>
        /// <param name="ITime">Время удержания тока</param>
        /// <param name="coil">интекс текущей обмотки трансформатора (из модели)</param>
        /// <param name="Ifirst">Ток начального цикла</param>
        /// <param name="Ilast">Ток конечного цикла</param>
        void Demagnetization(DemagMode demagMode, int ITime, int coil, double Ifirst = 0, double Ilast = 0);

        /// <summary>
        /// Генерация циклов для режима размагнитизации трансформатора
        /// </summary>
        /// <param name="coil">интекс текущей обмотки трансформатора (из модели)</param>
        /// <param name="Ifirst">Ток начального цикла</param>
        /// <param name="Ilast">Ток конечного цикла</param>
        /// <returns>Массив значений тока</returns>
        double[] GetCylesForDemagnitization(double Ifirst, double Ilast);
        /// <summary>
        /// Преждевременный переход на следующий цикл размагнитизации
        /// </summary>
        void NextDemagCycle();
        /// <summary>
        /// Команда 0x1F “Установить связь с измерительным блоком”
        /// </summary>
        void ConnectToMeas(Action<bool>? action = null);
        /// <summary>
        /// Изменить последовательность измерений сопротивления обмоток
        /// </summary>
        /// <param name="mode">Последовательность измерений</param>
        /// 0 - автоматический
        /// 1 - Фазы
        /// 2 - Переключатель
        /// 3 - Единичное
        /// <param name="reverseDirection"> направление измерений</param>
        void ChangeOrderMeasR(int mode, bool reverseDirection = false);
        /// <summary>
        /// Отправить команду непосредственно на контроллер (с ожиданием ответа)
        /// </summary>
        /// <param name="command">Комманда в формате "01 125 5 0 cs"</param>
        /// <param name="a">Действиет, по результату выполнения комманды (false - не получили ответа)</param>
        /// <param name="commandResponse">Номер команды ответа</param>
        void SetCommand(string? command, Action<bool>? a = null, string? commandResponse = null);
        /// <summary>
        /// Получить текущий трансформатор
        /// </summary>
        /// <returns></returns>
        SA640Transformer GetTransformer();
        /// <summary>
        /// Задать новый трансформатор
        /// </summary>
        /// <param name="transformer">готовый экземпляр трансформатора</param>
        void SetNewTransformer(SA640Transformer transformer);
    }
    public enum DemagMode
    {
        none,
        auto,
        //autoN,
        ab,
        bc,
        ca,
        //an,
        //bn,
        //cn
    }
}
