using ANG24.Sys.Application.Types.Enum;

namespace ANG24.Sys.Application.Interfaces
{
    public interface IMethodExecuter
    {
        /// <summary>
        /// Название метода (в точности как в классе)
        /// </summary>
        string MethodName { get; set; }
        /// <summary>
        /// Название оператора, который должен выполнить команду
        /// </summary>
        public string OperatorName { get; set; }
        /// <summary>
        /// Должна ли команда выполняться ассинхронно
        /// </summary>
        public bool IsAsync { get; set; }
        /// <summary>
        /// Параметры метода (типы c#)
        /// </summary>
        IList<object> Parameters { get; set; }
        /// <summary>
        /// Параметры метода (типы json)
        /// </summary>
        JsonElement JObjects { get; set; }
        /// <summary>
        /// срабатывает по завершению работы метода
        /// true если выполнено успешно
        /// </summary>
        Action<bool> Callback { get; set; }
        /// <summary>
        /// текущее состояние выполнения метода
        /// </summary>
        CommandState State { get; }
        /// <summary>
        /// текущее состояние выполнения метода в %
        /// </summary>
        byte ProgressPercent { get; }
        /// <summary>
        /// Срабатывает при изменении состояния выполнения команды
        /// </summary>
        event Action<CommandState> StateChanged;
        /// <summary>
        /// Срабатывает при изменении состояния выполнения команды в %
        /// </summary>
        event Action<byte> ProgressPercentChanged;
        /// <summary>
        /// Вызвать метод
        /// </summary>
        /// <param name="sender">объект класса, внутри которого будет выполняться метод</param>
        /// <param name="token">токен отмены</param>
        void Execute(object sender, CancellationToken token = default);
    }
}
