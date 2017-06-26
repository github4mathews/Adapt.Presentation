using Adapt.Presentation;
using Xamarin.Forms;

namespace Adapt.PresentationSamples
{
    public partial class App
    {
        #region Fields
        private ContentPage _MainPage;
        #endregion

        #region Public Static Properties
        public static IPresentationFactory PresentationFactory { get; private set; }
        public static IPermissions CurrentPermissions { get; private set; }
        #endregion

        #region Constructor
        public App(IPresentationFactory presentationFactory, IPermissions currentPermissions)
        {
            PresentationFactory = presentationFactory;
            CurrentPermissions = currentPermissions;

            var mainPage = new MainPage();

            InitializeComponent();
            MainPage = mainPage;
        }
        #endregion

        #region App Events
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion
    }
}
