using Microsoft.Maui.Graphics;
using System.Text;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Custom GTK LayoutManager for MAUI cross-platform layouts.
/// Uses Gtk.CustomLayout to delegate to ICrossPlatformLayout for measure/arrange.
/// </summary>
public class LayoutWidgetManager
{
	// Strong references to prevent GC from collecting delegates while GTK still uses them
	private static readonly Dictionary<IntPtr, LayoutWidgetManager> _layoutManagerInstances = [];

	private readonly Gtk.CustomLayout _customLayout;
	private ICrossPlatformLayout? _crossPlatformLayout;
	private ILayout? _layout;

	// Cache the width from vertical measurement to use in horizontal measurement
	// This is needed because GTK measures horizontal first with forSize=-1 (infinite),
	// but layouts with wrapping behavior (FlexLayout, Grid, etc.) need a constrained 
	// width to properly arrange their children
	private int _cachedWidth = -1;

	// Keep delegate references as fields to prevent GC collection
	private readonly Gtk.CustomRequestModeFunc _requestModeFunc;
	private readonly Gtk.CustomMeasureFunc _measureFunc;
	private readonly Gtk.CustomAllocateFunc _allocateFunc;

	// CRITICAL: Must keep GirCore CallHandler objects alive to prevent GC of native callbacks!
	private readonly Gtk.Internal.CustomRequestModeFuncCallHandler _requestModeHandler;
	private readonly Gtk.Internal.CustomMeasureFuncCallHandler _measureHandler;
	private readonly Gtk.Internal.CustomAllocateFuncCallHandler _allocateHandler;

	public LayoutWidgetManager()
	{
		_requestModeFunc = CustomRequestMode;
		_measureFunc = CustomMeasure;
		_allocateFunc = CustomAllocate;

		// Create CallHandlers and store them to prevent GC
		_requestModeHandler = new Gtk.Internal.CustomRequestModeFuncCallHandler(_requestModeFunc);
		_measureHandler = new Gtk.Internal.CustomMeasureFuncCallHandler(_measureFunc);
		_allocateHandler = new Gtk.Internal.CustomAllocateFuncCallHandler(_allocateFunc);

		// Use the native callbacks from the handlers
		var handle = Gtk.Internal.CustomLayout.New(
			_requestModeHandler.NativeCallback,
			_measureHandler.NativeCallback,
			_allocateHandler.NativeCallback);
		_customLayout = new Gtk.CustomLayout(new Gtk.Internal.CustomLayoutHandle(handle, true));

		// Store strong reference to prevent GC collection
		var layoutHandle = _customLayout.Handle.DangerousGetHandle();
		_layoutManagerInstances[layoutHandle] = this;
	}

	public Gtk.LayoutManager GetLayoutManager() => _customLayout;

	public void Dispose()
	{
		// Remove strong reference when layout manager is disposed
		var handle = _customLayout.Handle.DangerousGetHandle();
		_layoutManagerInstances.Remove(handle);
	}

	private Gtk.SizeRequestMode CustomRequestMode(Gtk.Widget widget)
	{
		// Most layouts need height-for-width mode because their height depends on
		// the available width (especially FlexLayout with Wrap, Grid with auto rows, etc.)
		return Gtk.SizeRequestMode.HeightForWidth;
	}

	private void CustomMeasure(
		Gtk.Widget widget,
		Gtk.Orientation orientation,
		int forSize,
		out int minimum,
		out int natural,
		out int minimumBaseline,
		out int naturalBaseline)
	{
		minimum = 0;
		natural = 0;
		minimumBaseline = -1;
		naturalBaseline = -1;

		if (_crossPlatformLayout is null)
			return;

		// GTK measures in two passes:
		// 1. Horizontal: forSize contains the height constraint (or -1 for infinite)
		//    We measure with infinite width and the provided height
		// 2. Vertical: forSize contains the width constraint (or -1 for infinite)
		//    We measure with the provided width and infinite height

		double widthConstraint;
		double heightConstraint;

		if (orientation == Gtk.Orientation.Horizontal)
		{
			// Measuring width: use cached width from previous vertical measurement if available,
			// otherwise use infinite width
			widthConstraint = _cachedWidth > 0 ? _cachedWidth : double.PositiveInfinity;
			heightConstraint = forSize < 0 ? double.PositiveInfinity : forSize;
		}
		else
		{
			// Measuring height: constrain width if provided, use infinite height
			widthConstraint = forSize < 0 ? double.PositiveInfinity : forSize;
			heightConstraint = double.PositiveInfinity;

			// Cache the width for next horizontal measurement
			if (forSize > 0)
			{
				_cachedWidth = forSize;
			}
		}

		var size = _crossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);

		if (orientation == Gtk.Orientation.Horizontal)
			natural = (int)size.Width;
		else
			natural = (int)size.Height;

		Console.Out.WriteLine(new StringBuilder()
			.AppendLine("[LayoutWidgetManager][CustomMeasure]")
			.AppendLine($" Orientation: {orientation}")
			.AppendLine($" forSize: {forSize}")
			.AppendLine($" WidthConstraint: {widthConstraint}, HeightConstraint: {heightConstraint}")
			.AppendLine($" Measured Size: {size.Width}x{size.Height}")
			.ToString());

		minimum = natural;
	}

	private void CustomAllocate(Gtk.Widget widget, int width, int height, int baseline)
	{
		if (_crossPlatformLayout == null)
			return;

		_crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, width, height));

		var baseWidget = LayoutWidget.GetInstance(widget.Handle.DangerousGetHandle());
		if (baseWidget == null)
		{
			return;
		}

		for (var child = baseWidget.GetFirstChild(); child != null; child = child.GetNextSibling())
		{
			var view = FindViewForWidget(_layout, child);
			if (view == null)
			{
				continue;
			}

			var frame = view.Frame;

			// GTK: Disable alignment to prevent gtk_widget_adjust_size_allocation from modifying position
			child.SetHalign(Gtk.Align.Fill);
			child.SetValign(Gtk.Align.Fill);

			// GTK: transform adjusts position without affecting allocated size
			var point = new Graphene.Point { X = (float)frame.X, Y = (float)frame.Y };
			var transform = Gsk.Transform.New()?.Translate(point);

			child.Allocate((int)frame.Width, (int)frame.Height, -1, transform);
		}
	}

	private static IView? FindViewForWidget(ILayout? layout, Gtk.Widget widget)
	{
		if (layout == null)
			return null;

		var widgetHandle = widget.Handle.DangerousGetHandle();

		for (int i = 0; i < layout.Count; i++)
		{
			var view = layout[i];
			if (view.ToPlatform() is Gtk.Widget platformWidget)
			{
				if (platformWidget.Handle.DangerousGetHandle() == widgetHandle)
					return view;
			}
		}

		return null;
	}

	public void SetCrossPlatformLayout(ICrossPlatformLayout layout)
	{
		_crossPlatformLayout = layout;
		_layout = layout as ILayout;
	}

	/// <summary>
	/// Clears the cached width to force a fresh measurement.
	/// Should be called when layout needs to remeasure due to content changes.
	/// </summary>
	public void InvalidateCache()
	{
		_cachedWidth = -1;
	}
}
