
using Adapt.Presentation.AndroidPlatform;
using Adapt.Presentation.AndroidPlatform.Geolocator;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using samples = Adapt.PresentationSamples;
using xf = Xamarin.Forms;

namespace XamForms.Droid
{
    [Activity(Label = "Adapt.Presentation Samples", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : xf.Platform.Android.FormsApplicationActivity, IRequestPermissionsActivity
    {
        #region Events
        public event PermissionsRequestCompletedHander PermissionsRequestCompleted;
        #endregion

        #region Protected Overrides
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            xf.Forms.Init(this, bundle);
            var permissions = new Permissions(this);
            LoadApplication(new samples.App(new PresentationFactory(ApplicationContext, permissions), permissions, new Geolocator(permissions)));
        }
        #endregion

        #region Public Overrides
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsRequestCompleted?.Invoke(requestCode, permissions, grantResults);
        }
        #endregion
    }
}

