using samples = Adapt.PresentationSamples;

namespace Adapt.Presentation.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new samples.App(new PresentationFactory(), new Permissions()));
        }
    }
}
