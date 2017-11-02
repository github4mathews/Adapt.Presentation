using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public partial class TreeViewNode : StackLayout, IDisposable
    {
        #region Fields
        private readonly BoxView _Spacer = new BoxView();

        private readonly Grid _MainGrid = new Grid
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            RowSpacing = 2
        };

        private readonly StackLayout _ContentStackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };

        private readonly ContentView _ContentView = new ContentView
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
        };

        private readonly StackLayout _ChildrenStackLayout = new StackLayout
        {
            Orientation =  StackOrientation.Vertical,
            Spacing = 0
        };

        private readonly ObservableCollection<TreeViewNode> _ChildTreeNodeViews = new ObservableCollection<TreeViewNode>();
        private readonly TapGestureRecognizer _TapGestureRecognizer = new TapGestureRecognizer();
        #endregion

        #region Private Properties
        private TreeViewNode ParentTreeNodeView => Parent?.Parent?.Parent as TreeViewNode;
        private double IndentWidth => Depth * SpacerWidth;
        private int SpacerWidth { get; set; } = 30;
        private int Depth
        {
            get { return (ParentTreeNodeView == null ? 0 : ParentTreeNodeView.Depth + 1); }
        }
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create("IsExpanded", typeof(bool), typeof(TreeViewNode), true, BindingMode.TwoWay, null, (bindable, oldValue, newValue) =>
        {
            var treeNodeView = bindable as TreeViewNode;

            if (oldValue == newValue || treeNodeView == null)
            {
                return;
            }

            treeNodeView.BatchBegin();
            try
            {
                treeNodeView._ChildrenStackLayout.IsVisible = treeNodeView.IsExpanded;
            }
            finally
            {
                treeNodeView.BatchCommit();
            }
        }, null, null);

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        #endregion

        #region Protected Overrides
        protected override void OnParentSet()
        {
            Render();
            base.OnParentSet();
        }
        #endregion

        #region Public Properties
        public View Content
        {
            get { return _ContentView.Content; }
            set { _ContentView.Content = value; }
        }

        public ObservableCollection<TreeViewNode> ChildTreeNodeViews
        {
            get
            {
                return _ChildTreeNodeViews;
            }
        }
        #endregion

        #region Constructor
        public TreeViewNode()
        {
            _ChildTreeNodeViews.CollectionChanged += ChildTreeNodeViews_CollectionChanged;

            IsExpanded = true;

            _TapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            GestureRecognizers.Add(_TapGestureRecognizer);

            _MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _ContentStackLayout.Children.Add(_Spacer);
            _ContentStackLayout.Children.Add(_ContentView);

            _MainGrid.Children.Add(_ContentStackLayout);
            _MainGrid.Children.Add(_ChildrenStackLayout, 0, 1);

            Children.Add(_MainGrid);

            Spacing = 0;
            Padding = new Thickness();
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.Start;

            Render();
        }

        private void Render()
        {
            _Spacer.WidthRequest = IndentWidth;
        }

        #endregion

        #region Public Methods
        public void Dispose()
        {
            foreach (var childTreeNodeViews in _ChildTreeNodeViews)
            {
                childTreeNodeViews.Dispose();
            }

            Children.Clear();

            _ChildTreeNodeViews.CollectionChanged -= ChildTreeNodeViews_CollectionChanged;
            _TapGestureRecognizer.Tapped -= TapGestureRecognizer_Tapped;
            GestureRecognizers.Remove(_TapGestureRecognizer);
        }
        #endregion

        #region Event Handlers
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            IsExpanded = !IsExpanded;
        }

        private void ChildTreeNodeViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _ChildrenStackLayout.Children.Clear();
            foreach (var childTreeNode in _ChildTreeNodeViews)
            {
                _ChildrenStackLayout.Children.Add(childTreeNode);
            }
        }
        #endregion
    }
}