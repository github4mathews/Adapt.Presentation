using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace Adapt.Presentation.iOS.ToastNotifications
{
    public class InAppNotification : IInAppNotification
    {
        #region Private Fields
        private readonly UnNotificationManager _NotificationManager;
        private readonly LocalNotificationManager _LocalNotificationManager;
        #endregion

        #region Constructor
        public InAppNotification()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                _NotificationManager = new UnNotificationManager();
            }
            else
            {
                _LocalNotificationManager = new LocalNotificationManager();
            }
        }
        #endregion

        #region Private Methods
        private async Task<INotificationResult> Notify(INotificationOptions options)
        {
            INotificationResult result = null;
            return await Task.Run(() =>
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {
                    return _NotificationManager.Notify(options);
                }
                else
                {
                    ManualResetEvent reset = new ManualResetEvent(false);
                    UIApplication.SharedApplication.InvokeOnMainThread(() => { result = _LocalNotificationManager.Notify(options); reset.Set(); });
                    reset.WaitOne();
                    return result;
                }

            });
        }

        private void Notify(Action<INotificationResult> callback, INotificationOptions options)
        {
            Task.Run(async () => callback(await Notify(options)));
        }

        private void CancelAllDelivered()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var notificationCenter = UNUserNotificationCenter.Current;

                notificationCenter.RemoveAllDeliveredNotifications();
            }
        }
        #endregion

        #region Public Methods
        public async void Show(string text)
        {
            await Notify(new NotificationOptions() { Title = text, Description = text });
        }
        #endregion
    }
}
