using System;
using System.Collections;
using System.Collections.ObjectModel;
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

        #region SelectedBackgroundColorProperty
        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(AdaptListView), Color.Gray, BindingMode.OneWayToSource);
        #endregion

        #region SelectedItemProperty
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AdaptListView), null, BindingMode.OneWayToSource, propertyChanged: OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptListView)bindable;
            control.RefreshSelection();
            control.SelectionChanged?.Invoke(control, new EventArgs());
        }
        #endregion

        #region SelectedItemsProperty		
        public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(nameof(SelectedItems), typeof(IList), typeof(AdaptListView), null, BindingMode.OneWayToSource, propertyChanged: OnSelectedItemsChanged);

        private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptListView)bindable;
            control.RefreshSelection();

            if (newValue is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += control.NotifyCollectionChanged_CollectionChanged;
            }

            control.SelectionChanged?.Invoke(control, new EventArgs());
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
                itemsSource.CollectionChanged += (s, e) =>
                {
                    control.RefreshItems();

                    //This is here to keep the async behaviour of the control while allowing for items to be removed.
                    //We don't process this in RefreshSelection because then, the SelectedItem would not get set if the ItemsSource doesn't yet have the SelectedItem
                    //But if the SelectedItem is removed from the ItemsSource later, we deselect the item
                    if (e.Action == NotifyCollectionChangedAction.Remove && control.SelectionMode == ItemSelectorSelectionMode.Single && e.OldItems != null && e.OldItems.Contains(control.SelectedItem))
                    {
                        control.SelectedItem = null;
                    }

                    //TODO: Handle multi select mode here, and also handle Resets

                };
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
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set
            {
                //TODO: We need to detach the previous collection's event here
                SetValue(SelectedItemsProperty, value);
            }
        }

        public Color SelectedBackgroundColor
        {
            get => (Color)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        public DataTemplate ItemTemplate { get; set; }

        #endregion Public Properties

        #region Event Handlers
        private void NotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshSelection();
            SelectionChanged?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Private Methods
        private void RefreshSelection()
        {
            //TODO: Support for duplicate records
            var dataItems = _StackList.Children.ToDictionary(x => x, y => y.BindingContext);

            foreach (var item in dataItems)
            {
                var view = item.Key;

                switch (SelectionMode)
                {
                    case ItemSelectorSelectionMode.Single:

                        if (GetIsEqual(item.Value, SelectedItem))
                        {
                            SetToBackgroundColor(view);
                        }
                        else
                        {
                            SetToTransparent(view);
                        }

                        break;

                    case ItemSelectorSelectionMode.Multi:

                        SetToTransparent(view);

                        if (SelectedItems != null)
                        {
                            foreach (var selectedItem in SelectedItems)
                            {
                                if (GetIsEqual(item.Value, selectedItem))
                                {
                                    SetToBackgroundColor(view);
                                }
                            }
                        }

                        break;
                }
            }
        }

        private static bool GetIsEqual(object item, object selectedItem)
        {
            return item != null && item.Equals(selectedItem);
        }

        private void SetToBackgroundColor(View view)
        {
            view.BackgroundColor = SelectedBackgroundColor;
        }

        private static void SetToTransparent(View view)
        {
            view.BackgroundColor = Color.Transparent;
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
            if (obj == null)
            {
                return;
            }

            var bindingContext = obj.BindingContext;

            switch (SelectionMode)
            {
                case ItemSelectorSelectionMode.Single:
                    SelectedItem = bindingContext;
                    break;
                case ItemSelectorSelectionMode.Multi:
                    if (SelectedItems != null)
                    {
                        if (!SelectedItems.Contains(bindingContext))
                        {
                            SelectedItems.Add(bindingContext);
                        }
                        else
                        {
                            SelectedItems.Remove(bindingContext);
                        }
                    }
                    else
                    {
                        SelectedItems = new ObservableCollection<object> { bindingContext };
                    }
                    break;
            }
        }

        #endregion Private Methods
    }
}

