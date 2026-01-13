using Gtk;
using SkiaSharp.Views.Maui.Handlers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace DrawnUi.Views;

public partial class DrawnView
{
	private bool _wasVisible = true;
	private bool _checkVisibility;
	private DateTime _visibilityChangedTime;
	private readonly TimeSpan _visibilityCheckDelay = TimeSpan.FromSeconds(0.1);
	private bool _hadValidSize = false;
	private int _frameCount = 0;
	private bool _lastIsHiddenInViewTree = false;

	/// <summary>
	/// Check if element is visible through entire parent chain
	/// </summary>
	private bool IsElementVisibleInParentChain(Widget element)
	{
		if (element == null)
		{
			return false;
		}

		// Quick check: only verify visibility
		if (!element.Visible)
		{
			return false;
		}

		// If element has no size yet, check if GTK layout has happened
		var width = element.GetAllocatedWidth();
		var height = element.GetAllocatedHeight();

		if (width <= 0 || height <= 0)
		{
			// Find the root window and check if it has been laid out
			var root = element.GetRoot();
			if (root is Gtk.Window window)
			{
				var windowWidth = window.GetWidth();
				var windowHeight = window.GetHeight();

				if (windowWidth <= 0 || windowHeight <= 0)
				{
					return true;
				}
			}
		}

		if (width > 0 && height > 0)
		{
			_hadValidSize = true;
		}

		// Walk up parent chain
		Widget current = element;
		int depth = 0;
		while (current != null)
		{
			depth++;
			// Only check visibility, not size - GTK widgets may be 0x0 during layout
			if (!current.Visible)
			{
				return false;
			}

			// Check if parent is a scrolled window or viewport
			if (current.Parent is ScrolledWindow scrolledWindow)
			{
				// Check if element is within visible viewport
				var hadjustment = scrolledWindow.Hadjustment;
				var vadjustment = scrolledWindow.Vadjustment;

				if (hadjustment != null && vadjustment != null)
				{
					// Get element allocation relative to scrolled window
					double elementX = 0, elementY = 0;
					if (!element.TranslateCoordinates(scrolledWindow, 0, 0, out elementX, out elementY))
					{
						return false;
					}

					// Check if element is within visible area
					var viewportX = hadjustment.Value;
					var viewportY = vadjustment.Value;
					var viewportWidth = hadjustment.PageSize;
					var viewportHeight = vadjustment.PageSize;

					if (elementX + element.GetAllocatedWidth() < viewportX ||
						elementX > viewportX + viewportWidth ||
						elementY + element.GetAllocatedHeight() < viewportY ||
						elementY > viewportY + viewportHeight)
					{
						return false;
					}
				}
			}

			current = current.Parent as Widget;
		}

		// Check if we have a toplevel window
		var hasRoot = element.GetRoot() != null;
		return hasRoot;
	}

	/// <summary>
	/// To optimize rendering and not update controls that are inside storyboard that is offscreen or hidden
	/// </summary>
	public void CheckElementVisibility(VisualElement element)
	{
		NeedCheckParentVisibility = false;

		if (Handler?.PlatformView is not Widget platformWidget)
		{
			IsHiddenInViewTree = true;
			return;
		}

		IsHiddenInViewTree = !IsElementVisibleInParentChain(platformWidget);
	}

	protected virtual void InitFrameworkPlatform(bool subscribe)
	{
		// Get the actual SKDrawingArea/SKGLArea from ContentPanel.Child
		Widget? actualPlatformView = null;
		if (Handler?.PlatformView is Microsoft.Maui.Platform.ContentWidget contentPanel)
		{
			actualPlatformView = contentPanel.Child;
		}
		else
		{
			actualPlatformView = Handler?.PlatformView as Widget;
		}

		if (subscribe)
		{
			if (actualPlatformView is DrawingArea drawingArea)
			{
				drawingArea.OnResize += OnDrawingAreaResize;
				drawingArea.OnRealize += OnWidgetRealize;
				drawingArea.OnUnrealize += OnWidgetUnrealize;
			}
			else if (actualPlatformView is GLArea glArea)
			{
				glArea.OnResize += OnGLAreaResize;
				glArea.OnRealize += OnWidgetRealize;
				glArea.OnUnrealize += OnWidgetUnrealize;
			}
		}
		else
		{
			if (actualPlatformView is DrawingArea drawingArea)
			{
				drawingArea.OnResize -= OnDrawingAreaResize;
				drawingArea.OnRealize -= OnWidgetRealize;
				drawingArea.OnUnrealize -= OnWidgetUnrealize;
			}
			else if (actualPlatformView is GLArea glArea)
			{
				glArea.OnResize -= OnGLAreaResize;
				glArea.OnRealize -= OnWidgetRealize;
				glArea.OnUnrealize -= OnWidgetUnrealize;
			}
		}
	}

	private void HandleResize(int width, int height, string source)
	{
		float scale = RenderingScale;
		if (scale <= 0)
		{
			// fall back to 1 if RenderingScale is not yet set
			scale = 1f;
		}

		var widthPts = (float)(width / (double)scale);
		var heightPts = (float)(height / (double)scale);

		ScaledSize measured = Measure(widthPts, heightPts);


		// Arrange expects a pixel destination (SKRect) and width/height requests in UNITS  
		SKRect dest = new SKRect(0, 0, width, height);

		// If the measured size equals (or is very close to) the available constraint then  
		// the measurement likely used the full available area. In that case avoid passing  
		// the full-window request back into Arrange — passing -1 keeps the view in autosize
		// mode and prevents overwriting a previously centered destination.
		const float eps = 0.5f;
		float widthReq = measured.Units.Width;
		float heightReq = measured.Units.Height;

		if (Math.Abs(measured.Units.Width - widthPts) <= eps)
		{
			widthReq = -1; // keep autosize
		}

		if (Math.Abs(measured.Units.Height - heightPts) <= eps)
		{
			heightReq = -1; // keep autosize
		}

		Arrange(dest, widthReq, heightReq, scale);


		// CRITICAL: Set the Canvas Width and Height properties manually
		// SetValueFromRenderer doesn't work, use direct property assignment
		((IView)this).Frame = new Rect(0, 0, widthPts, heightPts);
		WidthRequest = widthPts;
		HeightRequest = heightPts;

		NeedCheckParentVisibility = true;
	}

	private void OnDrawingAreaResize(DrawingArea sender, DrawingArea.ResizeSignalArgs args)
	{
		HandleResize(args.Width, args.Height, nameof(OnDrawingAreaResize));
	}

	private void OnGLAreaResize(GLArea sender, GLArea.ResizeSignalArgs args)
	{
		HandleResize(args.Width, args.Height, nameof(OnGLAreaResize));
	}

	private void OnWidgetRealize(Widget sender, EventArgs args)
	{
		NeedCheckParentVisibility = true;
	}

	private void OnWidgetUnrealize(Widget sender, EventArgs args)
	{
		IsHiddenInViewTree = true;
	}

	protected virtual void OnSizeChanged()
	{
		Update();
	}

	public virtual void SetupRenderingLoop()
	{
		Super.OnFrame -= OnFrame;
		Super.OnFrame += OnFrame;
	}

	private void OnFrame(object sender, EventArgs e)
	{
		_frameCount++;

		if (CheckCanDraw())
		{
			if (NeedCheckParentVisibility)
			{
				CheckElementVisibility(this);
			}

			if (CanDraw && !IsHiddenInViewTree)
			{
				CanvasView?.Update();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void UpdatePlatform()
	{
		IsDirty = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckCanDraw()
	{
		var result = CanvasView != null
			&& Handler != null
			&& Handler.PlatformView != null
			&& IsDirty
			&& !(UpdateLocks > 0 && StopDrawingWhenUpdateIsLocked)
			&& IsVisible
			&& Super.EnableRendering;

		return result;
	}

	protected virtual void DisposePlatform()
	{
		InitFrameworkPlatform(false);
		Super.OnFrame -= OnFrame;
	}

	protected virtual void PlatformHardwareAccelerationChanged()
	{
	}

	public void ResetFocus()
	{
		if (Handler?.PlatformView is Widget widget)
		{
			var toplevel = widget.GetRoot() as Gtk.Window;
			toplevel?.SetFocus(null);
		}
	}

	protected void OnHandlerChangedInternal()
	{
		if (Handler?.PlatformView is Widget widget)
		{
			widget.CanFocus = true;
		}
	}
}

