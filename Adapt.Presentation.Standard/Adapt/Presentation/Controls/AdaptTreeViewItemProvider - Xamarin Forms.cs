using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public partial class AdaptTreeViewItemProvider : BindableObject
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

        public static readonly BindableProperty ItemsTemplatesProperty = BindableProperty.Create(nameof(ItemsTemplates), typeof(IEnumerable), typeof(AdaptTreeViewItemProvider), null, propertyChanged: OnItemsTemplatesChanged);

        private static void OnItemsTemplatesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AdaptTreeViewItemProvider)bindable;
            if (control.ItemsTemplates is INotifyCollectionChanged ItemsTemplates)
            {
                ItemsTemplates.CollectionChanged += (s, e) =>
                {
                    control.Refresh();
                };
            }
            control.Refresh();
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
        private static void SetItemsSourceNodes(TreeViewItem treeNode, List<TreeViewItem> nodes)
        {
            treeNode.ItemsSource.Clear();
            foreach (var node in nodes)
            {
                treeNode.ItemsSource.Add(node);
            }
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
