using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class AdaptListView : ScrollView
    {
        #region Enums
        public enum ItemSelectorSelectionMode
        {
            Single,
            Multi
        }
        #endregion

        #region Events
        public event EventHandler SelectionChanged;
        #endregion

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

        #region SelectedItemProperty
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AdaptListView), null, BindingMode.OneWayToSource, propertyChanged: OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptListView)bindable;
            control.RefreshSelection();
        }
        #endregion

        #region SelectedItemsProperty		
        public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(nameof(SelectedItems), typeof(IList), typeof(AdaptListView), null, BindingMode.OneWayToSource, propertyChanged: OnSelectedItemsChanged);

        private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptListView)bindable;
            control.RefreshSelection();
        }
        #endregion

        #region ItemsSourceProperty
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AdaptListView), null, propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            //TODO: We need to detach the old collection changed handler here...
            var control = (AdaptListView)bindable;
            if (newValue is INotifyCollectionChanged itemsSource)
            {
                itemsSource.CollectionChanged += (s, e) => control.RefreshItems();
            }
            control.RefreshItems();
        }
        #endregion

        #endregion Dependency Properties

        #region Public Properties

        public ItemSelectorSelectionMode SelectionMode { get; set; }

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

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set
            {
                //TODO: We need to detach the previous collection's event here
                //INotifyCollectionChanged notifyCollectionChanged = null;

                //if(notifyCollectionChanged!=null)
                //{
                //	notifyCollectionChanged.CollectionChanged -= NotifyCollectionChanged_CollectionChanged;
                //}

                SetValue(SelectedItemsProperty, value);
                SelectionChanged?.Invoke(this, new EventArgs());

                if (value is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged += NotifyCollectionChanged_CollectionChanged;
                }
            }
        }

        public DataTemplate ItemTemplate { get; set; }

        public Color SelectedBackgroundColor { get; set; } = Color.Gray;

        #endregion Public Properties

        #region Event Handlers
        private void NotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Private Methods

        public void RefreshSelection()
        {
            //TODO: Support for duplicate records
            var dataItems = _StackList.Children.ToDictionary(x => x, y => y.BindingContext);

            foreach (var item in dataItems)
            {
                var view = item.Key;

                if (SelectedItems != null)
                {
                    view.BackgroundColor = Color.Transparent;

                    foreach (var selectedItem in SelectedItems)
                    {
                        if (item.Value != null && item.Value.Equals(selectedItem))
                        {
                            view.BackgroundColor = SelectedBackgroundColor;
                        }
                    }
                }
                else
                {
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
            if (SelectedItems != null)
            {
                SelectedItems.Add(obj.BindingContext);
                RefreshSelection();
            }
            else
            {
                SelectedItem = obj.BindingContext;
            }
        }

        #endregion Private Methods
    }
}