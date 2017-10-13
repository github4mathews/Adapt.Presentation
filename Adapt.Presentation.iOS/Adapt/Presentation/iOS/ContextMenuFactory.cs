using System.Collections.Generic;
using Adapt.Presentation.Behaviours;
using Adapt.Presentation.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using View = Xamarin.Forms.View;

//TODO: Possible to dependency inject this manually?
[assembly: Dependency(typeof(ContextMenuFactory))]

namespace Adapt.Presentation.iOS
{
    class ContextMenuFactory : IContextMenuFactory
    {
        public void Detach(View bindable)
        {
            ContextMenu.Dispose();
        }

        public void Attach(View bindable, IList<ContextMenuItem> contextActions)
        {
            //Set the renderer, it won't be created yet :/
            var renderer = Platform.CreateRenderer(bindable);
            Platform.SetRenderer(bindable, renderer);

            //Create a context menu and add the items
            ContextMenu = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            foreach (var action in contextActions)
            {
                ContextMenu.AddAction(UIAlertAction.Create(action.Text, UIAlertActionStyle.Default, a => action.InvokeClicked()));
            }

            //Set location to popout from
            ContextMenu.PopoverPresentationController.SourceView = renderer.NativeView;
            ContextMenu.PopoverPresentationController.SourceRect = renderer.NativeView.Frame;
        }

        public UIAlertController ContextMenu { get; set; }

        public void ShowContextMenu()
        {
            var rootViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            rootViewController.PresentViewController(ContextMenu, true, null);
        }
    }
}