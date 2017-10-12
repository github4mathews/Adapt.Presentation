using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Adapt.Presentation.UWP.Adapt.Presentation.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

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
			var flyout = new MenuFlyout();
			foreach (var action in contextActions)
			{
				//TODO: Don't use ICommand
				flyout.Items.Add(new MenuFlyoutItem { Command = action.Command, Text = action.Text });
			}

			var renderer = Platform.GetRenderer(bindable);
			FlyoutBase.SetAttachedFlyout(renderer.ContainerElement, flyout);
		}
	}
}
