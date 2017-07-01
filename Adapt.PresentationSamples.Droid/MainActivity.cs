
using Adapt.Presentation.AndroidPlatform;
using Adapt.Presentation.AndroidPlatform.Geolocator;
using Android.App;
using Android.Content.PM;
using Android.OS;
using samples = Adapt.PresentationSamples;
using xf = Xamarin.Forms;

namespace XamForms.Droid
{
    [Activity(Label = "XamForms", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : xf.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            xf.Forms.Init(this, bundle);

            var permissions = new Permissions();

            LoadApplication(new samples.App(new PresentationFactory(ApplicationContext), permissions, new Geolocator(permissions)));
        }
    }
}

