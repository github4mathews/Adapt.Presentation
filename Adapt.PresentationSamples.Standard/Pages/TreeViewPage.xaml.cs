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
            var node = new TreeNodeView(headertemplate, contenttemplate);
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