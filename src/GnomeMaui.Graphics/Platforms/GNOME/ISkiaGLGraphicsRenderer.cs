namespace Microsoft.Maui.Graphics.Platform;

public interface ISkiaGLGraphicsRenderer : ISkiaRenderer, IDisposable
{
	SkiaGLGraphicsView GraphicsView { set; }
}
