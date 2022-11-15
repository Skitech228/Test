using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Application.Interfaces.Services;

public interface ILabVoltageSafeguard : IExecuteableCreator
{
    /// <summary>
    /// Задать значение напряжения для проверок защиты
    /// </summary>
    /// <param name="module">модуль</param>
    /// <param name="voltage">напряжение</param>
    /// <param name="altVoltage">дополнительное напряжение (?) (зависит от модуля)</param>
    void SetVoltage(LabModule module, double voltage, double altVoltage);
}
