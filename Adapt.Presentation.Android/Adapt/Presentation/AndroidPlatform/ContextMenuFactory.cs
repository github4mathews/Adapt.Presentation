using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using apa.Adapt.Presentation.AndroidPlatform;
using Adapt.Presentation;
using Adapt.Presentation.Behaviours;
using Android.App;
using Android.Runtime;
using Android.Support.V7.View;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Xamarin.Forms.View;

//TODO: Possible to dependency inject this manually?
[assembly: Xamarin.Forms.Dependency(typeof(ContextMenuFactory))]

namespace apa.Adapt.Presentation.AndroidPlatform
{
    class ContextMenuFactory : IContextMenuFactory
    {
        public void Detach(View bindable)
        {
            ContextMenu.Dispose();
        }

        public void Attach(View bindable, IList<ContextMenuItem> contextActions)
        {
            //Set properties
            ContextActions = contextActions;
            View = bindable;

            //Set the renderer, it won't be created yet :/
            var renderer = Platform.CreateRenderer(View);
            Platform.SetRenderer(View, renderer);
            var nativeView = (Android.Views.View)renderer;

            //Create a context menu and add the items
            ContextMenu = new PopupMenu(Forms.Context, nativeView);
            foreach (var action in ContextActions)
            {
                ContextMenu.Menu.Add(Menu.None, ContextActions.IndexOf(action), Menu.None, action.Text);
            }

            //Attach long click listener
            nativeView.LongClick += ObjectLongTapped;

            //Handle item clicks
            ContextMenu.MenuItemClick += ContextMenu_MenuItemClick;
        }

        public IList<ContextMenuItem> ContextActions { get; set; }
        public PopupMenu ContextMenu { get; set; }
        public View View { get; private set; }

        private void ObjectLongTapped(object sender, Android.Views.View.LongClickEventArgs longClickEventArgs)
        {
            ContextMenu.Show();
        }

        private void ContextMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            ContextActions[e.Item.ItemId].InvokeClicked();
        }
    }
}