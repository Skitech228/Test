namespace ANG24.Sys.Communication.Interfaces
{
    public interface IController
    {
        /// <summary>
        /// Событие при отключении от контроллера
        /// </summary>
        event Action OnDisconnected;
        /// <summary>
        /// Событие при получении данных от контроллера
        /// </summary>
        event Action<string> OnDataReceived;
        /// <summary>
        /// Событие при изменении текущей команды
        /// </summary>
        event Action<ControllerCommand> OnCurrentCommandChanged;
        /// <summary>
        /// Число попыток отправки команды на контроллер
        /// </summary>
        byte WriteTryCount { get; set; }
        /// <summary>
        /// Текущая комманда в очереди
        /// </summary>
        ControllerCommand CurrentCommand { get; set; }
        /// <summary>
        /// Требуемая частота отправки данных (2 - 10 000 мс)
        /// </summary>
        ushort CommandsFrequency { get; set; }
        /// <summary>
        /// Таймер ожидания ответа на комманду
        /// </summary>
        uint CommandAwaitInterval { get; set; }
        /// <summary>
        /// Режим симуляции
        /// </summary>
        bool IsSimulation { get; set; }
        /// <summary>
        /// Состояние подключения
        /// </summary>
        bool Connected { get; }
        #region Команды
        /// <summary>
        /// Установка соединения с контроллером
        /// </summary>
        /// <param name="AttemptCount">Колличество попыток подключения</param>
        /// <returns></returns>
        bool Connect(int AttemptCount = 5);
        /// <summary>
        /// Разрыв соединения с контроллером
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Прекратить попытки отправки комманды 
        /// </summary>
        /// <returns>Колличество оставшихся в очереди комманд</returns>
        void ComamndDone(bool done = true);
        /// <summary>
        /// Повторить отправку команды в порт
        /// </summary>
        /// <returns>Колличество оставшихся попыток (WriteTryCount)</returns>
        byte CommandContinue();
        /// <summary>
        /// отправка команды в очередь
        /// </summary>
        /// <param name="command">Команда</param>
        void WriteCommand(ControllerCommand command);
        /// <summary>
        /// Отправка команды непосредственно в порт устройства
        /// не ожидая ответа
        /// </summary>
        /// <param name="command">Команда</param>
        void WriteCommandPriority(ControllerCommand command);
        void StartQueue();
        void StopQueue();
        /// <summary>
        /// Экстренная остановка
        /// </summary>
        void EmergencyStop();

        #endregion
    }
}
