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
        }
    }
}