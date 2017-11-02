using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public partial class TreeViewNode : StackLayout, IDisposable
    {
        #region Fields
        private readonly BoxView _SpacerBoxView = new BoxView();

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
            Orientation = StackOrientation.Vertical,
            Spacing = 0
        };

        private readonly ObservableCollection<TreeViewNode> _Children = new ObservableCollection<TreeViewNode>();
        private readonly TapGestureRecognizer _TapGestureRecognizer = new TapGestureRecognizer();
        #endregion

        #region Internal Fields
        internal readonly BoxView SelecionBoxView = new BoxView { Color = Color.Blue, Opacity = .5, IsVisible = false };
        #endregion

        #region Private Properties
        private TreeViewNode ParentTreeViewNode => Parent?.Parent?.Parent as TreeViewNode;
        private TreeView ParentTreeView => Parent?.Parent as TreeView;
        private double IndentWidth => Depth * SpacerWidth;
        private int SpacerWidth { get; set; } = 30;
        private int Depth => (ParentTreeViewNode == null ? 0 : ParentTreeViewNode.Depth + 1);
        #endregion

        #region Protected Overrides
        protected override void OnParentSet()
        {
            Render();
            base.OnParentSet();
        }
        #endregion

        #region Public Properties

        public bool IsSelected
        {
            get
            {
                return SelecionBoxView.IsVisible;
            }
            set
            {
                SelecionBoxView.IsVisible = value;
            }
        }
        public bool IsExpanded
        {
            get
            {
                return _ChildrenStackLayout.IsVisible;
            }
            set
            {
                _ChildrenStackLayout.IsVisible = value;
            }
        }

        public View Content
        {
            get { return _ContentView.Content; }
            set { _ContentView.Content = value; }
        }

        public ObservableCollection<TreeViewNode> Children
        {
            get
            {
                return _Children;
            }
        }
        #endregion

        #region Constructor
        public TreeViewNode()
        {
            _Children.CollectionChanged += ChildTreeViewNodes_CollectionChanged;

            IsExpanded = true;

            _TapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            //_TapGestureRecognizer.NumberOfTapsRequired = 2;
            GestureRecognizers.Add(_TapGestureRecognizer);

            _MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });


            _MainGrid.Children.Add(SelecionBoxView);

            _ContentStackLayout.Children.Add(_SpacerBoxView);
            _ContentStackLayout.Children.Add(_ContentView);

            _MainGrid.Children.Add(_ContentStackLayout);
            _MainGrid.Children.Add(_ChildrenStackLayout, 0, 1);

            base.Children.Add(_MainGrid);

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.Start;

            Render();
        }

        private void Render()
        {
            _SpacerBoxView.WidthRequest = IndentWidth;
        }

        #endregion

        #region Public Methods
        public void Dispose()
        {
            foreach (var childTreeViewNode in _Children)
            {
                childTreeViewNode.Dispose();
            }

            base.Children.Clear();

            _Children.CollectionChanged -= ChildTreeViewNodes_CollectionChanged;
            _TapGestureRecognizer.Tapped -= TapGestureRecognizer_Tapped;
            GestureRecognizers.Remove(_TapGestureRecognizer);
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// TODO: This is a little stinky...
        /// </summary>
        internal void ChildSelected(TreeViewNode child)
        {
            ParentTreeViewNode?.ChildSelected(child);
            ParentTreeView?.ChildSelected(child);
        }
        #endregion

        #region Event Handlers
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            ChildSelected(this);
            IsExpanded = !IsExpanded;
        }

        private void ChildTreeViewNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TreeView.RenderNodes(_Children, _ChildrenStackLayout);
        }

        #endregion
    }
}