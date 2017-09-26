using System.Collections;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
    public class RemoveFromCollectionBehavior : ButtonCollectionBehaviourBase
    {
        #region BindableProperty
        #region RemoveItems
        public static readonly BindableProperty RemoveItemsProperty =
        BindableProperty.CreateAttached(
        "RemoveItems",
        typeof(IList),
        typeof(Button),
        null,
        propertyChanged: OnRemoveItemsChanged);

        private static void OnRemoveItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetRemoveItems(bindable, (IList)newValue);
        }

        public static IList GetRemoveItems(BindableObject view)
        {
            return (IList)view.GetValue(RemoveItemsProperty);
        }

        public static void SetRemoveItems(BindableObject view, IList value)
        {
            view.SetValue(RemoveItemsProperty, value);
            var button = (Button)view;
            if (value is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += (s, e) => { IsEnabled(button, value); };
            }
            IsEnabled(button, value);
        }
        #endregion
        #endregion

        #region Protected Overrides
        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.IsEnabled = false;
        }

        protected override void OnButtonClicked(Button button)
        {
            base.OnButtonClicked(button);

            var collection = (IList)Button.GetValue(CollectionProperty);
            if (collection == null)
            {
                return;
            }

            var removeItems = (IList)Button.GetValue(RemoveItemsProperty);
            if (removeItems == null)
            {
                return;
            }

            //Get the binding context
            var bindingContext = button.BindingContext;

            if (removeItems != null)
            {
                for (var i = removeItems.Count - 1; i > -1; i--)
                {
                    collection.Remove(removeItems[i]);
                }
            }
            else if (bindingContext == null)
            {
                collection.Remove(bindingContext);
            }
        }

        #endregion

        #region Private Static Methods
        private static void IsEnabled(Button button, IList list)
        {
            button.IsEnabled = list.Count > 0;
        }
        #endregion
    }
}
