using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adapt.Presentation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apa.Adapt.Presentation.AndroidPlatform
{
    class ToastNotifier : IInAppNotification
    {
        public void Show(string text)
        {
            Toast.MakeText(Application.Context, text, ToastLength.Short);
        }
    }
}