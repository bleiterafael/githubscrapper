using RBL.GitHub.Scrapper.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RBL.GitHub.Scrapper.Business.Notifications
{
    public class Notifier : INotifier
    {
        private List<Notification> _notifications;
        public Notifier()
        {
            _notifications = new List<Notification>();
        }

        public void Handle(Notification notification) => _notifications.Add(notification);
        public List<Notification> GetNotifications() => _notifications;
        public bool HasNotification() => _notifications.Any();

        public override string ToString()
        {
            var notifications = string.Join('\n', _notifications.Select(n => n.Message));
            return notifications;
        }
    }
}
