using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.IO;

namespace Microsoft.Maui.Platform;

public class SKImageView : SKGLArea
{
	private SKImage? _image;
	private Aspect _aspect = Aspect.AspectFit;

	public SKImage? Image
	{
		get => _image;
		set
		{
			_image = value;
			Invalidate();
		}
	}

	public Aspect Aspect
	{
		get => _aspect;
		set
		{
			_aspect = value;
			Invalidate();
		}
	}

	protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		if (_image == null)
			return;

		var info = e.Surface.Canvas.DeviceClipBounds;
		var destRect = CalculateDestRect(info.Width, info.Height);

		using var paint = new SKPaint
		{
			IsAntialias = true
		};

		var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
		canvas.DrawImage(_image, destRect, sampling, paint);
	}

	private SKRect CalculateDestRect(float viewWidth, float viewHeight)
	{
		if (_image == null)
			return SKRect.Empty;

		var imageWidth = _image.Width;
		var imageHeight = _image.Height;

		return _aspect switch
		{
			Aspect.AspectFit => CalculateAspectFit(viewWidth, viewHeight, imageWidth, imageHeight),
			Aspect.AspectFill => CalculateAspectFill(viewWidth, viewHeight, imageWidth, imageHeight),
			Aspect.Fill => new SKRect(0, 0, viewWidth, viewHeight),
			Aspect.Center => CalculateCenter(viewWidth, viewHeight, imageWidth, imageHeight),
			_ => new SKRect(0, 0, viewWidth, viewHeight)
		};
	}

	private SKRect CalculateAspectFit(float vw, float vh, float iw, float ih)
	{
		var scale = Math.Min(vw / iw, vh / ih);
		var scaledWidth = iw * scale;
		var scaledHeight = ih * scale;
		var x = (vw - scaledWidth) / 2;
		var y = (vh - scaledHeight) / 2;
		return new SKRect(x, y, x + scaledWidth, y + scaledHeight);
	}

	private SKRect CalculateAspectFill(float vw, float vh, float iw, float ih)
	{
		var scale = Math.Max(vw / iw, vh / ih);
		var scaledWidth = iw * scale;
		var scaledHeight = ih * scale;
		var x = (vw - scaledWidth) / 2;
		var y = (vh - scaledHeight) / 2;
		return new SKRect(x, y, x + scaledWidth, y + scaledHeight);
	}

	private SKRect CalculateCenter(float vw, float vh, float iw, float ih)
	{
		var x = (vw - iw) / 2;
		var y = (vh - ih) / 2;
		return new SKRect(x, y, x + iw, y + ih);
	}
}
