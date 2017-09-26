using System;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    /// <summary>
    /// A button for toggling the selection mode of an AdaptListView.
    /// </summary>
    /// <remarks>TODO: Make this a behaviour instead</remarks>
    class SelectionModeToggleButton : Button
    {
        public static BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(AdaptListView.ItemSelectorSelectionMode?), typeof(SelectionModeToggleButton), null, BindingMode.TwoWay, propertyChanged: ItemSelectorSelectionModeChanged);

        public AdaptListView.ItemSelectorSelectionMode SelectionMode
        {
            get => ((AdaptListView.ItemSelectorSelectionMode?)GetValue(SelectionModeProperty)).GetValueOrDefault();
            set => SetValue(SelectionModeProperty, value);
        }

        private static void ItemSelectorSelectionModeChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var button = (SelectionModeToggleButton)bindable;
            button.Text = newvalue.ToString();
        }

        public SelectionModeToggleButton()
        {
            Clicked += SelectionModeToggleButton_Clicked;
        }

        private void SelectionModeToggleButton_Clicked(object sender, EventArgs e)
        {
            SelectionMode = SelectionMode == AdaptListView.ItemSelectorSelectionMode.Multi ? AdaptListView.ItemSelectorSelectionMode.Single : AdaptListView.ItemSelectorSelectionMode.Multi;
        }
    }
}
