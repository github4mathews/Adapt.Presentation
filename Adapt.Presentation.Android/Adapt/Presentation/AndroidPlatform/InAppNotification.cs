using Adapt.Presentation;
using Android.Widget;
using Application = Android.App.Application;

namespace apa.Adapt.Presentation.AndroidPlatform
{
    public class InAppNotification : IInAppNotification
    {
        public void Show(string text)
        {
            Toast.MakeText(Application.Context, text, ToastLength.Short).Show();
        }
    }
}