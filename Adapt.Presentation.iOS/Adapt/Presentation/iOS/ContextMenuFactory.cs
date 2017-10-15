using System.Collections.Generic;
using Adapt.Presentation.Behaviours;
using Adapt.Presentation.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Platform = Xamarin.Forms.Platform.iOS.Platform;
using View = Xamarin.Forms.View;

//TODO: Possible to dependency inject this manually?
[assembly: Dependency(typeof(ContextMenuFactory))]

namespace Adapt.Presentation.iOS
{
    class ContextMenuFactory : IContextMenuFactory
    {
        private IVisualElementRenderer _TheRenderer;

        public void Detach(View bindable)
        {
            ContextMenu.Dispose();
        }

        public void Attach(View bindable, IList<ContextMenuItem> contextActions)
        {
            //Set the renderer, it won't be created yet :/
            _TheRenderer = Platform.CreateRenderer(bindable);
            Platform.SetRenderer(bindable, _TheRenderer);

            //Create a context menu and add the items
            ContextMenu = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            foreach (var action in contextActions)
            {
                ContextMenu.AddAction(UIAlertAction.Create(action.Text, UIAlertActionStyle.Default, a => action.InvokeClicked()));
            }

            //Attach the long tap recogniser
            var longTapRecogniser = new UILongPressGestureRecognizer();
            longTapRecogniser.AddTarget(ShowContextMenu);
            _TheRenderer.NativeView.AddGestureRecognizer(longTapRecogniser);
        }

        public UIAlertController ContextMenu { get; set; }

        public void ShowContextMenu()
        {
            //Set location to popout from
            ContextMenu.PopoverPresentationController.SourceView = _TheRenderer.NativeView;
            ContextMenu.PopoverPresentationController.SourceRect = _TheRenderer.NativeView.Frame;

            //Present the context menu
            var rootViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            rootViewController.PresentViewController(ContextMenu, true, null);
        }
    }
}