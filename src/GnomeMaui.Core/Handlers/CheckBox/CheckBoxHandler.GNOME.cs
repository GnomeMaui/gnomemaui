using System;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, Gtk.CheckButton>
	{
		protected override Gtk.CheckButton CreatePlatformView() => this.Create();

		public static partial void MapIsChecked(ICheckBoxHandler handler, ICheckBox check) { }

		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check) { }
	}
}