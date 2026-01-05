using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using System;

namespace SkiaSharp.Views.Maui.Handlers;

public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKGLArea>
{
	SKSizeI lastCanvasSize;
	GRContext? lastGRContext;
	SKTouchHandler? touchHandler;

	protected override SKGLArea CreatePlatformView() => new SKGLArea();

	protected override void ConnectHandler(SKGLArea platformView)
	{
		platformView.PaintSurface += OnPaintSurface;

		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(SKGLArea platformView)
	{
		touchHandler?.Detach(platformView);
		touchHandler = null;

		platformView.PaintSurface -= OnPaintSurface;

		base.DisconnectHandler(platformView);
	}

	void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
	{
		var newCanvasSize = e.Info.Size;
		if (lastCanvasSize != newCanvasSize)
		{
			lastCanvasSize = newCanvasSize;
			VirtualView?.OnCanvasSizeChanged(newCanvasSize);
		}
		if (sender is SKGLArea platformView)
		{
			var newGRContext = platformView.GRContext;
			if (lastGRContext != newGRContext)
			{
				lastGRContext = newGRContext;
				VirtualView?.OnGRContextChanged(newGRContext);
			}
		}

		VirtualView?.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
	}

	public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler?.PlatformView is not SKGLArea pv)
		{
			return;
		}

		pv.IgnorePixelScaling = view.IgnorePixelScaling;
		pv.Invalidate();
	}

	public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler?.PlatformView == null)
		{
			return;
		}

		handler.PlatformView.EnableRenderLoop = view.HasRenderLoop;
	}

	public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
	{
		if (handler?.PlatformView == null)
		{
			return;
		}

		handler.touchHandler ??= new SKTouchHandler(
			args => view.OnTouch(args),
			(x, y) => handler.OnGetScaledCoord(x, y));

		handler.touchHandler?.SetEnabled(handler.PlatformView, view.EnableTouchEvents);
	}

	public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
	{
		if (handler?.PlatformView == null)
			return;

		if (!handler.PlatformView.EnableRenderLoop)
			handler.PlatformView.Invalidate();
	}

	SKPoint OnGetScaledCoord(double x, double y)
	{
		if (VirtualView?.IgnorePixelScaling == false && PlatformView != null)
		{
			var scale = 1.0f; // TODO: Get the actual scale factor

			x *= scale;
			y *= scale;
		}

		return new SKPoint((float)x, (float)y);
	}
}
