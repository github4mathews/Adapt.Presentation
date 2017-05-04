using Adapt.Presentation;

using Xamarin.Forms;

namespace Adapt.PresentationSamples
{
    public partial class App
    {
        public static IPresentationFactory PresentationFactory { get; private set; }
        public static IPermissions CurrentPermissions { get; private set; }

        public App(IPresentationFactory presentationFactory, IPermissions currentPermissions)
        {
            PresentationFactory = presentationFactory;
            CurrentPermissions = currentPermissions;
            InitializeComponent();
            MainPage = new MainPage();
        }

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
    }
}
