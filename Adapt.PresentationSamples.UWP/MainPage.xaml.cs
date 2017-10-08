using Adapt.Presentation.UWP;
using Adapt.Presentation.UWP.Adapt.Presentation.UWP;
using Adapt.Presentation.UWP.Geolocator;
using samples = Adapt.PresentationSamples;

namespace XamForms.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            var app = new samples.App(new PresentationFactory(), new Permissions(), new Geolocator(), new Clipboard(), new InAppNotification(), new FileSource());

            LoadApplication(app);
        }
    }
}
