using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class TreeView : ScrollView, IDisposable
    {
        #region Fields
        private readonly StackLayout _StackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
        private IEnumerable<TreeViewItem> _ItemsSource;
        private TreeViewItem _SelectedItem;
        #endregion

        #region Public Properties

        /// <summary>
        /// The item that is selected in the tree
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

        /// <summary>
        /// The colour of the selected item
        /// TODO: Make this a bindable property for styling purposes
        /// </summary>
        public Color SelectedBackgroundColour { get; } = Color.Blue;


        /// <summary>
        /// The opacity of the box that sits over the top of the selected item
        /// TODO: Make this a bindable property for styling purposes
        /// </summary>
        public double SelectedBackgroundOpacity { get; } = .5;

        public IEnumerable<TreeViewItem> ItemsSource
        {
            get
            {
                return _ItemsSource;
            }
            set
            {
                _ItemsSource = value;

                if (value is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged += (s, e) =>
                    {
                        RenderNodes(_ItemsSource, _StackLayout, e, null);
                    };
                }

                RenderNodes(_ItemsSource, _StackLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), null);
            }
        }


        #endregion

        #region Events
        /// <summary>
        /// Occurs when the user selects a TreeViewItem
        /// </summary>
        public event EventHandler SelectedItemChanged;
        #endregion

        #region Constructor
        public TreeView()
        {
            Content = _StackLayout;
        }
        #endregion

        #region Event Handlers
        private void ChildTreeViewItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RenderNodes(ItemsSource, _StackLayout, e, null);
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

                RemoveSelectionRecursive(treeViewItem.ItemsSource);
            }
        }
        #endregion

        #region Private Static Methods
        private static void AddItems(IEnumerable<TreeViewItem> childTreeViewItems, StackLayout parent, TreeViewItem parentTreeViewItem)
        {
            foreach (var childTreeNode in childTreeViewItems)
            {
                parent.Children.Add(childTreeNode);
                childTreeNode.ParentTreeViewItem = parentTreeViewItem;
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
            RemoveSelectionRecursive(ItemsSource);
        }
        #endregion

        #region Internal Static Methods
        internal static void RenderNodes(IEnumerable<TreeViewItem> childTreeViewItems, StackLayout parent, NotifyCollectionChangedEventArgs e, TreeViewItem parentTreeViewItem)
        {
            System.Diagnostics.Debug.WriteLine($"Render Nodes {e.Action}");

            //TODO: This shouldn't clear and re-add. It should only do that on a reset. This is a performance problem but leaving it as is until someone reports it as a problem
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                parent.Children.Clear();
                AddItems(childTreeViewItems, parent, parentTreeViewItem);
            }
            else
            {
                AddItems(e.NewItems.Cast<TreeViewItem>(), parent, parentTreeViewItem);
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Call this when you are finished with the control to make sure events are detached and memory resources are freed up
        /// </summary>
        public void Dispose()
        {
            SelectedItemChanged = null;

            foreach (TreeViewItem TreeViewItem in ItemsSource)
            {
                TreeViewItem.Dispose();
            }

            _StackLayout.Children.Clear();
        }
        #endregion
    }
}