using Adapt.Presentation.UWP;
using System.IO;
using Windows.Storage;
using samples = Adapt.PresentationSamples;

namespace XamForms.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var app = new samples.App(new PresentationFactory(), new Permissions());

            LoadApplication(app);
        }
    }
}
