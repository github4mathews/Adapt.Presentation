
using Adapt.Presentation.AndroidPlatform;
using Android.App;
using Android.Content.PM;
using Android.OS;
using samples = Adapt.PresentationSamples;

namespace XamForms.Droid
{
    [Activity (Label = "XamForms", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);


            LoadApplication(new samples.App(new PresentationFactory(ApplicationContext), new Permissions()));
        }
	}
}

