using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class AdaptListView : ScrollView
    {
        #region Private Fields

        private readonly StackLayout _StackList = new StackLayout { Spacing = 0 };

        #endregion Private Fields

        #region Constructor

        public AdaptListView()
        {
            Content = _StackList;
        }

        #endregion Constructor

        #region Dependency Properties

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AdaptListView), null, BindingMode.OneWayToSource, propertyChanged: OnSelectedItemChanged);
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AdaptListView), null, propertyChanged: OnItemsSourceChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptListView)bindable;
            control.RefreshSelection();
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptListView)bindable;
            if (newValue is INotifyCollectionChanged itemsSource)
            {
                itemsSource.CollectionChanged += (s, e) => control.RefreshItems();
            }
            control.RefreshItems();
        }

        #endregion Dependency Properties

        #region Public Properties

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public DataTemplate ItemTemplate { get; set; }

        public Color SelectedBackgroundColor { get; set; } = Color.Gray;

        #endregion Public Properties

        #region Private Methods

        private void RefreshSelection()
        {
            //TODO: Support for duplicate records
            var dataItems = _StackList.Children.ToDictionary(x => x, y => y.BindingContext);

            foreach (var item in dataItems)
            {
                var view = item.Key;
                if (item.Value != null && item.Value.Equals(SelectedItem))
                {
                    view.BackgroundColor = SelectedBackgroundColor;
                }
                else
                {
                    view.BackgroundColor = Color.Transparent;
                }
            }
        }

        private void RefreshItems()
        {
            if (ItemsSource == null)
            {
                return;
            }

            _StackList.Children.Clear();
            foreach (var item in ItemsSource)
            {
                var view = (View)ItemTemplate.CreateContent();
                view.BindingContext = item;
#pragma warning disable CS0618 // Type or member is obsolete
                view.GestureRecognizers.Add(new TapGestureRecognizer(TemplateTapped));
#pragma warning restore CS0618 // Type or member is obsolete
                _StackList.Children.Add(view);
            }

            RefreshSelection();
        }

        private void TemplateTapped(View obj)
        {
            SelectedItem = obj.BindingContext;
        }

        #endregion Private Methods
    }
}