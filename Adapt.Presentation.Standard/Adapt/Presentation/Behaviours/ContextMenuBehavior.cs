using System.Collections.Generic;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
	public class ContextMenuBehavior : Behavior<View>
	{
		public IList<MenuItem> ContextActions => (IList<MenuItem>)GetValue(ContextActionsProperty);
		public static BindableProperty ContextActionsProperty = BindableProperty.Create(nameof(ContextActions), typeof(IList<MenuItem>), typeof(ContextMenuBehavior), new List<MenuItem>());

		protected override void OnAttachedTo(View bindable)
		{
			base.OnAttachedTo(bindable);
			DependencyService.Get<IContextMenuFactory>().Attach(bindable, ContextActions);
		}

		protected override void OnDetachingFrom(View bindable)
		{
			base.OnDetachingFrom(bindable);
			DependencyService.Get<IContextMenuFactory>().Detach(bindable);
		}
	}
}
