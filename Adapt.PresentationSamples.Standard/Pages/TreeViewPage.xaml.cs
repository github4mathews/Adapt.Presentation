
using Adapt.Presentation.Controls;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Pages
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
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


            TheTreeView.SelectedItemChanged += TheTreeView_SelectedItemChanged;

            var aBindingContext1 = new Something { TestString = "Content 1" };
            var aBindingContext2 = new Something { TestString = "Content 2" };
            var aBindingContext3 = new Something { TestString = "Content 3" };
            var aBindingContext4 = new Something { TestString = "Content 4" };

            var node = new TreeViewItem();
            var label = new Label { BackgroundColor = Color.Purple, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
            node.Header = label;
            label.SetBinding(Label.TextProperty, new Binding("TestString"));
            node.BindingContext = aBindingContext1;

            var node2 = new TreeViewItem();
            var label2 = new Label { BackgroundColor = Color.Green, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
            node2.Header = label2;
            label2.SetBinding(Label.TextProperty, new Binding("TestString"));
            node2.BindingContext = aBindingContext2;

            var node3 = new TreeViewItem();
            var label3 = new Label { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
            node3.Header = label3;
            label3.SetBinding(Label.TextProperty, new Binding("TestString"));
            node3.BindingContext = aBindingContext3;

            var node4 = new TreeViewItem();
            var label4 = new Label { BackgroundColor = Color.Pink, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
            node4.Header = label4;
            label4.SetBinding(Label.TextProperty, new Binding("TestString"));
            node4.BindingContext = aBindingContext4;

            node.ItemsSource.Add(node2);
            node2.ItemsSource.Add(node3);
            TheTreeView.ItemsSource.Add(node);
            TheTreeView.ItemsSource.Add(node4);

            base.OnAppearing();
        }

        private async void TheTreeView_SelectedItemChanged(object sender, EventArgs e)
        {
            var selectedItem = TheTreeView.SelectedItem?.BindingContext as Something;
            if (selectedItem != null)
            {
                await DisplayAlert("Item Selected", $"Selected Content: {selectedItem.TestString}", "OK");
            }
        }

        public TreeViewPage()
        {
            InitializeComponent();
            var firstKey = TheAdaptTreeViewItemProvider.ItemsTemplates.FirstOrDefault();
        }
    }

    public class Something
    {
        public string TestString { get; set; }
    }
}