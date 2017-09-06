using System.Threading.Tasks;

namespace Adapt.Presentation
{
    public interface IClipboard
    {
        Task<string> GetClipboardText();
        void SetClipboardText(string data);
    }
}
