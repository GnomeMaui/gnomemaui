using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Platform;

public abstract class SkiaRenderer : ISkiaRenderer
{
	readonly SkiaCanvas _canvas = new();
	readonly ScalingCanvas _scalingCanvas;
	IDrawable? _drawable;

	public SkiaRenderer()
	{
		_scalingCanvas = new ScalingCanvas(_canvas);
	}

	public ICanvas Canvas => _canvas;

	public IDrawable Drawable
	{
		get => _drawable!;
		set
		{
			_drawable = value;
			Invalidate();
		}
	}

	public Color BackgroundColor { get; set; } = Colors.Transparent;

	public void Draw(SKCanvas canvas, RectF dirtyRect)
	{
		if (canvas == null)
		{
			return;
		}

		var oldCanvas = _canvas.Canvas;
		_canvas.Canvas = canvas;

		try
		{
			_canvas.SaveState();

			if (BackgroundColor != null)
			{
				_canvas.FillColor = BackgroundColor;
				_canvas.FillRectangle(dirtyRect);
			}

			_drawable?.Draw(_scalingCanvas, dirtyRect);
		}
		catch (Exception e)
		{
			System.Diagnostics.Debug.WriteLine($"Skia render error: {e}");
		}
		finally
		{
			_canvas.Canvas = oldCanvas;
			_scalingCanvas.ResetState();
		}
	}

	public abstract void Invalidate();

	public void Invalidate(float x, float y, float w, float h) => Invalidate();

	public void SizeChanged(int width, int height) { }

	public void Detached() { }

	public virtual void Dispose()
	{
		_canvas.Dispose();
	}
}
