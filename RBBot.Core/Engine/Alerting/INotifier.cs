using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Alerting
{
    interface INotifier
    {
        NotificationType MinimumNotificationType { get; }

        Task Notify(NotificationType type, string message, params string[] parameters);
    }
}
