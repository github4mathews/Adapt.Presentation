﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class TreeView : ScrollView, IDisposable
    {
        #region Fields
        private readonly ObservableCollection<TreeViewItem> _ItemsSource = new ObservableCollection<TreeViewItem>();
        private readonly StackLayout _StackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
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

        public ObservableCollection<TreeViewItem> ItemsSource => _ItemsSource;
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
            _ItemsSource.CollectionChanged += ChildTreeViewItems_CollectionChanged;
        }
        #endregion

        #region Event Handlers
        private void ChildTreeViewItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RenderNodes(_ItemsSource, _StackLayout);
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
            RemoveSelectionRecursive(_ItemsSource);
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

        /// <summary>
        /// Call this when you are finished with the control to make sure events are detached and memory resources are freed up
        /// </summary>
        public void Dispose()
        {
            SelectedItemChanged = null;
            _ItemsSource.CollectionChanged -= ChildTreeViewItems_CollectionChanged;

            foreach (var TreeViewItem in _ItemsSource)
            {
                TreeViewItem.Dispose();
            }

            _StackLayout.Children.Clear();
        }
        #endregion
    }
}