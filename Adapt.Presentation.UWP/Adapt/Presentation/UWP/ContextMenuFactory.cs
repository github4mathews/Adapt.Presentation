using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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

		public void Attach(View bindable, IList<MenuItem> contextActions)
		{
			//Build the flyout
			var flyout = new MenuFlyout();
			foreach (var action in contextActions)
			{
				//TODO: Don't use ICommand
				flyout.Items.Add(new MenuFlyoutItem { Command = action.Command, Text = action.Text });
			}

			//This is fucking nasty
			var mainThread = SynchronizationContext.Current;
			new TaskFactory().StartNew(() =>
			{
				//Wait for the renderer e.e
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
