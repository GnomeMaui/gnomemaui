using GnomeMaui.CSS;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

public static partial class ViewExtensions
{
	public static void UpdateIsEnabled(this Gtk.Widget platformView, IView view)
	{
		platformView.Sensitive = view.IsEnabled;
	}

	public static void Focus(this Gtk.Widget platformView, FocusRequest request) { }

	public static void Unfocus(this Gtk.Widget platformView, IView view) { }

	public static void UpdateVisibility(this Gtk.Widget platformView, IView view)
	{
		platformView.Visible = view.Visibility == Visibility.Visible;
	}

	internal static void UpdatePlatformViewBackground(this Gtk.Widget platformView, IView view)
	{
	}

	public static Task UpdateBackgroundImageSourceAsync(this Gtk.Widget platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		=> Task.CompletedTask;

	public static void UpdateBackground(this Gtk.Widget platformView, IView view)
	{
		if (view.Background is not Graphics.SolidPaint solidPaint)
			return;

		if (solidPaint.Color is null)
			return;

		var color = solidPaint.Color;
		var instanceClass = $"maui-{platformView.Handle.DangerousGetHandle()}";
		var cssNodeName = platformView.CssName;
		var instanceSelector = string.IsNullOrEmpty(cssNodeName) ? $".{instanceClass}" : $"{cssNodeName}.{instanceClass}";
		var isElementSelector = !string.IsNullOrEmpty(cssNodeName);

		var css = $"{instanceSelector} {{ background-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); }}";

		if (isElementSelector)
			CssCache.AddElementSelector(css);
		else
			CssCache.AddClassSelector(css);

		platformView.AddCssClass(instanceClass);
	}

	public static void UpdateClipsToBounds(this Gtk.Widget platformView, IView view) { }

	public static void UpdateAutomationId(this Gtk.Widget platformView, IView view) { }

	public static void UpdateClip(this Gtk.Widget platformView, IView view)
	{
		// TODO: Implement CSS clip-path for complex clipping
		// GTK4 supports clip-path CSS property
		// For now, ClipsToBounds handles the basic case
	}

	public static void UpdateShadow(this Gtk.Widget platformView, IView view)
	{
		if (view.Shadow is null)
			return;

		var shadow = view.Shadow;
		var instanceClass = $"maui-{platformView.Handle.DangerousGetHandle()}";
		var cssNodeName = platformView.CssName;
		var instanceSelector = string.IsNullOrEmpty(cssNodeName) ? $".{instanceClass}" : $"{cssNodeName}.{instanceClass}";
		var isElementSelector = !string.IsNullOrEmpty(cssNodeName);

		// box-shadow: offset-x offset-y blur-radius spread-radius color
		var offsetX = shadow.Offset.X;
		var offsetY = shadow.Offset.Y;
		var blur = shadow.Radius;
		var color = shadow.Paint is Graphics.SolidPaint solidPaint && solidPaint.Color != null
			? solidPaint.Color
			: Graphics.Colors.Black;
		var opacity = shadow.Opacity;

		var css = $"{instanceSelector} {{ box-shadow: {offsetX}px {offsetY}px {blur}px rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {opacity}); }}";

		if (isElementSelector)
			CssCache.AddElementSelector(css);
		else
			CssCache.AddClassSelector(css);

		platformView.AddCssClass(instanceClass);
	}

	public static void UpdateBorder(this Gtk.Widget platformView, IView view)
	{
		if (view is not IBorderStroke border)
			return;

		var instanceClass = $"maui-{platformView.Handle.DangerousGetHandle()}";
		var cssNodeName = platformView.CssName;
		var instanceSelector = string.IsNullOrEmpty(cssNodeName) ? $".{instanceClass}" : $"{cssNodeName}.{instanceClass}";
		var isElementSelector = !string.IsNullOrEmpty(cssNodeName);
		var cssRules = new System.Text.StringBuilder();

		// Border color
		if (border.Stroke is Graphics.SolidPaint strokePaint && strokePaint.Color != null)
		{
			var color = strokePaint.Color;
			cssRules.Append($"border-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); ");
		}

		// Border width and style
		if (border.StrokeThickness > 0)
		{
			cssRules.Append($"border-width: {border.StrokeThickness}px; ");
			cssRules.Append("border-style: solid; ");
		}

		// TODO: Handle border.Shape for rounded corners
		// Border.StrokeShape can be RoundRectangle with CornerRadius

		if (cssRules.Length > 0)
		{
			var css = $"{instanceSelector} {{ {cssRules} }}";

			if (isElementSelector)
				CssCache.AddElementSelector(css);
			else
				CssCache.AddClassSelector(css);

			platformView.AddCssClass(instanceClass);
		}
	}

	public static void UpdateOpacity(this Gtk.Widget platformView, IView view)
	{
		platformView.UpdateOpacity(view.Opacity);
	}

	internal static void UpdateOpacity(this Gtk.Widget platformView, double opacity)
	{
		platformView.Opacity = opacity;
	}

	public static void UpdateSemantics(this Gtk.Widget platformView, IView view) { }

	public static void UpdateFlowDirection(this Gtk.Widget platformView, IView view) { }

	public static void UpdateTranslationX(this Gtk.Widget platformView, IView view) { }

	public static void UpdateTranslationY(this Gtk.Widget platformView, IView view) { }

	public static void UpdateScale(this Gtk.Widget platformView, IView view) { }

	public static void UpdateRotation(this Gtk.Widget platformView, IView view) { }

	public static void UpdateRotationX(this Gtk.Widget platformView, IView view) { }

	public static void UpdateRotationY(this Gtk.Widget platformView, IView view) { }

	public static void UpdateAnchorX(this Gtk.Widget platformView, IView view) { }

	public static void UpdateAnchorY(this Gtk.Widget platformView, IView view) { }

	public static void InvalidateMeasure(this Gtk.Widget platformView, IView view)
	{
		platformView.InvalidateMeasure();
	}

	internal static void InvalidateMeasure(this Gtk.Widget platformView)
	{
		// Invalidate the current widget and check if we should propagate
		var propagate = true;

		if (platformView is IPlatformMeasureInvalidationController controller)
		{
			propagate = controller.InvalidateMeasure(isPropagating: false);
		}
		else
		{
			platformView.QueueResize();
		}

		// Propagate to ancestors if needed
		if (propagate)
		{
			platformView.InvalidateAncestorsMeasures();
		}
	}

	internal static void InvalidateAncestorsMeasures(this Gtk.Widget child)
	{
		while (true)
		{
			var parent = child.GetParent();
			if (parent is null)
			{
				return;
			}

			// Invalidate the parent and check if we should continue propagating
			var propagate = true;

			if (parent is IPlatformMeasureInvalidationController controller)
			{
				propagate = controller.InvalidateMeasure(isPropagating: true);
			}
			else
			{
				parent.QueueResize();
			}

			if (!propagate)
			{
				// We've been asked to stop propagation
				return;
			}

			child = parent;
		}
	}

	public static void UpdateWidth(this Gtk.Widget platformView, IView view)
	{
		if (Primitives.Dimension.IsExplicitSet(view.Width))
			platformView.WidthRequest = (int)view.Width;
		else
			platformView.WidthRequest = -1;
	}

	public static void UpdateHeight(this Gtk.Widget platformView, IView view)
	{
		if (Primitives.Dimension.IsExplicitSet(view.Height))
			platformView.HeightRequest = (int)view.Height;
		else
			platformView.HeightRequest = -1;
	}

	public static void UpdateMinimumHeight(this Gtk.Widget platformView, IView view)
	{

	}

	public static void UpdateMaximumHeight(this Gtk.Widget platformView, IView view)
	{
	}

	public static void UpdateMinimumWidth(this Gtk.Widget platformView, IView view)
	{
	}

	public static void UpdateMaximumWidth(this Gtk.Widget platformView, IView view)
	{
	}

	internal static Graphics.Rect GetPlatformViewBounds(this IView view) => view.Frame;

	internal static System.Numerics.Matrix4x4 GetViewTransform(this IView view) => new System.Numerics.Matrix4x4();

	// Used by MAUI XAML Hot Reload.
	// Consult XET if updating!
	internal static Graphics.Rect GetBoundingBox(this IView view) => view.Frame;

	internal static Gtk.Widget? GetParent(this Gtk.Widget? view)
	{
		return view?.GetParent();
	}

	internal static IWindow? GetHostedWindow(this IView? view)
		=> null;

	public static void UpdateInputTransparent(this Gtk.Widget nativeView, IViewHandler handler, IView view) { }

	public static void UpdateToolTip(this Gtk.Widget? platformView, ToolTip? tooltip)
	{
		if (platformView is null)
			return;

		if (tooltip?.Content is string text)
			platformView.TooltipText = text;
		else
			platformView.TooltipText = null;
	}

	public static bool IsLoaded(this Gtk.Widget? platformView)
	{
		if (platformView is not { })
		{
			return false;
		}

		return platformView.GetRealized();
	}

	internal static IDisposable OnLoaded(this Gtk.Widget platformView, Action action)
	{
		if (platformView.IsLoaded())
		{
			action();
			return new ActionDisposable(() => { });
		}

		GObject.SignalHandler<Gtk.Widget>? routedEventHandler = null;

		ActionDisposable? disposable = new ActionDisposable(() =>
		{
			if (routedEventHandler != null)
			{
				platformView?.OnRealize -= routedEventHandler;
			}
		});

		routedEventHandler = (_, __) =>
		{
			disposable?.Dispose();
			disposable = null;
			action();
		};

		platformView?.OnRealize += routedEventHandler;

		return disposable;
	}

	internal static IDisposable OnUnloaded(this Gtk.Widget platformView, Action action)
	{
		if (!platformView.IsLoaded())
		{
			action();
			return new ActionDisposable(() => { });
		}

		GObject.SignalHandler<Gtk.Widget>? routedEventHandler = null;


		ActionDisposable? disposable = new ActionDisposable(() =>
		{
			if (routedEventHandler != null)
			{
				platformView.OnUnrealize -= routedEventHandler;
			}
		});

		routedEventHandler = (_, __) =>
		{
			disposable?.Dispose();
			disposable = null;
			action();
		};

		platformView.OnUnrealize += routedEventHandler;

		return disposable;
	}

	internal static T? GetChildAt<T>(this Gtk.Widget platformView, int index) where T : Gtk.Widget
	{
		// if (platformView is Gtk.Container container && container.Children.Length < index)
		// {
		// 	return (T)container.Children[index];
		// }

		// if (platformView is Gtk.Bin bin && index == 0 && bin.Child is T child)
		// {
		// 	return child;
		// }

		return default;
	}
}
