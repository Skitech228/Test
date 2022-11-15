using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IVoltageRegulatorOperator : IControllerOperator<VoltageRegulatorData>
    {
        /// <summary>
        /// запросить уровень напряжения
        /// </summary>
        void GetVoltageLevel();
        /// <summary>
        /// установить уровень напряжения
        /// </summary>
        /// <param name="level">уровень напряжения 0-1982</param>
        void SetVoltageLevel(int level);
        /// <summary>
        /// установить генерацию синуса по уровню напряжения
        /// </summary>
        /// <param name="f">
        /// 0.01
        /// 0.02
        /// 0.05
        /// 0.1
        /// </param>
        /// <param name="level">уровень(%)</param>
        void StartSinWithVoltLevel(double f, int level);
        /// <summary>
        /// установить процент ограничения тока
        /// </summary>
        /// <param name="percent">
        /// 1--25%
        /// 2--50%
        /// 3--75%
        /// 4--100%
        /// </param>
        void SetCurentLimitPercent(int percent);
        /// <summary>
        /// подьем напряжения на определенное время
        /// </summary>
        /// <param name="level">напряжение(0-1982)</param>
        /// <param name="time">время(с)</param>
        void SetVoltageForTime(int level, int time);

        /// <summary>
        /// установить генерацию синуса без изменения напряжения
        /// </summary>
        /// <param name="f"></param>
        void StartSin(double f);

        /// <summary>
        /// Получить ограничение
        /// </summary>
        void GetLimit();


        /// <summary>
        /// сила срабатывания магнита (a) смены полярности 
        /// </summary>
        /// <param name="level">Уровень силы 0-999</param>
        void SetMagnitATriggerLevel(int level);
        /// <summary>
        /// время срабатывания магнита (a) смены полярности
        /// </summary>
        /// <param name="time">ms</param>
        void SetMagnitATriggerTime(int time);
        /// <summary>
        /// сила удержания магнита (a) смены полярности 
        /// </summary>
        /// <param name="level">Уровень силы 0-999</param>
        void SetMagnitAHoldLevel(int level);

        /// <summary>
        /// сила срабатывания магнита (b) разрядника
        /// </summary>
        /// <param name="level">0-999 (level)</param>
        void SetMagnitBTrigerLevel(int level);
        /// <summary>
        /// время срабатывания магнита (b) разрядника
        /// </summary>
        /// <param name="time">ms</param>
        void SetMagnitBTriggerTime(int time);
        /// <summary>
        /// сила удержания магнита (b) разрядника
        /// </summary>
        /// <param name="level">0-999 (level)</param>
        void SetMagnitBHoldLevel(int level);

        /// <summary>
        /// полная остановка всего
        /// </summary>
        void Stop();
    }
}