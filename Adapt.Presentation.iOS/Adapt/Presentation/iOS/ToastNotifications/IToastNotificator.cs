using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adapt.Presentation.iOS.ToastNotifications
{
    public interface IToastNotifier
    {
        /// <summary>
        /// Show a Toast notification
        /// </summary>
        Task<INotificationResult> Notify(INotificationOptions options);

        /// <summary>
        /// Shows a Toast then runs the callback. Will not wait for the toast action to be completed but will call the callback instead when complete.
        /// </summary>
        void Notify(Action<INotificationResult> callback, INotificationOptions options);

        /// <summary>
        /// Delivered Notifications to the phone through the Toast Plugin
        /// UWP, iOS or Android >= API23 only.
        /// </summary>
        Task<IList<INotification>> GetDeliveredNotifications();

        /// <summary>
        /// Cancels all currently showing or previously shown notifications
        /// </summary>
        void CancelAllDelivered();

    }
}