using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Application.Types.ServiceTypes;

namespace ANG24.Sys.Infrastructure.Services
{
    public sealed class NotificationService : Executer, INotificationService
    {
        public event Action<Notification> OnNotification;
        public event Action<int> OnNotificationClosed;
        private readonly Dictionary<int, Notification> notifications = new();
        private static int notificationId;
        private bool WaitTaskStarted;
        private Queue<Notification> notify;
        private Queue<Notification> NotificationQueue => notify ??= new Queue<Notification>();

        public NotificationService(Autofac.ILifetimeScope container) : base(container) { }

        public void Cancel(int notificationId)
        {
            if (!notifications.TryGetValue(notificationId, out var notify)) return;
            if (notify is null) return;
            if (notify.IsCancelButton)
                notify.OnCancel?.Invoke();
            notifications.Remove(notificationId);
        }
        public void Ok(int notificationId)
        {
            if (!notifications.TryGetValue(notificationId, out var notify)) return;
            if (notify is null) return;
            if (notify.IsOKButton)
                notify.OnOK?.Invoke();
            notifications.Remove(notificationId);
        }

        public int SendNotification(string message, int dismissDelay)
        {
            return SendNotification(new Notification
            {
                Id = notificationId++,
                Message = message,
                DismissDelay = dismissDelay,
            });
        }
        public int SendNotification(string message, string messageTitle, int dismissDelay)
        {
            return SendNotification(new Notification
            {
                Id = notificationId++,
                MessageTitle = messageTitle,
                Message = message,
                DismissDelay = dismissDelay,
            });
        }
        public int SendNotificationOK(string message, Action ok)
        {
            return SendNotification(new Notification
            {
                Id = notificationId++,
                Message = message,
                IsOKButton = true,
                OnOK = ok,
            });
        }
        public int SendNotificationOK(string message, string messageTitle, Action ok)
        {
            return SendNotification(new Notification
            {
                Id = notificationId++,
                MessageTitle = messageTitle,
                Message = message,
                IsOKButton = true,
                OnOK = ok,
            });
        }
        public int SendNotificationOKCancel(string message, Action ok, Action cancel)
        {
            return SendNotification(new Notification
            {
                Id = notificationId++,
                Message = message,
                IsOKButton = true,
                IsCancelButton = true,
                OnCancel = cancel,
                OnOK = ok,
            });
        }
        public int SendNotificationOKCancel(string message, string messageTitle, Action ok, Action cancel)
        {
            return SendNotification(new Notification
            {
                Id = notificationId++,
                MessageTitle = messageTitle,
                Message = message,
                IsOKButton = true,
                IsCancelButton = true,
                OnCancel = cancel,
                OnOK = ok,
            });
        }
        public void CloseNotification(int notificationId)
        {
            if (!notifications.TryGetValue(notificationId, out var notify)) return;
            notifications.Remove(notificationId);
            OnNotificationClosed?.Invoke(notificationId);
        }
        private int SendNotification(Notification notify)
        {
            notifications.Add(notify.Id, notify);
            if (OnNotification is null)
                AddNotificationToQueue(notify);
            else
                OnNotification?.Invoke(notify);

            if (notify.DismissDelay > 0)
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(notify.DismissDelay);
                    CloseNotification(notify.Id);
                });
            return notify.Id;

            void AddNotificationToQueue(Notification notify)
            {
                NotificationQueue.Enqueue(notify);
                if (!WaitTaskStarted)
                    Task.Factory.StartNew(async () =>
                    {
                        WaitTaskStarted = true;
                        // перед отправкой сообщения, ждём пока кто-нибудь не подключится к сервису
                        // срабатывает только на первое сообщение, можно поменять переставив строки местами
                        while (OnNotification is null) await Task.Delay(1000);
                        while (NotificationQueue.Count > 0)
                            OnNotification?.Invoke(NotificationQueue.Dequeue());
                        WaitTaskStarted = false;
                    });
            }
        }
    }
}
