using GObject;
using SkiaSharp.Views.Maui;

namespace Microsoft.Maui.Graphics.Platform;

public class SkiaGLGraphicsView : SKGLArea, IDisposable
{
	RectF _dirtyRect = default;
	SignalHandler<Gtk.GLArea, ResizeSignalArgs>? _resizeHandler;

	public SkiaGLGraphicsView()
	{
		Renderer = CreateDefaultRenderer();

		_resizeHandler = OnResizeHandler;

		OnResize += _resizeHandler;
	}

	void OnResizeHandler(Gtk.GLArea area, ResizeSignalArgs args)
	{
		_dirtyRect.Width = args.Width;
		_dirtyRect.Height = args.Height;
		Renderer?.SizeChanged(args.Width, args.Height);
		Resize(args.Width, args.Height);
	}

	protected virtual void Resize(int width, int height)
	{
	}

	public ISkiaGLGraphicsRenderer Renderer
	{
		get;
		set
		{
			if (field != null)
			{
				field.Drawable = default!;
				field.GraphicsView = default!;
				field.Dispose();
			}

			field = value ?? CreateDefaultRenderer();
			field.GraphicsView = this;
			field.Drawable = Drawable;
			field.SizeChanged((int)CanvasSize.Width, (int)CanvasSize.Height);
		}
	} = default!;

	static SkiaGLDirectRenderer CreateDefaultRenderer() => new();

	public Color BackgroundColor
	{
		get => Renderer.BackgroundColor;
		set => Renderer.BackgroundColor = value;
	}

	public IDrawable Drawable
	{
		get;
		set
		{
			field = value;
			Renderer.Drawable = field;
		}
	} = default!;

	protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
	{
		Renderer?.Draw(e.Surface.Canvas, _dirtyRect);
	}

	public override void Dispose()
	{
		Dispose(true);
		base.Dispose();
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _resizeHandler is not null)
		{
			OnResize -= _resizeHandler;
			_resizeHandler = null;
		}
	}
}
