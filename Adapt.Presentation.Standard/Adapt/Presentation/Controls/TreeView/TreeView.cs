using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public class TreeView : ScrollView, IDisposable
    {
        #region Fields
        private readonly ObservableCollection<TreeViewNode> _ChildTreeViewNodes = new ObservableCollection<TreeViewNode>();
        private readonly StackLayout _StackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
        #endregion

        #region Public Properties
        public ObservableCollection<TreeViewNode> ChildTreeViewNodes => _ChildTreeViewNodes;
        #endregion

        #region Constructor
        public TreeView()
        {
            Content = _StackLayout;
            _ChildTreeViewNodes.CollectionChanged += ChildTreeViewNodes_CollectionChanged;
        }
        #endregion

        #region Event Handlers
        private void ChildTreeViewNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RenderNodes(_ChildTreeViewNodes, _StackLayout);
        }
        #endregion

        #region Internal Static Methods
        internal static void RenderNodes(ObservableCollection<TreeViewNode> childTreeViewNodes, StackLayout parent)
        {
            parent.Children.Clear();
            foreach (var childTreeNode in childTreeViewNodes)
            {
                parent.Children.Add(childTreeNode);
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            _ChildTreeViewNodes.CollectionChanged -= ChildTreeViewNodes_CollectionChanged;

            foreach (var TreeViewNode in _ChildTreeViewNodes)
            {
                TreeViewNode.Dispose();
            }

            _StackLayout.Children.Clear();
        }
        #endregion
    }
}