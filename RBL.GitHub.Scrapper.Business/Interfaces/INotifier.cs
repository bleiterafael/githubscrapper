using RBL.GitHub.Scrapper.Business.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace RBL.GitHub.Scrapper.Business.Interfaces
{
    public interface INotifier
    {
        bool HasNotification();
        List<Notification> GetNotifications();
        void Handle(Notification notification);
        string ToString();
    }
}
