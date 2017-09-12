using Xamarin.Forms;

namespace Adapt.Presentation
{
    public interface IInAppNotification
    {
        void Show(string text);

        void Attach(Page view);
    }
}
