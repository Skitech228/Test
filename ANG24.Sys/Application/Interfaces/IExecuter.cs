namespace ANG24.Sys.Application.Interfaces
{
    public interface IExecuter
    {
        /// <summary>
        /// Текущая команда
        /// </summary>
        IMethodExecuter CurrentProcess { get; }

        /// <summary>
        /// Вызвать исполнение команды
        /// </summary>
        /// <param name="executable"></param>
        void Execute(IMethodExecuter executable) => executable.Execute(this, new CancellationToken());
        /// <summary>
        /// Выполнить команду ассинхронно
        /// </summary>
        /// <param name="executable">команда</param>
        /// <returns>Получилось ли выполнить команду</returns>
        async Task ExecuteAsync(IMethodExecuter executable) => await Task.Factory.StartNew(() => Execute(executable));

    }
    public interface IExecuteableCreator
    {
        /// <summary>
        /// Текущая команда
        /// </summary>
        IMethodExecuter CurrentProcess { get; }
        /// <summary>
        /// Вызвать исполнение команды
        /// </summary>
        /// <param name="executable"></param>
        bool ExecuteCommand(Action<IMethodExecuter> command);
        /// <summary>
        /// Выполнить команду ассинхронно
        /// </summary>
        /// <param name="executable">команда</param>
        /// <returns>Получилось ли выполнить команду</returns>
        async Task<bool> ExecuteCommandAsync(Action<IMethodExecuter> command) => await Task.Factory.StartNew(() => ExecuteCommand(command));
    }
}