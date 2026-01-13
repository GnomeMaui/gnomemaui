using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Microsoft.Maui.Graphics.Platform;

/// <summary>
/// A GTK OpenGL area for SkiaSharp rendering.
/// </summary>
public class SKGLArea : GLArea, IDisposable
{
	const SKColorType ColorType = SKColorType.Rgba8888;
	const GRSurfaceOrigin SurfaceOrigin = GRSurfaceOrigin.BottomLeft;

	bool ignorePixelScaling;
	bool enableRenderLoop;
	uint _lastFramebufferId = 0;

	GRContext? _context;
	GRBackendRenderTarget? _renderTarget;
	SKSurface? _surface;

	public SKGLArea() : base()
	{
		Vexpand = true;
		Hexpand = true;

		// KRITIKUS FIX #1: AutoRender = false
		// ListView recycling során ez megelőzi a race condition-öket
		AutoRender = false;

		OnRender += RenderHandler;
		OnRealize += RealizeHandler;
		OnUnrealize += UnrealizeHandler;
	}

	public bool EnableRenderLoop
	{
		get => enableRenderLoop;
		set
		{
			if (enableRenderLoop != value)
			{
				enableRenderLoop = value;
				UpdateRenderLoop(value);
			}
		}
	}

	public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

	public SKSize CanvasSize => new(GetAllocatedWidth(), GetAllocatedHeight());

	public GRContext GRContext => _context!;

	public void Invalidate() => QueueRender();

	protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e) => PaintSurface?.Invoke(this, e);

	void RealizeHandler(object? sender, EventArgs e)
	{
		MakeCurrent();
		_context = GRContext.CreateGl(GRGlInterface.CreateGles(EGL.GetProcAddress));
		if (_context == null)
		{
			return;
		}

		// KRITIKUS FIX #2: Resource cache limit
		// Megelőzi a memória túlcsordulást sok kép esetén
		_context.SetResourceCacheLimit(1024 * 1024 * 64); // 64MB
	}

	bool RenderHandler(GLArea sender, RenderSignalArgs args)
	{
		MakeCurrent();

		if (_context == null)
			return false;

		var width = GetAllocatedWidth();
		var height = GetAllocatedHeight();

		if (width <= 0 || height <= 0)
			return false;

		// KRITIKUS FIX #3: Framebuffer ID tracking
		// Query current framebuffer ID
		int[] fbo = new int[1];
		GL.GetIntegerv(GL.GL_FRAMEBUFFER_BINDING, fbo);
		uint currentFboId = (uint)fbo[0];

		// Ha az FBO ID megváltozott, újra kell építeni a surface-t
		// Ez történik ListView recycling során
		if (_surface == null ||
			_renderTarget == null ||
			_surface.Canvas.DeviceClipBounds.Width != width ||
			_surface.Canvas.DeviceClipBounds.Height != height ||
			_lastFramebufferId != currentFboId)
		{
			_surface?.Dispose();
			_renderTarget?.Dispose();

			int[] stencil = new int[1];
			GL.GetIntegerv(GL.GL_STENCIL_BITS, stencil);

			int[] samples = new int[1];
			GL.GetIntegerv(GL.GL_SAMPLES, samples);

			var maxSamples = _context.GetMaxSurfaceSampleCount(ColorType);
			if (samples[0] > maxSamples)
				samples[0] = maxSamples;

			var framebuffer = new GRGlFramebufferInfo(currentFboId, ColorType.ToGlSizedFormat());
			_renderTarget = new GRBackendRenderTarget(width, height, samples[0], stencil[0], framebuffer);
			_surface = SKSurface.Create(_context, _renderTarget, SurfaceOrigin, ColorType);

			if (_surface == null)
				return false;

			_lastFramebufferId = currentFboId;
		}

		var canvas = _surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		using (new SKAutoCanvasRestore(canvas, true))
		{
			var surfaceArgs = new SKPaintGLSurfaceEventArgs(_surface, _renderTarget, SurfaceOrigin, ColorType);
			OnPaintSurface(surfaceArgs);
		}

		canvas.Flush();
		_context.Flush();

		// KRITIKUS FIX #4: GL.Finish()
		// Biztosítja hogy minden GPU parancs végrehajtódjon
		GL.Finish();

		// KRITIKUS FIX #5: PurgeUnlockedResources(true) vs PurgeResources()
		// Csak a régi, nem használt erőforrásokat törli, nem az aktív texture-öket
		_context.PurgeUnlockedResources(true);

		return true;
	}

	void UnrealizeHandler(object? sender, EventArgs e)
	{
		if (GetRealized())
			MakeCurrent();

		_surface?.Dispose();
		_surface = null;
		_renderTarget?.Dispose();
		_renderTarget = null;

		// KRITIKUS FIX #6: Context-et NEM dispose-oljuk unrealize-nél
		// ListView újrahasználhatja ugyanazt a context-et
		_context?.PurgeResources();
		_context?.Flush();

		_lastFramebufferId = 0;
	}

	uint _renderLoopTickId;
	void UpdateRenderLoop(bool start)
	{
		if (start)
		{
			if (_renderLoopTickId == 0)
			{
				_renderLoopTickId = AddTickCallback((widget, frameClock) =>
				{
					if (GetVisible() && GetRealized() && GetMapped())
						QueueRender();
					return GLib.Constants.SOURCE_CONTINUE;
				});
			}
		}
		else
		{
			if (_renderLoopTickId != 0)
			{
				RemoveTickCallback(_renderLoopTickId);
				_renderLoopTickId = 0;
			}
		}
	}

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
		if (_renderLoopTickId != 0)
		{
			RemoveTickCallback(_renderLoopTickId);
			_renderLoopTickId = 0;
		}

		OnRender -= RenderHandler;
		OnRealize -= RealizeHandler;
		OnUnrealize -= UnrealizeHandler;

		UnrealizeHandler(this, EventArgs.Empty);

		// Most már dispose-olhatjuk a context-et
		_context?.Dispose();
		_context = null;

		base.Dispose();
		GC.SuppressFinalize(this);
	}
}
