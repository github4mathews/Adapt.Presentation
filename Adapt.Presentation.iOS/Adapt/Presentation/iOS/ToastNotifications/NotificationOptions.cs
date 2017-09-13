using System;
using System.Collections.Generic;

namespace Adapt.Presentation.iOS.ToastNotifications
{
    public class NotificationOptions : INotificationOptions
    {
        public string Description { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public bool IsClickable { get; set; } = false;
        
        public IDictionary<string, string> CustomArgs { get; set; } = new Dictionary<string, string>();

        public bool ClearFromHistory { get; set; } = false;

        public DateTime? DelayUntil { get; set; } = null;
    }
}
