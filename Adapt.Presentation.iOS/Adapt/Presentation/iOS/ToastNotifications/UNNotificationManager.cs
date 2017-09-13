using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using UserNotifications;

namespace Adapt.Presentation.iOS.ToastNotifications
{
    public class UnNotificationManager
    {
        private readonly IDictionary<string, ManualResetEvent> _ResetEvents = new Dictionary<string, ManualResetEvent>();
        private readonly IDictionary<string, NotificationResult> _EventResult = new Dictionary<string, NotificationResult>();
        private int _Count;
        private static readonly object Lock = new object();

        public INotificationResult Notify(INotificationOptions options)
        {
            var notificationCenter = UNUserNotificationCenter.Current;

            var content = new UNMutableNotificationContent
            {
                Title = options.Title,
                Body = options.Description,
                Sound = UNNotificationSound.Default
            };
            UNNotificationTrigger trigger;

            if (options.DelayUntil.HasValue)
            {
                trigger = UNCalendarNotificationTrigger.CreateTrigger(options.DelayUntil.Value.ToNSDateComponents(), false);
            }
            else
            {
                trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false);
            }

            var id = _Count.ToString();
            _Count++;

            var request = UNNotificationRequest.FromIdentifier(id, content, trigger);
            notificationCenter.Delegate = new UserNotificationCenterDelegate(id, (identifier, notificationResult) =>
            {
                lock (Lock)
                {
                    if (_ResetEvents?.ContainsKey(identifier) == true && _EventResult?.ContainsKey(identifier) == false)
                    {
                        _EventResult.Add(identifier, notificationResult);
                        _ResetEvents[identifier].Set();
                    }
                }
            }, options.ClearFromHistory);

            var resetEvent = new ManualResetEvent(false);
            _ResetEvents.Add(id, resetEvent);

            notificationCenter.AddNotificationRequest(request, (error) =>
            {
                if (error != null)
                {
                    _EventResult?.Add(request.Identifier, new NotificationResult() { Action = NotificationAction.Failed });
                }
            });

            if (options.DelayUntil.HasValue)
            {
                return new NotificationResult() { Action = NotificationAction.NotApplicable };
            }

            resetEvent.WaitOne();

            var result = _EventResult[id];

            _ResetEvents.Remove(id);
            _EventResult.Remove(id);

            return result;
        }

        internal class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
        {
            private readonly Action<string, NotificationResult> _Action;
            private readonly string _Id;
            private readonly bool _Cancel;
            public UserNotificationCenterDelegate(string id, Action<string, NotificationResult> action, bool cancel)
            {
                _Action = action;
                _Id = id;
                _Cancel = cancel;
            }

            public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
            {
                // Timer here for a timeout since no Toast Dismissed Event (7 seconds til auto dismiss)
                NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(7), (nsTimer) =>
                {
                    _Action(_Id, new NotificationResult() { Action = NotificationAction.Timeout });

                    if (_Cancel) // Clear notification from list
                    {
                        UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new[] { _Id });
                    }

                    nsTimer.Invalidate();
                });

                // Shows toast on screen
                completionHandler(UNNotificationPresentationOptions.Alert);
            }


            public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
            {
                // I Clicked it :)
                _Action(_Id, new NotificationResult { Action = NotificationAction.Clicked });
            }

        }
    }
}
