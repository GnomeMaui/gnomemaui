using System;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, Gtk.SpinButton>
	{
		protected override Gtk.SpinButton CreatePlatformView() => this.Create();

		public static void MapMinimum(IViewHandler handler, IStepper stepper) { }
		public static void MapMaximum(IViewHandler handler, IStepper stepper) { }
		public static void MapIncrement(IViewHandler handler, IStepper stepper) { }
		public static void MapValue(IViewHandler handler, IStepper stepper) { }
	}
}