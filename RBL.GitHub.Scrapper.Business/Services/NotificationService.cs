using RBL.GitHub.Scrapper.Business.Interfaces;
using RBL.GitHub.Scrapper.Business.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RBL.GitHub.Scrapper.Business.Services
{
    public class NotificationService
    {
        private readonly INotifier _notifier;
        public NotificationService(INotifier notifier)
        {
            _notifier = notifier;
        }

        protected void Notify(RBL.GitHub.Scrapper.Business.Validations.ValidationResult result)
        {
            result.Errors.ForEach(error => _notifier.Handle(new Notification(error)));
        }

        protected void Notify(string message)
        {
            _notifier.Handle(new Notification(message));
        }
    }
}
