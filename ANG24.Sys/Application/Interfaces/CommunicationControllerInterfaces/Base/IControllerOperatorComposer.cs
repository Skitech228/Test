using ANG24.Sys.Application.Core.DTO;
using ANG24.Sys.Application.Types;
using ANG24.Sys.Application.Types.Enum;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base
{
    public interface IControllerOperatorComposer : IExecuter, IExecuteableCreator
    {
        event Action<OperatorMessageDto> OnDataReceived;
        /// <summary>
        /// Список всех доступных операторов
        /// </summary>
        IDictionary<LabController, IControllerOperator<IControllerOperatorData>> Operators { get; }
        /// <summary>
        /// Получение всех публичных методов оператора
        /// и их сигнатур
        /// </summary>
        /// <param name="operName">Имя сервиса</param>
        /// <returns></returns>
        Task<IEnumerable<Method>> GetOperatorCommands(string operName);
        /// <summary>
        /// Выполнить скрипт
        /// </summary>
        /// <param name="script">скрипт</param>
        /// <param name="imports">Импортируемые пространства имён (через пробел)</param>
        /// <returns>Получилось ли выполнить скрипт</returns>
        Task<bool> StartScript(string script, string imports);
        ///// <summary>
        ///// Проверить скрипт на возможность компиляции
        ///// </summary>
        ///// <param name="script">скрипт</param>
        ///// <param name="imports">Импортируемые пространства имён (через пробел)</param>
        ///// <returns>Получилось ли выполнить скрипт</returns>
        //Task<string> CheckScript(string script, string imports);
        /// <summary>
        /// Подписка на события конкретного сервиса
        /// </summary>
        /// <param name="operName">Имя оператора</param>
        /// <returns></returns>
        Task Subscribe(string operName);
        Task Unsubscribe(string operName);
        /// <summary>
        /// Запуск алгоритма поиска контроллеров (после выполнения требуется перезагрузка сервера)
        /// </summary>
        /// <returns></returns>
        Task<bool> SearchForControllers();
    }
}
