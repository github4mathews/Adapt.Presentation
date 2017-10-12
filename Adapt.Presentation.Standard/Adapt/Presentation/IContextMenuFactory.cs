using System.Collections.Generic;
using Xamarin.Forms;

namespace Adapt.Presentation
{
    public interface IContextMenuFactory
    {
        void Detach(View bindable);

        void Attach(View bindable, IList<MenuItem> contextActions);
    }
}