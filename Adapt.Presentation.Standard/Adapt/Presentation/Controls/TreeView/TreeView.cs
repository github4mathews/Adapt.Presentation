using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        /// <summary>
        /// TODO: Make this two way - and maybe eventually a bindable property
        /// </summary>
        public TreeViewNode SelectedItem { get; private set; }

        public Color SelectedBackgroundColour { get; } = Color.Blue;
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
        private void ChildTreeViewNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RenderNodes(_ChildTreeViewNodes, _StackLayout);
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// TODO: A bit stinky but better than bubbling an event up...
        /// </summary>
        internal void ChildSelected(TreeViewNode child)
        {
            SelectedItem = child;
            child.OldBackgroundColour = child.BackgroundColor;
            child.BackgroundColor = SelectedBackgroundColour;
        }
        #endregion

        #region Internal Static Methods
        internal static void RenderNodes(ObservableCollection<TreeViewNode> childTreeViewNodes, StackLayout parent)
        {
            //TODO: This shouldn't clear and re-add. It should only do that on a reset. This is a performance problem but leaving it as is until someone reports it as a problem

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