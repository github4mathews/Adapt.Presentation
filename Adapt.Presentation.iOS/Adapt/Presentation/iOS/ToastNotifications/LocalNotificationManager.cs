using Foundation;
using UIKit;

namespace Adapt.Presentation.iOS.ToastNotifications
{
    public class LocalNotificationManager
    {
        private static object _lock = new object();

        public INotificationResult Notify(INotificationOptions options)
        {
            // create the notification
            var notification = new UILocalNotification
            {
                // set the fire date (the date time in which it will fire)
                FireDate = options.DelayUntil == null ? NSDate.Now : options.DelayUntil.Value.ToNSDate(),

                // configure the alert
                AlertTitle = options.Title,
                AlertBody = options.Description,

                // set the sound to be the default sound
                SoundName = UILocalNotification.DefaultSoundName
            };

            if (options.CustomArgs != null)
            {
                NSMutableDictionary dictionary = new NSMutableDictionary();
                foreach (var arg in options.CustomArgs)
                {
                    dictionary.SetValueForKey(NSObject.FromObject(arg.Value), new NSString(arg.Key));
                }

                // Don't document, this feature is most likely to change
                dictionary.SetValueForKey(NSObject.FromObject(System.Guid.NewGuid().ToString()), new NSString("Identifier"));

                notification.UserInfo = dictionary;
            }
            // schedule it
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);

            return new NotificationResult() { Action = NotificationAction.NotApplicable };
        }
    }
}
