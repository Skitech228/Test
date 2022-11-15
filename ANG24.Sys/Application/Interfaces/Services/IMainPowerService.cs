using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace ANG24.Sys.Application.Interfaces.Services;

public interface IMainPowerService : IExecuteableCreator
{
    event Action<StartVisualState> StartStateChanged;
    event Action<bool> WorkStateChanged;
    StartVisualState StartState { get; set; }
    /// <summary>
    /// Текущее состояние лабаратории
    /// </summary>
    bool WorkState { get; set; }
    /// <summary>
    /// Попытка сменить состояние питания лабаратории (подача питания)
    /// </summary>
    void Run();
}
