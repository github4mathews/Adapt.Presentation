using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestXamarinForms.AsyncListView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListViewPage 
    {

        ItemModelProvider items;
        ItemModel two;

        private AsyncListViewModel CurrentAsyncListViewModel => BindingContext as AsyncListViewModel;


        public ListViewPage()
        {
            InitializeComponent();

            CreateNewModel();

            items = (ItemModelProvider)ListViewPageGrid.Resources["items"];
            items.ItemsLoaded += Items_ItemsLoaded;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void SetWaitIndicatorVisibility(bool isVisible)
        {
            ListViewActivityIndicator.IsRunning = isVisible;
            ListViewActivityIndicator.IsVisible = isVisible;
        }



        private void CreateNewModel()
        {
            //Note: if you replace the line below with this, the behaviour works:
            //BindingContext = new AsyncListViewModel { ItemModel = two };

            BindingContext = new AsyncListViewModel { ItemModel = GetNewTwo() };
        }

        private static ItemModel GetNewTwo()
        {
            return new ItemModel { Name = 2, Description = "Second" };
        }

        private void Items_ItemsLoaded(object sender, System.EventArgs e)
        {
            SetWaitIndicatorVisibility(false);
            TheListView.SelectedItems = new ObservableCollection<ItemModel> { new ItemModel { Name = 2, Description = "Second" } };
        }

    }
}