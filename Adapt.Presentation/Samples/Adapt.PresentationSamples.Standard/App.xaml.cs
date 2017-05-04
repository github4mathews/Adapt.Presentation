using Adapt.Presentation;

using Xamarin.Forms;

namespace Adapt.PresentationSamples
{
    public partial class App : Application
	{
        public static IPresentationFactory PresentationFactory { get; private set; }

        public App (IPresentationFactory presentationFactory)
		{
            PresentationFactory = presentationFactory;
            InitializeComponent();
			MainPage = new MainPage();
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
