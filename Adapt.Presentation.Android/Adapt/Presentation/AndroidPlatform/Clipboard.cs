using System.Threading.Tasks;
using Adapt.Presentation;
using Android.App;
using Android.Content;

namespace apa.Adapt.Presentation.AndroidPlatform
{
    public class Clipboard : IClipboard
    {
        ClipboardManager ClipboardManager => (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);

        public async Task<string> GetClipboardTextAsync()
        {
            return await Task.Run(() => GetClipboardText());
        }

        private string GetClipboardText()
        {
            return ClipboardManager.Text;
        }

        public void SetClipboardText(string data)
        {
            ClipboardManager.Text = data;
        }
    }
}