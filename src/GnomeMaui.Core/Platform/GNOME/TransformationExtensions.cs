namespace Microsoft.Maui.Platform;

public static class TransformationExtensions
{
	public static void UpdateTransformation(this Gtk.Widget? platformView, IView? view)
	{
		if (platformView == null || view == null)
			return;

		// NOTE: Position (Frame.X/Y) is handled by the parent layout manager (BaseWidget with CustomLayout)
		// DO NOT use margins for positioning in GTK4 - margins are subtracted from allocated width/height!

		// Size is also handled by the layout manager during allocation
		// SetSizeRequest is only for hint, actual size comes from child.Allocate() call
	}
}
