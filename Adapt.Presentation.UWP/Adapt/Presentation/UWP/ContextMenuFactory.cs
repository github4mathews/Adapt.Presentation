using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Adapt.Presentation.Behaviours;
using Adapt.Presentation.UWP.Adapt.Presentation.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

//TODO: Possible to dependency inject this manually?
[assembly: Dependency(typeof(ContextMenuFactory))]
namespace Adapt.Presentation.UWP.Adapt.Presentation.UWP
{
    class ContextMenuFactory : IContextMenuFactory
    {
        public void Detach(View bindable)
        {
            FlyoutBase.SetAttachedFlyout(Platform.GetRenderer(bindable).ContainerElement, null);
        }

        public void Attach(View bindable, IList<ContextMenuItem> contextActions)
        {
            //Build the flyout
            var flyout = new MenuFlyout();
            foreach (var action in contextActions)
            {
                var menuteim = new MenuFlyoutItem { Text = action.Text, Tag = action };
                menuteim.Click += (s, e) => action.InvokeClicked();
                flyout.Items.Add(menuteim);
            }

            //This is fucking nasty
            var mainThread = SynchronizationContext.Current;
            new TaskFactory().StartNew(() =>
            {
                //Wait for the renderer, creating it does not work in UWP for some reason :/
                IVisualElementRenderer renderer = null;
                while (renderer == null)
                {
                    renderer = Platform.GetRenderer(bindable);
                }

                //Renderer is all good, set the flyout and the event
                mainThread.Post(arg =>
                {
                    var nativeElement = renderer.ContainerElement;
                    FlyoutBase.SetAttachedFlyout(nativeElement, flyout);
                    nativeElement.RightTapped += (s, e) => FlyoutBase.ShowAttachedFlyout(nativeElement);
                }, null);
            });
        }
    }
}
