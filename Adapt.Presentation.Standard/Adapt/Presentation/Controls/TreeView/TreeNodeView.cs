using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public partial class TreeNodeView : StackLayout
    {
        #region Fields
        private readonly Grid _MainGrid;
        private readonly ContentView _ContentView;
        private readonly StackLayout _ChildrenStackLayout;
        private readonly ObservableCollection<TreeNodeView> _ChildTreeNodeViews = new ObservableCollection<TreeNodeView>();
        private TreeNodeView ParentTreeNodeView { get; set; }
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create("IsExpanded", typeof(bool), typeof(TreeNodeView), true, BindingMode.TwoWay, null,
            (bindable, oldValue, newValue) =>
            {
                var node = bindable as TreeNodeView;

                if (oldValue == newValue || node == null)
                    return;

                node.BatchBegin();
                try
                {
                    // show or hide all children
                    node._ChildrenStackLayout.IsVisible = node.IsExpanded;
                }
                finally
                {
                    // ensure we commit
                    node.BatchCommit();
                }
            }
            , null, null);

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        #endregion

        #region Public Properties

        public View Content
        {
            get { return _ContentView.Content; }
            set { _ContentView.Content = value; }
        }

        public ObservableCollection<TreeNodeView> ChildTreeNodeViews
        {
            get
            {
                return _ChildTreeNodeViews;
            }
        }
        #endregion

        #region Constructor
        public TreeNodeView()
        {
            _ChildTreeNodeViews.CollectionChanged += _ChildTreeNodeViews_CollectionChanged;

            IsExpanded = true;

            _MainGrid = new Grid
            {
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Gray,
                RowSpacing = 2
            };

            _MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _ContentView = new ContentView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = BackgroundColor
            };
            _MainGrid.Children.Add(_ContentView);

            _ChildrenStackLayout = new StackLayout
            {
                Orientation = Orientation,
                BackgroundColor = Color.Blue,
                Spacing = 0
            };
            _MainGrid.Children.Add(_ChildrenStackLayout, 0, 1);

            Children.Add(_MainGrid);

            Spacing = 0;
            Padding = new Thickness(0);
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.Start;
        }

        #endregion

        #region Event Handlers
        private void _ChildTreeNodeViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _ChildrenStackLayout.Children.Clear();
            foreach (var childTreeNode in _ChildTreeNodeViews)
            {
                _ChildrenStackLayout.Children.Add(childTreeNode);
            }
        }
        #endregion

        #region Protected Methods
        protected void DetachVisualChildren()
        {
            var views = _ChildrenStackLayout.Children.OfType<TreeNodeView>().ToList();

            foreach (TreeNodeView nodeView in views)
            {
                _ChildrenStackLayout.Children.Remove(nodeView);
                nodeView.ParentTreeNodeView = null;
            }
        }

        // [recursive down] create item template instances, attach and layout, and set descendents until finding overrides
        protected void BuildVisualChildren()
        {
            var bindingContextNode = BindingContext;
            if (bindingContextNode == null)
                return;

            // STEP 1: remove child visual tree nodes (TreeNodeViews) that don't correspond to an item in our data source

            var nodeViewsToRemove = new List<TreeNodeView>();

            BatchBegin();
            try
            {
                // perform removal in a batch
                foreach (TreeNodeView nodeView in nodeViewsToRemove)
                    _MainGrid.Children.Remove(nodeView);
            }
            finally
            {
                // ensure we commit
                BatchCommit();
            }

            // STEP 2: add visual tree nodes (TreeNodeViews) for children of the binding context not already associated with a TreeNodeView


            BatchBegin();
            try
            {
                // perform the additions in a batch
                foreach (var nodeView in ChildTreeNodeViews)
                {
                    _ChildrenStackLayout.Children.Add(nodeView);

                    _ChildrenStackLayout.SetBinding(IsVisibleProperty, new Binding("IsExpanded", BindingMode.TwoWay));

                    // TODO: make sure to unsubscribe elsewhere
                    nodeView.PropertyChanged += HandleListCountChanged;
                }
            }
            finally
            {
                // ensure we commit
                BatchCommit();
            }

        }

        #endregion

        #region Protected Overrides
        protected override void OnBindingContextChanged()
        {
            // prevent exceptions for null binding contexts
            // and during startup, this node will inherit its BindingContext from its Parent - ignore this
            if (BindingContext == null || (Parent != null && BindingContext == Parent.BindingContext))
                return;

            base.OnBindingContextChanged();

            // clear out any existing child nodes - the new data source replaces them
            // make sure we don't do this if BindingContext == null
            DetachVisualChildren();

            // build the new visual tree
            BuildVisualChildren();
        }
        #endregion

        #region Private Methods

        private void HandleListCountChanged(object sender, PropertyChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
                {
                    if (e.PropertyName == "Count")
                    {
                        var nodeView = ChildTreeNodeViews.Where(nv => nv.BindingContext == sender).FirstOrDefault();
                        if (nodeView != null)
                            nodeView.BuildVisualChildren();
                    }
                });
        }
        #endregion
    }
}