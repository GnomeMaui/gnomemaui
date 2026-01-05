namespace Microsoft.Maui.Graphics.Platform;

public interface ISkiaGraphicsRenderer : ISkiaRenderer, IDisposable
{
	SkiaGraphicsView GraphicsView { set; }
}
