using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using System;

namespace SkiaSharp.Views.Maui.Handlers;

public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, SKDrawingArea>
{
	SKSizeI lastCanvasSize;
	SKTouchHandler? touchHandler;

	protected override SKDrawingArea CreatePlatformView() => new SKDrawingArea();

	protected override void ConnectHandler(SKDrawingArea platformView)
	{
		platformView.PaintSurface += OnPaintSurface;

		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(SKDrawingArea platformView)
	{
		touchHandler?.Detach(platformView);
		touchHandler = null;

		platformView.PaintSurface -= OnPaintSurface;

		base.DisconnectHandler(platformView);
	}

	public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args)
	{
		handler?.PlatformView?.Invalidate();
	}

	public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView)
	{
		handler?.PlatformView?.IgnorePixelScaling = canvasView.IgnorePixelScaling;
	}

	public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView)
	{
		handler.touchHandler ??= new SKTouchHandler(
			args => canvasView.OnTouch(args),
			(x, y) => handler.OnGetScaledCoord(x, y));

		handler.touchHandler?.SetEnabled(handler.PlatformView, canvasView.EnableTouchEvents);
	}

	// helper methods

	void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var newCanvasSize = e.Info.Size;
		if (lastCanvasSize != newCanvasSize)
		{
			lastCanvasSize = newCanvasSize;
			VirtualView?.OnCanvasSizeChanged(newCanvasSize);
		}

		VirtualView?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info, e.RawInfo));
	}

	SKPoint OnGetScaledCoord(double x, double y)
	{
		if (VirtualView?.IgnorePixelScaling == false && PlatformView != null)
		{
			var scale = 1.0f; // TODO: get actual scale from GTK

			x *= scale;
			y *= scale;
		}

		return new SKPoint((float)x, (float)y);
	}
}
