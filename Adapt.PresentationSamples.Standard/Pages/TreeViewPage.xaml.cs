using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;


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
            var aBindingContext2 = new Something2 { TestString = "Content 2" };
            var aBindingContext3 = new Something3 { TestString = "Content 3" };
            var aBindingContext4 = new Something4 { TestString = "Content 4" };

            aBindingContext1.Somethings.Add(aBindingContext2);
            aBindingContext2.Somethings.Add(aBindingContext3);

            var itemsSource = new ObservableCollection<Something>();

            itemsSource.Add(aBindingContext1);
            itemsSource.Add(aBindingContext4);

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
        public SomethingList Somethings { get; } = new SomethingList();
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