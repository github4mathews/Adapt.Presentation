using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class TreeView : ScrollView, IDisposable
    {
        #region Fields
        private readonly ObservableCollection<TreeViewItem> _ChildTreeViewItems = new ObservableCollection<TreeViewItem>();
        private readonly StackLayout _StackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
        private TreeViewItem _SelectedItem;
        #endregion

        #region Public Properties

        /// <summary>
        /// TODO: Make this two way - and maybe eventually a bindable property
        /// </summary>
        public TreeViewItem SelectedItem
        {
            get
            {
                return _SelectedItem;
            }

            private set
            {
                _SelectedItem = value;
                SelectedItemChanged?.Invoke(this, new EventArgs());
            }
        }

        public Color SelectedBackgroundColour { get; } = Color.Blue;
        public double SelectedBackgroundOpacity { get; } = .5;
        public ObservableCollection<TreeViewItem> ChildTreeViewItems => _ChildTreeViewItems;
        #endregion

        #region Events
        public EventHandler SelectedItemChanged;
        #endregion

        #region Constructor
        public TreeView()
        {
            Content = _StackLayout;
            _ChildTreeViewItems.CollectionChanged += ChildTreeViewItems_CollectionChanged;
        }
        #endregion

        #region Event Handlers
        private void ChildTreeViewItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RenderNodes(_ChildTreeViewItems, _StackLayout);
        }
        #endregion

        #region Private Methods
        private void RemoveSelectionRecursive(IEnumerable<TreeViewItem> nodes)
        {
            foreach (var treeViewItem in nodes)
            {
                if (treeViewItem != SelectedItem)
                {
                    treeViewItem.IsSelected = false;
                }

                RemoveSelectionRecursive(treeViewItem.ChildTreeViewItems);
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// TODO: A bit stinky but better than bubbling an event up...
        /// </summary>
        internal void ChildSelected(TreeViewItem child)
        {
            SelectedItem = child;
            child.IsSelected = true;
            child.SelectionBoxView.Color = SelectedBackgroundColour;
            child.SelectionBoxView.Opacity = SelectedBackgroundOpacity;
            RemoveSelectionRecursive(_ChildTreeViewItems);
        }
        #endregion

        #region Internal Static Methods
        internal static void RenderNodes(ObservableCollection<TreeViewItem> childTreeViewItems, StackLayout parent)
        {
            //TODO: This shouldn't clear and re-add. It should only do that on a reset. This is a performance problem but leaving it as is until someone reports it as a problem

            parent.Children.Clear();
            foreach (var childTreeNode in childTreeViewItems)
            {
                parent.Children.Add(childTreeNode);
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            _ChildTreeViewItems.CollectionChanged -= ChildTreeViewItems_CollectionChanged;

            foreach (var TreeViewItem in _ChildTreeViewItems)
            {
                TreeViewItem.Dispose();
            }

            _StackLayout.Children.Clear();
        }
        #endregion
    }
}