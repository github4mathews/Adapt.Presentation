using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Adapt.Presentation.UWP.Adapt.Presentation.UWP
{
    public class Clipboard : IClipboard
    {
        public async Task<string> GetClipboardText()
        {
            var content = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            return await content.GetTextAsync();
        }

        public void SetClipboardText(string data)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(data);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
    }
}
