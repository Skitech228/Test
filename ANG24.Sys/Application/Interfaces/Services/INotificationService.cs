using ANG24.Sys.Application.Types.ServiceTypes;

namespace ANG24.Sys.Application.Interfaces.Services
{
    public interface INotificationService : IExecuteableCreator
    {
        /// <summary>
        /// Срабатывает при создании нового оповещения
        /// </summary>
        event Action<Notification> OnNotification;
        /// <summary>
        /// Срабатывает при закрытии оповещения со стороны сервера 
        /// передаёт в метод ID оповещения
        /// </summary>
        event Action<int> OnNotificationClosed;
        #region методы клиента
        /// <summary>
        /// Вызывает метод по нажатию на кнопку ok
        /// </summary>
        /// <param name="notificationId">ID оповещения</param>
        void Ok(int notificationId);
        /// <summary>
        /// Вызывает метод по нажатию на кнопку cancel
        /// </summary>
        /// <param name="notificationId">ID оповещения</param>
        void Cancel(int notificationId);
        /// <summary>
        /// Удалить оповещение из списка сервера
        /// </summary>
        /// <param name="notificationId">ID оповещения</param>
        void CloseNotification(int notificationId);
        #endregion
        #region методы сервера-
        /// <summary>
        /// Создать оповещение без кнопок
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="dismissDelay">задержка скрытия</param>
        /// <returns>ID оповещения</returns>
        int SendNotification(string message, int dismissDelay);
        /// <summary>
        /// Создать оповещение без кнопок
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="messageTitle">Заголовок</param>
        /// <param name="dismissDelay">задержка скрытия</param>
        /// <returns>ID оповещения</returns>
        int SendNotification(string message, string messageTitle, int dismissDelay);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="ok">Метод, срабатывающий при нажатии на кнопку ОК</param>
        /// <returns>ID оповещения</returns>
        int SendNotificationOK(string message, Action ok);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="messageTitle">Заголовок</param>
        /// <param name="ok">Метод, срабатывающий при нажатии на кнопку ОК</param>
        /// <returns>ID оповещения</returns>
        int SendNotificationOK(string message, string messageTitle, Action ok);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="ok">Метод, срабатывающий при нажатии на кнопку ОК</param>
        /// <param name="cancel">Метод, срабатывающий при нажатии на кнопку Cancel</param>
        /// <returns>ID оповещения</returns>
        int SendNotificationOKCancel(string message, Action ok, Action cancel);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="messageTitle">Заголовок</param>
        /// <param name="ok">Метод, срабатывающий при нажатии на кнопку ОК</param>
        /// <param name="cancel">Метод, срабатывающий при нажатии на кнопку Cancel<</param>
        /// <returns>ID оповещения</returns>
        int SendNotificationOKCancel(string message, string messageTitle, Action ok, Action cancel);
        #endregion
    }
}
