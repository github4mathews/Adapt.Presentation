using System.Threading.Tasks;

namespace Adapt.Presentation
{
    public interface IClipboard
    {
        Task<string> GetClipboardTextAsync();
        void SetClipboardText(string data);
    }
}
