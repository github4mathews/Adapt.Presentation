using System;
using System.Collections.ObjectModel;
using TestXamarinForms.AsyncListView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListViewPage
    {
        #region Fields
        private ItemModelProvider _Items;
        #endregion

        #region Private Properties
        private AsyncListViewModel CurrentAsyncListViewModel => BindingContext as AsyncListViewModel;
        #endregion

        #region Constructor
        public ListViewPage()
        {
            InitializeComponent();
            CreateNewModel();
        }
        #endregion

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Resources = new ResourceDictionary();
            _Items = new ItemModelProvider();
            _Items.ItemsLoaded += Items_ItemsLoaded;
            Resources.Add("items", _Items);
        }

        #region Private Methods
        private void SetWaitIndicatorVisibility(bool isVisible)
        {
            ListViewActivityIndicator.IsRunning = isVisible;
            ListViewActivityIndicator.IsVisible = isVisible;
        }

        private void CreateNewModel()
        {
            BindingContext = new AsyncListViewModel { ItemModel = GetNewTwo() };
        }

        private static ItemModel GetNewTwo()
        {
            return new ItemModel { Name = 2, Description = "Second" };
        }
        #endregion

        #region Event Handlers
        private void Items_ItemsLoaded(object sender, EventArgs e)
        {
            SetWaitIndicatorVisibility(false);
            TheListView.SelectedItems = new ObservableCollection<ItemModel> { new ItemModel { Name = 2, Description = "Second" } };
        }
        #endregion
    }
}