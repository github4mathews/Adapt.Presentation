using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public class TreeView : ScrollView, IDisposable
    {
        #region Fields
        private readonly ObservableCollection<TreeViewNode> _ChildTreeNodeViews = new ObservableCollection<TreeViewNode>();
        private readonly StackLayout _StackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
        #endregion

        #region Public Properties
        public ObservableCollection<TreeViewNode> ChildTreeNodeViews => _ChildTreeNodeViews;
        #endregion

        #region Constructor
        public TreeView()
        {
            Content = _StackLayout;
            _ChildTreeNodeViews.CollectionChanged += ChildTreeNodeViews_CollectionChanged;
        }
        #endregion

        #region Event Handlers
        private void ChildTreeNodeViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _StackLayout.Children.Clear();
            foreach (var treeNodeView in _ChildTreeNodeViews)
            {
                _StackLayout.Children.Add(treeNodeView);
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            _ChildTreeNodeViews.CollectionChanged -= ChildTreeNodeViews_CollectionChanged;

            foreach (var treeNodeView in _ChildTreeNodeViews)
            {
                treeNodeView.Dispose();
            }

            _StackLayout.Children.Clear();
        }
        #endregion
    }
}