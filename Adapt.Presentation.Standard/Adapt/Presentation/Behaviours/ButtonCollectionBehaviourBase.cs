using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
    public class ButtonCollectionBehaviourBase : Behavior<Button>
    {
        #region Fields
        protected Button Button;
        #endregion

        #region BindableProperty

        #region Collection
        public static readonly BindableProperty CollectionProperty =
        BindableProperty.CreateAttached(
        "Collection",
        typeof(IList),
        typeof(Button),
        null,
        propertyChanged: OnCollectionChanged);

        private static void OnCollectionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetCollection(bindable, (IList)newValue);
        }

        public static IList GetCollection(BindableObject view)
        {
            return (IList)view.GetValue(CollectionProperty);
        }

        public static void SetCollection(BindableObject view, IList value)
        {
            var button = (Button)view;
            view.SetValue(CollectionProperty, value);
            if (value is INotifyCollectionChanged notifyCollectionChanged)
            {
                RaiseCollectionChanged(button);
            }

            RaiseCollectionChanged(button);
        }
        #endregion

        #endregion

        #region Protected Overrides
        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);
            Button = bindable;
            bindable.Clicked += Button_Clicked;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Clicked -= Button_Clicked;
        }
        #endregion

        #region Virtual Methods

        protected virtual void OnCollectionChanged(Button button) { }

        protected virtual void OnButtonClicked(Button button)
        {
        }
        #endregion

        #region Event Handlers
        private void Button_Clicked(object sender, EventArgs e)
        {
            OnButtonClicked(sender as Button);
        }
        #endregion

        #region Private Static Methods
        private static void RaiseCollectionChanged(Button button)
        {
            //TODO: Why is this necessary in XF? Is there a better way?
            var behaviour = (ButtonCollectionBehaviourBase)button.Behaviors.Where(b => (b is ButtonCollectionBehaviourBase)).FirstOrDefault();
            if (behaviour == null)
            {
                //This shouldn't be possible...
                return;
            }

            behaviour.OnCollectionChanged(button);
        }
        #endregion
    }
}
