using System.Threading.Tasks;
using UIKit;

namespace Adapt.Presentation.iOS
{
    public class Clipboard : IClipboard
    {
        public async Task<string> GetClipboardTextAsync()
        {
            return await Task.Run(() => GetClipboardText());
        }

        private static string GetClipboardText()
        {
            return UIPasteboard.General.String;
        }

        public void SetClipboardText(string data)
        {
            UIPasteboard.General.String = data;
        }
    }
}