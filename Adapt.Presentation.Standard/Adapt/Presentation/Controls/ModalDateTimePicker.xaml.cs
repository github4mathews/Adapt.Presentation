using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Adapt.Presentation.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalDateTimePicker
    {
        #region Events
        public event EventHandler Closing;
        #endregion

        #region Fields
        private bool _IsOkClicked;
        #endregion

        #region Public Properties
        public NavigationPage NavigationPage { get; set; }

        public ModalDateTimePickerButton.PickerNavigationMode NavigationMode { get; set; }

        public DateTime SelectedValue
        {
            get
            {
                DateTime date;

                date = TheCalendar.Date.Date == new DateTime(1900, 1, 1) ? DateTime.MinValue : TheCalendar.Date.Date;

                return date + TheTimePicker.Time;
            }
            set
            {
                TheCalendar.Date = value == DateTime.MinValue ? DateTime.Now.Date : value.Date;

                TheTimePicker.Time = value - value.Date;
            }
        }

        public bool IsOkClicked => _IsOkClicked;

        #endregion

        #region Constructor
        public ModalDateTimePicker()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private async void OKButton_Clicked(object sender, EventArgs e)
        {
            _IsOkClicked = true;
            await Close();
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            _IsOkClicked = false;
            await Close();
        }

        private async Task Close()
        {
            Closing?.Invoke(this, new EventArgs());

            switch (NavigationMode)
            {
                case ModalDateTimePickerButton.PickerNavigationMode.Modal:
                    await Navigation.PopModalAsync();
                    break;
                case ModalDateTimePickerButton.PickerNavigationMode.SDI:
                    if (NavigationPage == null)
                    {
                        throw new Exception(ModalDateTimePickerButton.NoNavigationPageMessage);
                    }
                    await NavigationPage.PopAsync();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async void Clear_Clicked(object sender, EventArgs e)
        {
            TheCalendar.Date = DateTime.MinValue;
            TheTimePicker.Time = new TimeSpan();
            _IsOkClicked = true;
            await Close();
        }

        private async void Now_Clicked(object sender, EventArgs e)
        {
            TheCalendar.Date = DateTime.Now;
            TheTimePicker.Time = DateTime.Now.TimeOfDay;
            _IsOkClicked = true;
            await Close();
        }

        #endregion
    }
}