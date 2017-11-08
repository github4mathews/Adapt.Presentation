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