using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, Gtk.Spinner>
	{
		protected override Gtk.Spinner CreatePlatformView() => this.Create();

		public static partial void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView.UpdateIsRunning(activityIndicator);
		}
		public static partial void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView.UpdateColor(activityIndicator);
		}
	}
}