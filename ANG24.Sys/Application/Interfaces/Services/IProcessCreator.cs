namespace ANG24.Sys.Application.Interfaces.Services
{
    public interface IProcessCreator : IExecuteableCreator
    {
        /// <summary>
        /// Срабатывает при закрытии процесса
        /// </summary>
        event Action OnExit;
        /// <summary>
        /// Принудительное закрытие процесса
        /// </summary>
        void KillProcess();
        /// <summary>
        /// Запустить процесс
        /// </summary>
        /// <param name="path">Полный путь к исполняемому файлу</param>
        /// <returns>Получилось ли запустить</returns>
        bool StartProcess(string path);
    }
}