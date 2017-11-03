using System.Collections;
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
            if (newValue is INotifyCollectionChanged ItemsTemplates)
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
    }
}
