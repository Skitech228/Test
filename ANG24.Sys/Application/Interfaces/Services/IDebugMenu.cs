using ANG24.Sys.Application.Types.ServiceTypes;

namespace ANG24.Sys.Application.Interfaces.Services;
public interface IDebugMenu : IExecuteableCreator
{
    /// <summary>
    /// Срабатывает при изменении состояния флага
    /// </summary>
    event Action<string, bool> OnFlagChanged;
    /// <summary>
    /// получить состояния всех флагов
    /// </summary>
    /// <returns>Возвращает состояния всех флагов</returns>
    DebugFlags GetCurrentState();
    /// <summary>
    /// Задать состояние флага
    /// </summary>
    /// <param name="Flag">название флага</param>
    /// <param name="state">состояние</param>
    void ChangeFlag(string Flag, bool state);
    /// <summary>
    /// Задать состояние всем флагам
    /// </summary>
    /// <param name="state">состояние</param>
    void ChangeAllFlags(bool state);
}
