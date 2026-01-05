namespace Microsoft.Maui.Graphics.Platform;

public class SkiaGLDirectRenderer : SkiaRenderer, ISkiaGLGraphicsRenderer
{
	public SkiaGLGraphicsView GraphicsView { private get; set; } = default!;
	public override void Invalidate() => GraphicsView?.Invalidate();

	public override void Dispose()
	{
		base.Dispose();
		GraphicsView.Dispose();
		GraphicsView = default!;
		GC.SuppressFinalize(this);
	}
}
