using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;

#if (SILVERLIGHT)
using TreeViewItem = System.Windows.Controls.TreeViewItem;
using DataTemplate = System.Windows.DataTemplate;
#else
using DataTemplate = Xamarin.Forms.DataTemplate;
using System.Reflection;
#endif

namespace Adapt.Presentation.Controls
{
    /// <summary>
    /// Provides TreeViewItems formatted in to DataTemplates for a given Object with a hierarchical strcuture
    /// </summary>
    public partial class AdaptTreeViewItemProvider : IEnumerable<TreeViewItem>, INotifyCollectionChanged
    {
        #region Delegates
        public delegate void TreeViewItemBuildingHandler(object dataContext, ref TreeViewItem proposedNode);
        #endregion

        #region Events
        public event EventHandler<PropertyChangedEventArgs> ItemPropertyChanged;
        public event TreeViewItemBuildingHandler TreeViewItemBuilding;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region Fields
        private readonly List<string> _ProbingPaths = new List<string>();
        private readonly List<TreeViewItem> _TreeViewItems = new List<TreeViewItem>();
        private readonly List<object> _FlattenedObjects = new List<object>();
        private readonly Dictionary<TreeViewItem, TreeViewItem> _ParentChildLink = new Dictionary<TreeViewItem, TreeViewItem>();
        #endregion

        #region Protected Fields
        protected readonly Dictionary<object, TreeViewItem> _TreeViewItemsByDataContext = new Dictionary<object, TreeViewItem>();
        #endregion

        #region Public Properties
        public string RootNodeText { get; set; }
        public bool IsAllNodesExpanded { get; set; }
        public string DefaultCollectionTemplateKey { get; set; }

        /// <summary>
        /// The root list of items that will be converted in to TreeItems
        /// </summary>
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public string ProbingPaths
        {
            get
            {
                return string.Join(",", _ProbingPaths.ToArray());
            }
            set
            {
                _ProbingPaths.Clear();
                var probingPaths = new List<string>(value.Split(','));
                foreach (var probingPath in probingPaths)
                {
                    _ProbingPaths.Add(probingPath);
                }
            }
        }

        #endregion

        #region Private Methods
        private TreeViewItem GetParentTreeViewItem(TreeViewItem treeViewItem)
        {
            return _ParentChildLink[treeViewItem];
        }

        private void Refresh()
        {
            InitializeRefresh();

            _TreeViewItems.Clear();
            _FlattenedObjects.Clear();
            _ParentChildLink.Clear();
            _TreeViewItemsByDataContext.Clear();

            if (ItemsTemplates == null || ItemsTemplates.Count <= 0 || ItemsSource == null)
            {
                return;
            }

            var children = ItemsSource;

            //Clear these index
            //Recursively iterate through root nodes
            _TreeViewItems.Clear();
            _TreeViewItems.Add(CreateTreeViewItem(children, RootNodeText, null));

            FinishRefresh();

            //Tell the treeview we've updated the collection
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private IEnumerable<TreeViewItem> GetItemsChildren(TreeViewItem theItem)
        {
            var retVal = new List<TreeViewItem>();

            foreach (var theLinkedChild in _ParentChildLink)
            {
                if (theLinkedChild.Value == theItem)
                {
                    retVal.Add(theLinkedChild.Key);
                }
            }

            return retVal;
        }

        public CollectionChildInfo GetParentListAndIndex(TreeViewItem selectedItem)
        {
            if (selectedItem == null)
            {
                return null;
            }

            var retVal = new CollectionChildInfo();
            var parentItem = GetParentTreeViewItem(selectedItem);

            if (GetDataContext(parentItem) is CollectionInformation)
            {
                retVal.Item = GetDataContext(selectedItem);
                retVal.TreeViewItem = GetTreeViewItemByDataContext(retVal.Item);

                retVal.IsPartOfCollection = true;
                object dataContext = GetDataContext(parentItem);
                var collection = ((CollectionInformation)dataContext).Collection;
                var list = collection as IList;
                if (list != null)
                {
                    retVal.IsOriginalCollectionList = true;
                    retVal.List = list;
                }
                else
                {
                    throw new NotImplementedException();
                }

                retVal.ItemIndex = retVal.List.IndexOf(retVal.Item);
            }

            else
            {
                retVal.ItemIndex = -1;
                retVal.IsOriginalCollectionList = false;
                retVal.IsPartOfCollection = false;
                retVal.List = null;
            }

            return retVal;
        }

        private TreeViewItem CreateTreeViewItem(object child, string propertyName, TreeViewItem parentTreeViewItem)
        {
            if (child == null)
            {
                return new TreeViewItem();
            }

            var enumerable = child as IEnumerable;
            var childAsString = child as string;

            var notifyPropertyChanged = child as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += AdaptTreeViewItemProvider_PropertyChanged;
            }

            //Create a flat list of objects
            _FlattenedObjects.Add(child);

            //Create treeviewitem for child
            var treeNode = new TreeViewItem();

            //should we have this check? Could stuff get left off like this?
            if (!_TreeViewItemsByDataContext.ContainsKey(child))
            {
                _TreeViewItemsByDataContext.Add(child, treeNode);
            }

            if (IsAllNodesExpanded)
            {
                treeNode.IsExpanded = true;
            }

            if (ItemTemplatesDictionary.ContainsKey(child.GetType().FullName))
            {
                if (enumerable != null && childAsString == null)
                {

                    treeNode.Header = LoadContent(ItemTemplatesDictionary[enumerable.GetType().FullName]);
                    SetDataContext(treeNode, new CollectionInformation(propertyName, enumerable));
                }
                else
                {
                    //Set the header based on the template
                    treeNode.Header = LoadContent(ItemTemplatesDictionary[child.GetType().FullName]);
                    SetDataContext(treeNode, child);
                }
            }
            else
            {
                if (enumerable != null && childAsString == null)
                {
                    if (string.IsNullOrEmpty(DefaultCollectionTemplateKey))
                    {
                        throw new Exception($"{nameof(DefaultCollectionTemplateKey)} must be specified.");
                    }

                    treeNode.Header = LoadContent(ItemTemplatesDictionary[DefaultCollectionTemplateKey]);
                    SetDataContext(treeNode, new CollectionInformation(propertyName, enumerable));
                }
                else
                {
                    CreateNoTemplateNode(child, treeNode);
                }
            }


            //Create a list for the child nodes
            var nodes = new ObservableCollection<TreeViewItem>();

            //Iterate through all the child collections that could be on the item
            foreach (var childPropertyName in _ProbingPaths)
            {
                //Get the child collection property
                var childProperty = child.GetType().GetProperty(childPropertyName);

                //Process the child collection
                var childValue = childProperty?.GetValue(child, null);

                if (childValue == null)
                {
                    continue;
                }

                var treeViewItem = CreateTreeViewItem(childValue, childProperty.Name, treeNode);

                //Add the children to the current treeviewitem
                nodes.Add(treeViewItem);
            }

            if (enumerable != null && childAsString == null)
            {
                var children = enumerable;

                //Iterate through list of children passed in 
                foreach (var theChild in children)
                {
                    var childNode = CreateTreeViewItem(theChild, "Something went wrong", treeNode);
                    nodes.Add(childNode);
                }
            }

            // Register the link between children and parent nodes
            foreach (var theChild in nodes)
            {
                _ParentChildLink.Add(theChild, treeNode);
            }

            //Set the itemssource on the nodes
            treeNode.ItemsSource = nodes;

            TreeViewItemBuilding?.Invoke(child, ref treeNode);

            return treeNode;
        }

        #endregion

        #region Public Methods
        public TreeViewItem GetTreeViewItemByDataContext(object dataContext)
        {
            return _TreeViewItemsByDataContext[dataContext];
        }
        #endregion

        #region Event Handlers
        private void AdaptTreeViewItemProvider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(sender, e);
        }
        #endregion

        #region Other

        public IEnumerator<TreeViewItem> GetEnumerator()
        {
            return _TreeViewItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _TreeViewItems.GetEnumerator();
        }

        #endregion

        #region Inner Classes
        public class CollectionChildInfo
        {
            public bool IsPartOfCollection { get; set; }
            public bool IsOriginalCollectionList { get; set; }
            public IList List { get; set; }
            public int ItemIndex { get; set; }
            public object Item { get; set; }
            public TreeViewItem TreeViewItem { get; set; }
        }
        #endregion
    }
}
