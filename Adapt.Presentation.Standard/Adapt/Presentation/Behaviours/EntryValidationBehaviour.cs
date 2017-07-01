using System.ComponentModel;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
    public static class EntryValidationBehaviour
    {
        public static readonly BindableProperty AttachBehaviorProperty =
            BindableProperty.CreateAttached(
                "AttachBehavior",
                typeof(bool),
                typeof(EntryValidationBehaviour),
                false,
                propertyChanged: OnAttachBehaviorChanged);

        public static bool GetAttachBehavior(BindableObject view)
        {
            return (bool)view.GetValue(AttachBehaviorProperty);
        }

        public static void SetAttachBehavior(BindableObject view, bool value)
        {
            view.SetValue(AttachBehaviorProperty, value);
        }

        private static void OnAttachBehaviorChanged(BindableObject view, object oldValue, object newValue)
        {
            var entry = view as Entry;
            if (entry == null)
            {
                return;
            }

            var attachBehavior = (bool)newValue;
            if (attachBehavior)
            {
                entry.TextChanged += OnEntryTextChanged;
            }
            else
            {
                entry.TextChanged -= OnEntryTextChanged;
            }
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            var entry = sender as Entry;

            var notifyDataErrorInfo = entry?.BindingContext as INotifyDataErrorInfo;
            if (notifyDataErrorInfo == null)
            {
                return;
            }

            entry.BackgroundColor = notifyDataErrorInfo.HasErrors ? Color.Red : Color.Default;
        }
    }
}
