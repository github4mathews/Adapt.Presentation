using Adapt.Presentation;
using Android.Widget;

namespace apa.Adapt.Presentation.AndroidPlatform
{
    public class InAppNotification : IInAppNotification
    {
        public void Show(string text)
        {
            Toast.MakeText(Android.App.Application.Context, text, ToastLength.Short).Show();
        }
    }
}