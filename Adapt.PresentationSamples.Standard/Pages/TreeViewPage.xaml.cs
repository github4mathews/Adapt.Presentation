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
            node.Content = new Label { Text = "Content" };
            node.HeaderContent = new Label { Text = "Header"};

            var node2 = new TreeNodeView();
            node2.Content = new Label { Text = "Content 2" };
            node2.HeaderContent = new Label { Text = "Header 2" };

            node.ChildTreeNodeViews.Add(node2);

            TheTreeView.ChildTreeNodeViews.Add(node);
            base.OnAppearing();
        }

        public TreeViewPage()
        {
            InitializeComponent();




            //var grid = new Grid();
            //grid.RowDefinitions.Add(new RowDefinition { Height=100 });
            //grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            //grid.BackgroundColor = Color.Red;

            //TheTreeView.ChildTreeNodeViews.Add(new Adapt.Presentation.Controls.TreeView.TreeNodeView() { Children = { grid } });

        }
    }
}