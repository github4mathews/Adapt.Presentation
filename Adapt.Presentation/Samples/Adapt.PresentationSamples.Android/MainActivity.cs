
using Android.App;
using Android.Content.PM;
using Android.OS;
using samples = Adapt.PresentationSamples;
using droid = Xamarin.Forms.Platform.Android;
using Application;
using Adapt.Presentation.AndroidPlatform;

namespace Adapt.Presentation.Droid
{
    [Activity(Label = "Adapt.Presentation", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : droid.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new samples.App(new PresentationFactory(ApplicationContext), new Permissions()));
        }
    }
}

