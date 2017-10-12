using System;

namespace Adapt.Presentation.Behaviours
{
	public class ContextMenuItem
	{
		public event EventHandler Clicked;
		public string Text { get; set; }

		public void InvokeClicked()
		{
			Clicked?.Invoke(this, null);
		}
	}
}