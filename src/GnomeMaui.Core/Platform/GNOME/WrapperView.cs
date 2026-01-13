using GObject;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using System;

namespace Microsoft.Maui.Platform;

public partial class WrapperView : Gtk.Grid
{
	SkiaGraphicsView _drawableCanvas;
	Gtk.Widget? _child;
	MauiDrawable _mauiDrawable;
	bool _disposed;

	public WrapperView()
	{
		_mauiDrawable = new MauiDrawable();
		_drawableCanvas = new SkiaGraphicsView
		{
			Drawable = _mauiDrawable,
			Vexpand = true,
			Hexpand = true,
			BackgroundColor = Colors.Transparent
		};

		Attach(_drawableCanvas, 0, 0, 1, 1);
	}

	public Gtk.Widget? Child
	{
		get => _child;
		set
		{
			if (_child == value)
				return;

			if (_child is not null)
			{
				Remove(_child);
			}

			_child = value;

			if (_child is not null)
			{
				Attach(_child, 0, 0, 1, 1);
			}
		}
	}

	bool NeedToUpdateCanvas =>
		_mauiDrawable.Background != null ||
		_mauiDrawable.Shape != null ||
		_mauiDrawable.Border != null ||
		_mauiDrawable.Shadow != null;

	public void UpdateBackground(Paint? paint)
	{
		_mauiDrawable.Background = paint;
		UpdateDrawableCanvas();
	}

	public void UpdateShape(IShape? shape)
	{
		_mauiDrawable.Shape = shape;
		UpdateDrawableCanvas();
	}

	public void UpdateBorder(IBorderStroke? border)
	{
		Border = border;
	}

	partial void ShadowChanged()
	{
		_mauiDrawable.Shadow = Shadow;
		UpdateDrawableCanvas(geometryUpdate: true);
	}

	partial void ClipChanged()
	{
		_mauiDrawable.Clip = Clip;
		UpdateDrawableCanvas();
	}

	partial void BorderChanged()
	{
		_mauiDrawable.Border = Border;
		UpdateShape(Border?.Shape);
		UpdateDrawableCanvas(Border != null);
	}

	void UpdateDrawableCanvas(bool geometryUpdate = false)
	{
		if (NeedToUpdateCanvas)
		{
			if (geometryUpdate)
				UpdateDrawableCanvasGeometry();

			_drawableCanvas.QueueDraw();
		}
	}

	void UpdateDrawableCanvasGeometry()
	{
		var width = GetAllocatedWidth();
		var height = GetAllocatedHeight();

		if (width <= 0 || height <= 0)
			return;

		var bounds = new Rect(0, 0, width, height);

		if (Shadow != null)
		{
			var shadowThickness = GetShadowMargin(Shadow);
			_mauiDrawable.ShadowThickness = shadowThickness;
			// Expand bounds to accommodate shadow
			bounds = new Rect(
				-shadowThickness.Left,
				-shadowThickness.Top,
				width + shadowThickness.HorizontalThickness,
				height + shadowThickness.VerticalThickness
			);
		}

		_drawableCanvas.SetSizeRequest((int)bounds.Width, (int)bounds.Height);
	}

	static Thickness GetShadowMargin(IShadow shadow)
	{
		double left = 0;
		double top = 0;
		double right = 0;
		double bottom = 0;

		var offsetX = shadow.Offset.X;
		var offsetY = shadow.Offset.Y;
		var blurRadius = (double)shadow.Radius;
		var spreadSize = blurRadius * 3;
		var spreadLeft = offsetX - spreadSize;
		var spreadRight = offsetX + spreadSize;
		var spreadTop = offsetY - spreadSize;
		var spreadBottom = offsetY + spreadSize;

		if (left > spreadLeft)
			left = spreadLeft;
		if (top > spreadTop)
			top = spreadTop;
		if (right < spreadRight)
			right = spreadRight;
		if (bottom < spreadBottom)
			bottom = spreadBottom;

		return new Thickness(-left, -top, right, bottom);
	}

	public override void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		_drawableCanvas.Dispose();

		if (_child != null)
		{
			Remove(_child);
			_child = null;
		}

		base.Dispose();
	}
}
