using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Microsoft.Maui.Graphics.Platform;

/// <summary>
/// A GTK drawing area for SkiaSharp rendering.
/// </summary>
public class SKDrawingArea : DrawingArea, IDisposable
{
	SKImageInfo? _cachedImageInfo;
	bool ignorePixelScaling;

	public SKDrawingArea() : base()
	{
		Vexpand = true;
		Hexpand = true;
		SetDrawFunc(DrawCallback);
	}

	public float Scale { get; private set; } = 1;

	/// <summary>
	/// Occurs when the drawing area needs to be repainted.
	/// </summary>
	public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

	/// <summary>
	/// Gets the size of the canvas.
	/// </summary>
	public SKSize CanvasSize => new(GetAllocatedWidth(), GetAllocatedHeight());

	/// <summary>
	/// Invalidates the drawing area, causing it to be redrawn.
	/// </summary>
	public void Invalidate() => QueueDraw();

	/// <summary>
	/// Raises the PaintSurface event.
	/// </summary>
	/// <param name="e">The <see cref="SKPaintSurfaceEventArgs"/> instance containing the event data.</param>
	protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e) => PaintSurface?.Invoke(this, e);

	void DrawCallback(DrawingArea area, Cairo.Context context, int width, int height)
	{
		if (width == 0 || height == 0)
		{
			return;
		}

		if (_cachedImageInfo?.Width != width || _cachedImageInfo?.Height != height)
		{
			_cachedImageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		}

		var imageInfo = _cachedImageInfo.Value;

		using var imageSurface = new Cairo.ImageSurface(Cairo.Format.Argb32, imageInfo.Width, imageInfo.Height);
		var data = Cairo.Internal.ImageSurface.GetData(imageSurface.Handle);
		using var surface = SKSurface.Create(imageInfo, data, imageInfo.RowBytes);

		using (new SKAutoCanvasRestore(surface.Canvas, true))
		{
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, imageInfo));
		}

		surface.Canvas.Flush();
		imageSurface.MarkDirty();

		if (imageInfo.ColorType == SKColorType.Rgba8888)
		{
			using var pixmap = surface.PeekPixels();
			SKSwizzle.SwapRedBlue(pixmap.GetPixels(), imageInfo.Width * imageInfo.Height);
		}

		context.SetSourceSurface(imageSurface, 0, 0);
		context.Paint();
	}

	// TODO : implement proper DPI handling
	public bool IgnorePixelScaling
	{
		get => ignorePixelScaling;
		set
		{
			ignorePixelScaling = value;
			Invalidate();
		}
	}


	public override void Dispose()
	{
		_cachedImageInfo = null;
		base.Dispose();
		GC.SuppressFinalize(this);
	}
}
