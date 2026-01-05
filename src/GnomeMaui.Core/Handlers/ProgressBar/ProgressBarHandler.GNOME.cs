namespace Microsoft.Maui.Handlers;

public partial class ProgressBarHandler : ViewHandler<IProgress, Gtk.ProgressBar>
{
	protected override Gtk.ProgressBar CreatePlatformView()
		=> this.Create();

	public static void MapProgress(IProgressBarHandler handler, IProgress progress)
		=> handler.PlatformView?.UpdateProgress(progress);

	public static void MapProgressColor(IProgressBarHandler handler, IProgress progress)
		=> handler.PlatformView?.UpdateProgressColor(progress);
}