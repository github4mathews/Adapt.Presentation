using Android.Content;
using System.Threading.Tasks;

namespace Adapt.Presentation.AndroidPlatform
{
    public class Clipboard : IClipboard
    {
        ClipboardManager ClipboardManager => (ClipboardManager)Android.App.Application.Context.GetSystemService(Context.ClipboardService);

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