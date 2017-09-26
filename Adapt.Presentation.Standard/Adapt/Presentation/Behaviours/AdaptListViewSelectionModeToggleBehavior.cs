using Adapt.Presentation.Controls;
using System;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
    public class AdaptListViewSelectionModeToggleBehavior : Behavior<Button>
    {
        #region Fields
        private Button _Button;
        #endregion

        #region SelectionMode
        public static readonly BindableProperty SelectionModeProperty =
        BindableProperty.CreateAttached(
        nameof(AdaptListView.SelectionMode),
        typeof(AdaptListView.ItemSelectorSelectionMode?),
        typeof(Button),
        null,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnSelectionModeChanged);

        private static void OnSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetSelectionMode(bindable, (AdaptListView.ItemSelectorSelectionMode)newValue);

            var button = (Button)bindable;

            //TODO: This will need to be something more sophisticated in future
            button.Text = newValue.ToString();
        }

        public static AdaptListView.ItemSelectorSelectionMode GetSelectionMode(BindableObject view)
        {
            return (AdaptListView.ItemSelectorSelectionMode)view.GetValue(SelectionModeProperty);
        }

        public static void SetSelectionMode(BindableObject view, AdaptListView.ItemSelectorSelectionMode value)
        {
            view.SetValue(SelectionModeProperty, value);
        }
        #endregion

        #region Protected Overrides
        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);
            _Button = bindable;
            bindable.Clicked += SelectionModeToggleButton_Clicked;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Clicked -= SelectionModeToggleButton_Clicked;
        }
        #endregion

        #region Private Methods
        private void Toggle()
        {
            var selectionMode = (AdaptListView.ItemSelectorSelectionMode)_Button.GetValue(SelectionModeProperty);
            var newValue = selectionMode == AdaptListView.ItemSelectorSelectionMode.Multi ? AdaptListView.ItemSelectorSelectionMode.Single : AdaptListView.ItemSelectorSelectionMode.Multi;
            SetSelectionMode(_Button, newValue);
        }
        #endregion

        #region Event Handlers
        private void SelectionModeToggleButton_Clicked(object sender, EventArgs e)
        {
            Toggle();
        }
        #endregion
    }
}
