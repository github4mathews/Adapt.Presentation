using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls.TreeView
{
    public class TreeView : ScrollView
    {
        private readonly ObservableCollection<TreeNodeView> _ChildTreeNodeViews = new ObservableCollection<TreeNodeView>();

        public ObservableCollection<TreeNodeView> ChildTreeNodeViews
        {
            get
            {
                return _ChildTreeNodeViews;
            }
        }

        public TreeView()
        {
            _ChildTreeNodeViews.CollectionChanged += _ChildTreeNodeViews_CollectionChanged;
        }

        private void _ChildTreeNodeViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Content = null;

            var stackLayout = new StackLayout { Orientation = StackOrientation.Vertical };

            Content = stackLayout;

            foreach (var asdas in _ChildTreeNodeViews)
            {
                stackLayout.Children.Add(asdas);
            }
        }
    }
}