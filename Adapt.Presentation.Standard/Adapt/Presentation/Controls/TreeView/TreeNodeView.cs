using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public partial class TreeNodeView : StackLayout
    {
        #region Fields
        private Grid MainLayoutGrid;
        private ContentView HeaderView;
        private StackLayout ChildrenStackLayout;
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
                    node.ChildrenStackLayout.IsVisible = node.IsExpanded;
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

        public DataTemplate ContentTemplate { get; set; }
        public DataTemplate HeaderTemplate { get; set; }

        public View HeaderContent
        {
            get { return HeaderView.Content; }
            set { HeaderView.Content = value; }
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
        public TreeNodeView(DataTemplate headerTemplate, DataTemplate contentTemplate)
        {
            HeaderTemplate = headerTemplate;
            ContentTemplate = contentTemplate;
            IsExpanded = true;

            Render();
        }

        #endregion

        #region Protected Methods
        protected void DetachVisualChildren()
        {
            var views = ChildrenStackLayout.Children.OfType<TreeNodeView>().ToList();

            foreach (TreeNodeView nodeView in views)
            {
                ChildrenStackLayout.Children.Remove(nodeView);
                nodeView.ParentTreeNodeView = null;
            }
        }

        protected void BuildHeader()
        {
            HeaderContent = (View)HeaderTemplate.CreateContent();
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
                    MainLayoutGrid.Children.Remove(nodeView);
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
                    ChildrenStackLayout.Children.Add(nodeView);

                    ChildrenStackLayout.SetBinding(IsVisibleProperty, new Binding("IsExpanded", BindingMode.TwoWay));

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

        private void Render()
        {
            MainLayoutGrid = new Grid
            {
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Gray,
                RowSpacing = 2
            };

            MainLayoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            MainLayoutGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            MainLayoutGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            HeaderView = new ContentView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = this.BackgroundColor
            };
            MainLayoutGrid.Children.Add(HeaderView);

            ChildrenStackLayout = new StackLayout
            {
                Orientation = this.Orientation,
                BackgroundColor = Color.Blue,
                Spacing = 0
            };
            MainLayoutGrid.Children.Add(ChildrenStackLayout, 0, 1);

            ChildrenStackLayout.Children.Add((View)ContentTemplate.CreateContent());

            Children.Add(MainLayoutGrid);

            Spacing = 0;
            Padding = new Thickness(0);
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.Start;
        }

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