using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public partial class AdaptTreeViewItemProvider : View
    {
        #region BindableProperties
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AdaptTreeViewItemProvider), null, propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptTreeViewItemProvider)bindable;
            if (newValue is INotifyCollectionChanged itemsSource)
            {
                itemsSource.CollectionChanged += (s, e) =>
                {
                    control.Refresh();
                };
            }
            control.Refresh();
        }

        #endregion

        #region Fields
        private ObservableCollection<ItemTemplateInfo> _ItemsTemplates = new ObservableCollection<ItemTemplateInfo>();
        #endregion

        #region Private Propertiers
        private Dictionary<string, DataTemplate> ItemTemplatesDictionary
        {
            get
            {
                var retVal = new Dictionary<string, DataTemplate>();
                foreach (var keyValuePair in _ItemsTemplates)
                {
                    retVal.Add(keyValuePair.TypeName, keyValuePair.ItemTemplate);
                }

                return retVal;
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The list of templates indexed by the property name on the objects that will be recursed through
        /// </summary>
        public ObservableCollection<ItemTemplateInfo> ItemsTemplates
        {
            get
            {
                return _ItemsTemplates;
            }
        }
        #endregion

        #region Private Methods
        private void InitializeRefresh()
        {
        }

        private void FinishRefresh()
        {
        }
        #endregion

        #region Private Static Methods
        private static void RegisterParents(TreeViewItem treeNode, ObservableCollection<TreeViewItem> nodes)
        {
            //Silverlight only
        }

        private static TreeViewItem GetParentTreeViewItem(TreeViewItem selectedItem)
        {
            return selectedItem.ParentTreeViewItem;
        }

        private static void CreateNoTemplateNode(object child, TreeViewItem treeNode)
        {
            //Note: Although there might not be a template for this type, the node's header might get created manually in the event called later.
            treeNode.BindingContext = child;
            var textBlock = new Label();
            textBlock.Text = $"No Template ({child.GetType().FullName})";
            treeNode.Header = textBlock;
        }

        private static object GetDataContext(TreeViewItem parentItem)
        {
            return parentItem?.BindingContext;
        }

        private static void SetDataContext(TreeViewItem parentItem, object bindingContext)
        {
            if (parentItem != null)
            {
                parentItem.BindingContext = bindingContext;
            }
        }

        private View LoadContent(DataTemplate dataTemplate)
        {
            return (View)dataTemplate.CreateContent();
        }
        #endregion
    }
}
