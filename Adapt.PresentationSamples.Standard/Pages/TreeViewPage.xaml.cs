
using Adapt.Presentation.Controls.TreeView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeViewPage : ContentPage
    {
        private bool _IsLoaded;

        protected override void OnAppearing()
        {
            if (_IsLoaded)
            {
                return;
            }

            _IsLoaded = true;



            var node = new TreeViewNode();
            node.Content = new Label { BackgroundColor = Color.Purple, Text = "Content", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            var node2 = new TreeViewNode();
            node2.Content = new Label { BackgroundColor = Color.Green, Text = "Content 2", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            var node3 = new TreeViewNode();
            node3.Content = new Label { BackgroundColor = Color.Red, Text = "Content 3", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            var node4 = new TreeViewNode();
            node4.Content = new Label { BackgroundColor = Color.Pink, Text = "Content 4", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            node.Children.Add(node2);
            node2.Children.Add(node3);

            TheTreeView.Children.Add(node);
            TheTreeView.Children.Add(node4);

            base.OnAppearing();
        }

        public TreeViewPage()
        {
            InitializeComponent();
        }
    }
}