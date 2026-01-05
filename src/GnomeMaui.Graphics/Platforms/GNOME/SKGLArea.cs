using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Microsoft.Maui.Graphics.Platform;

/// <summary>
/// A GTK OpenGL area for SkiaSharp rendering.
/// </summary>
public class SKGLArea : GLArea, IDisposable
{
	const int GL_FRAMEBUFFER_BINDING = 0x8CA6;
	const int GL_STENCIL_BITS = 0x0D57;
	const int GL_SAMPLES = 0x80A9;
	const SKColorType ColorType = SKColorType.Rgba8888;
	const GRSurfaceOrigin SurfaceOrigin = GRSurfaceOrigin.BottomLeft;
	bool ignorePixelScaling;
	bool enableRenderLoop;

	GRContext? _context;
	GRBackendRenderTarget? _renderTarget;
	SKSurface? _surface;

	public SKGLArea() : base()
	{
		Vexpand = true;
		Hexpand = true;

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

	/// <summary>
	/// Occurs when the GL area needs to be repainted.
	/// </summary>
	public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

	/// <summary>
	/// Gets the size of the canvas.
	/// </summary>
	public SKSize CanvasSize => new(GetAllocatedWidth(), GetAllocatedHeight());

	public GRContext GRContext => _context!;


	/// <summary>
	/// Invalidates the GL area, causing it to be redrawn.
	/// </summary>
	public void Invalidate() => QueueRender();

	/// <summary>
	/// Raises the PaintSurface event.
	/// </summary>
	/// <param name="e">The <see cref="SKPaintGLSurfaceEventArgs"/> instance containing the event data.</param>
	protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e) => PaintSurface?.Invoke(this, e);

	void RealizeHandler(object? sender, EventArgs e)
	{
		MakeCurrent();
		_context = GRContext.CreateGl(GRGlInterface.CreateGles(EGL.GetProcAddress));
		if (_context == null)
		{
			Console.Error.WriteLine("[SKGLArea] Failed to create GRContext");
		}
	}

	bool RenderHandler(GLArea sender, RenderSignalArgs args)
	{
		if (_context == null)
		{
			return false;
		}

		var width = GetAllocatedWidth();
		var height = GetAllocatedHeight();

		if (width <= 0 || height <= 0)
		{
			return false;
		}

		if (_surface == null || _surface.Canvas.DeviceClipBounds.Width != width || _surface.Canvas.DeviceClipBounds.Height != height)
		{
			_surface?.Dispose();
			_renderTarget?.Dispose();

			// Query framebuffer ID
			int[] fbo = new int[1];
			GL.GetIntegerv(GL_FRAMEBUFFER_BINDING, fbo);

			// Query stencil bits
			int[] stencil = new int[1];
			GL.GetIntegerv(GL_STENCIL_BITS, stencil);

			// Query samples and limit to max supported
			int[] samples = new int[1];
			GL.GetIntegerv(GL_SAMPLES, samples);

			var maxSamples = _context.GetMaxSurfaceSampleCount(ColorType);
			if (samples[0] > maxSamples)
			{
				samples[0] = maxSamples;
			}

			var framebuffer = new GRGlFramebufferInfo((uint)fbo[0], ColorType.ToGlSizedFormat());
			_renderTarget = new GRBackendRenderTarget(width, height, samples[0], stencil[0], framebuffer);
			_surface = SKSurface.Create(_context, _renderTarget, SurfaceOrigin, ColorType);

			if (_surface == null)
			{
				return false;
			}
		}

		if (_renderTarget == null)
		{
			return false;
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
		_context.PurgeResources();

		return true;
	}

	void UnrealizeHandler(object? sender, EventArgs e)
	{
		MakeCurrent();

		_surface?.Dispose();
		_surface = null;
		_renderTarget?.Dispose();
		_renderTarget = null;
		_context?.Dispose();
		_context = null;
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
		if (_renderLoopTickId != 0)
		{
			RemoveTickCallback(_renderLoopTickId);
			_renderLoopTickId = 0;
		}

		OnRender -= RenderHandler;
		OnRealize -= RealizeHandler;
		OnUnrealize -= UnrealizeHandler;
		UnrealizeHandler(this, EventArgs.Empty);
		base.Dispose();
		//GC.SuppressFinalize(this);
	}
}
