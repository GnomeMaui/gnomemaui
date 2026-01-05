using SkiaSharp;

namespace Microsoft.Maui.Graphics.Platform;

public interface ISkiaRenderer : IDisposable
{
	ICanvas Canvas { get; }
	IDrawable Drawable { get; set; }
	Color BackgroundColor { get; set; }
	void Draw(SKCanvas canvas, RectF dirtyRect);
	void SizeChanged(int width, int height);
	void Detached();
	void Invalidate();
	void Invalidate(float x, float y, float w, float h);
}
