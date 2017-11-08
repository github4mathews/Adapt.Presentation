using Adapt.Presentation.Controls;
using System;
using System.Collections.ObjectModel;
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

            TheTreeView.SelectedItemChanged += TheTreeView_SelectedItemChanged;

            var something = new Something { TestString = "Content 1" };
            var something2 = new Something2 { TestString = "Content 2" };
            something.Children.Add(something2);

            var itemsSource = new ObservableCollection<Something>();

            itemsSource.Add(something);

            TheAdaptTreeViewItemProvider.ItemsSource = itemsSource;


            var node = new TreeViewItem();
            node.Header = new Label { BackgroundColor = Color.Purple, Text = "Content", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            var node2 = new TreeViewItem();
            node2.Header = new Label { BackgroundColor = Color.Green, Text = "Content 2", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            var node3 = new TreeViewItem();
            node3.Header = new Label { BackgroundColor = Color.Red, Text = "Content 3", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            var node4 = new TreeViewItem();
            node4.Header = new Label { BackgroundColor = Color.Pink, Text = "Content 4", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };

            node.Children.Add(node2);
            node2.Children.Add(node3);

            TheTreeView2.ItemsSource.Add(node);
            TheTreeView2.ItemsSource.Add(node4);

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
        }
    }

    public class SomethingList : ObservableCollection<Something>
    {

    }

    public class Something
    {
        public string TestString { get; set; }
        public SomethingList Children { get; } = new SomethingList();
    }

    public class Something2 : Something
    {
    }

    public class Something3 : Something
    {
    }

    public class Something4 : Something
    {
    }
}