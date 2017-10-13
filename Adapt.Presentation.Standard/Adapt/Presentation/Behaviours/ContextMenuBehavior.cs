using System.Collections.Generic;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
    public class ContextMenuBehavior : Behavior<View>
    {
        public IList<ContextMenuItem> ContextActions => (IList<ContextMenuItem>)GetValue(ContextActionsProperty);
        public static BindableProperty ContextActionsProperty = BindableProperty.Create(nameof(ContextActions), typeof(IList<ContextMenuItem>), typeof(ContextMenuBehavior), new List<ContextMenuItem>());
        readonly IContextMenuFactory _ContextMenu = DependencyService.Get<IContextMenuFactory>(DependencyFetchTarget.NewInstance);

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);
            _ContextMenu.Attach(bindable, ContextActions);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);
            _ContextMenu.Detach(bindable);
        }
    }
}
