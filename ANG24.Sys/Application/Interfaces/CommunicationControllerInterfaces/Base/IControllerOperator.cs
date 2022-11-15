namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base
{
    public interface IControllerOperator<out T> : IExecuter where T : IControllerOperatorData
    {
        Type DataType => typeof(T);
        /// <summary>
        /// Событие при получении данных от оператора
        /// </summary>
        event Action<T> OnDataReceived;
        /// <summary>
        /// Событие при получении данных от оператора
        /// </summary>
        event Action<IControllerOperatorData> OnData;
        /// <summary>
        /// Имя оператора
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Состояние подключения
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// Установка соединения с контроллером
        /// </summary>
        /// <param name="AttemptCount">Колличество попыток подключения</param>
        /// <returns>Получилось ли установить соединение</returns>
        bool Connect();
        bool Connect(int AttemptCount);
        /// <summary>
        /// Установка соединения с контроллером
        /// </summary>
        /// <returns>Получилось ли установить соединение</returns>
        async Task<bool> ConnectAsync() => await Task.Factory.StartNew(() => Connect());
        /// <summary>
        /// Разрыв соединения с контроллером
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Запустить очередь команд (по умолчанию запущена)
        /// </summary>
        void StartQueue();
        /// <summary>
        /// Остановить очередь команд
        /// </summary>
        void StopQueue();
        /// <summary>
        /// Экстренная остановка
        /// </summary>
        void EmergencyStop();
    }
}
