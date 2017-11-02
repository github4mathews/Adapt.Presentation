using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Adapt.Presentation.Controls.TreeView;


namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeViewPage : ContentPage
    {
        protected override void OnAppearing()
        {
            var node = new TreeNodeView();
            node.Content = new Label { Text = "Content"};

            var node2 = new TreeNodeView();
            node2.Content = new Label { Text = "Content 2" };

            var node3 = new TreeNodeView();
            node3.Content = new Label { Text = "Content 3" };
            node3.BackgroundColor = Color.Red;

            node.ChildTreeNodeViews.Add(node2);
            node2.ChildTreeNodeViews.Add(node3);

            TheTreeView.ChildTreeNodeViews.Add(node);
            base.OnAppearing();
        }

        public TreeViewPage()
        {
            InitializeComponent();
        }
    }
}