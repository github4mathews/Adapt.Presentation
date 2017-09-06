using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapt.Presentation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apa.Adapt.Presentation.AndroidPlatform
{
    public class Clipboard : IClipboard
    {
        ClipboardManager ClipboardManager => (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);

        public async Task<string> GetClipboardText()
        {
            return ClipboardManager.Text;
        }

        public void SetClipboardText(string data)
        {
            ClipboardManager.Text = data;
        }
    }
}