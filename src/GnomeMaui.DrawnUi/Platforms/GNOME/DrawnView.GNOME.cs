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
			// #if DEBUG
			// 			Console.Out.WriteLine("[DrawnView][IsElementVisibleInParentChain] element is null");
			// #endif
			return false;
		}

		// Quick check: only verify visibility
		if (!element.Visible)
		{
			// #if DEBUG
			// 			Console.Out.WriteLine($"[DrawnView][IsElementVisibleInParentChain] element not visible");
			// #endif
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
					// Window hasn't been laid out yet, assume visible
					// #if DEBUG
					// 					if (_frameCount <= 10)
					// 					{
					// 						Console.Out.WriteLine($"[DrawnView][IsElementVisibleInParentChain] Window not laid out yet, assuming visible");
					// 					}
					// #endif
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
				// #if DEBUG
				// 				Console.Out.WriteLine(new StringBuilder()
				// 					.AppendLine($"[DrawnView][IsElementVisibleInParentChain] Parent not visible at depth {depth}")
				// 					.AppendLine($" - Visible: {current.Visible}")
				// 					.ToString());
				// #endif
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
						// #if DEBUG
						// 						Console.Out.WriteLine($"[DrawnView][IsElementVisibleInParentChain] TranslateCoordinates failed at depth {depth}");
						// #endif
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
						// #if DEBUG
						// 						Console.Out.WriteLine(new StringBuilder()
						// 							.AppendLine($"[DrawnView][IsElementVisibleInParentChain] ScrolledWindow viewport check failed at depth {depth}")
						// 							.AppendLine($" - elementX: {elementX}, elementY: {elementY}")
						// 							.AppendLine($" - viewportX: {viewportX}, viewportY: {viewportY}")
						// 							.AppendLine($" - viewportWidth: {viewportWidth}, viewportHeight: {viewportHeight}")
						// 							.ToString());
						// #endif
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
		if (Handler?.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
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
		// #if DEBUG
		// 		Console.Out.WriteLine(new StringBuilder()
		// 			.AppendLine($"[DrawnView][{source}]")
		// 			.AppendLine($" - Width: {width}")
		// 			.AppendLine($" - Height: {height}")
		// 			.ToString());
		// #endif

		float scale = RenderingScale;
		if (scale <= 0)
		{
			// fall back to 1 if RenderingScale is not yet set
			scale = 1f;
		}

		var widthPts = (float)(width / (double)scale);
		var heightPts = (float)(height / (double)scale);

		// Console.WriteLine($"[{source}] Calling Measure with {widthPts}x{heightPts} (units) at scale={scale}.");
		ScaledSize measured = Measure(widthPts, heightPts);

		// Console.WriteLine($"[{source}] Measured Units={measured.Units.Width}x{measured.Units.Height}, Pixels={measured.Pixels.Width}x{measured.Pixels.Height}");

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

		// Console.WriteLine($"[{source}] Arrange called. Destination={dest}, WidthRequest(units)={widthReq}, HeightRequest(units)={heightReq}");

		// CRITICAL: Set the Canvas Width and Height properties manually
		// SetValueFromRenderer doesn't work, use direct property assignment
		((IView)this).Frame = new Rect(0, 0, widthPts, heightPts);
		WidthRequest = widthPts;
		HeightRequest = heightPts;

		// #if DEBUG
		// 		Console.Out.WriteLine(new StringBuilder()
		// 			.AppendLine($"[{source}] AFTER SetValueFromRenderer:")
		// 			.ToString());

		// 		// Print FULL MAUI Visual Tree
		// 		var current = this as Element;
		// 		int depth = 0;
		// 		var treeBuilder = new StringBuilder();
		// 		treeBuilder.AppendLine("  === MAUI VISUAL TREE ===");

		// 		while (current != null)
		// 		{
		// 			var indent = new string(' ', depth * 2);
		// 			treeBuilder.AppendLine($"{indent}[{depth}] {current.GetType().Name}");

		// 			if (current is VisualElement ve)
		// 			{
		// 				treeBuilder.AppendLine($"{indent}    Width={ve.Width} Height={ve.Height}");
		// 				treeBuilder.AppendLine($"{indent}    WidthRequest={ve.WidthRequest} HeightRequest={ve.HeightRequest}");
		// 				treeBuilder.AppendLine($"{indent}    Bounds={ve.Bounds}");
		// 				treeBuilder.AppendLine($"{indent}    DesiredSize={ve.DesiredSize}");
		// 				treeBuilder.AppendLine($"{indent}    IsVisible={ve.IsVisible}");

		// 				if (ve.Handler != null)
		// 				{
		// 					treeBuilder.AppendLine($"{indent}    Handler={ve.Handler.GetType().Name}");
		// 					treeBuilder.AppendLine($"{indent}    PlatformView={ve.Handler.PlatformView?.GetType().Name ?? "null"}");
		// 				}

		// 				// Print children
		// 				if (ve is Layout layout)
		// 				{
		// 					treeBuilder.AppendLine($"{indent}    Children.Count={layout.Count}");
		// 					int childIdx = 0;
		// 					foreach (var child in layout)
		// 					{
		// 						if (child is VisualElement childVE)
		// 						{
		// 							treeBuilder.AppendLine($"{indent}      [{childIdx}] {childVE.GetType().Name} W={childVE.Width} H={childVE.Height} Bounds={childVE.Bounds}");
		// 						}
		// 						childIdx++;
		// 					}
		// 				}
		// 				else if (ve is ContentView contentView && contentView.Content != null)
		// 				{
		// 					treeBuilder.AppendLine($"{indent}    Content: {contentView.Content.GetType().Name}");
		// 				}
		// 			}

		// 			current = current.Parent;
		// 			depth++;

		// 			if (depth > 10) // Safety limit
		// 			{
		// 				treeBuilder.AppendLine($"{indent}... (depth limit reached)");
		// 				break;
		// 			}
		// 		}

		// 		treeBuilder.AppendLine("  === END MAUI TREE ===");
		// 		Console.Out.WriteLine(treeBuilder.ToString());

		// 		// Print Canvas.Content (DrawnUI tree)
		// 		var canvas = this as DrawnUi.Views.Canvas;
		// 		var content = canvas?.Content as DrawnUi.Draw.SkiaControl;
		// 		if (content != null)
		// 		{
		// 			var drawnTreeBuilder = new StringBuilder();
		// 			drawnTreeBuilder.AppendLine("  === DRAWNUI TREE ===");
		// 			PrintDrawnUITree(content, 0, drawnTreeBuilder);
		// 			drawnTreeBuilder.AppendLine("  === END DRAWNUI TREE ===");
		// 			Console.Out.WriteLine(drawnTreeBuilder.ToString());
		// 		}
		// #endif

		NeedCheckParentVisibility = true;
	}

	// private void PrintDrawnUITree(DrawnUi.Draw.SkiaControl control, int depth, StringBuilder builder)
	// {
	// 	var indent = new string(' ', depth * 2);
	// 	builder.AppendLine($"{indent}[{depth}] {control.GetType().Name}");
	// 	builder.AppendLine($"{indent}    Width={control.Width} Height={control.Height}");
	// 	builder.AppendLine($"{indent}    X={control.X} Y={control.Y}");
	// 	builder.AppendLine($"{indent}    MeasuredSize={control.MeasuredSize.Pixels.Width}x{control.MeasuredSize.Pixels.Height}");
	// 	builder.AppendLine($"{indent}    DrawingRect={control.DrawingRect}");
	// 	builder.AppendLine($"{indent}    Destination={control.Destination}");
	// 	builder.AppendLine($"{indent}    IsVisible={control.IsVisible}");

	// 	if (control is DrawnUi.Draw.SkiaLayout layout && layout.Views != null)
	// 	{
	// 		builder.AppendLine($"{indent}    Views.Count={layout.Views.Count}");
	// 		for (int i = 0; i < layout.Views.Count; i++)
	// 		{
	// 			PrintDrawnUITree(layout.Views[i], depth + 1, builder);
	// 		}
	// 	}
	// }

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

		// #if DEBUG
		// 		if (_frameCount <= 3)
		// 		{
		// 			var canvas = this as DrawnUi.Views.Canvas;
		// 			var content = canvas?.Content as DrawnUi.Draw.SkiaControl;

		// 			Console.Out.WriteLine(new StringBuilder()
		// 				.AppendLine($"[DrawnView][OnFrame] #{_frameCount} CheckCanDraw={CheckCanDraw()} CanDraw={CanDraw}")
		// 				.AppendLine($"  Canvas: Width={this.Width} Height={this.Height} WidthRequest={this.WidthRequest} HeightRequest={this.HeightRequest}")
		// 				.ToString());

		// 			if (content != null)
		// 			{
		// 				Console.Out.WriteLine($"  Content: {content.GetType().Name} Visible={content.IsVisible} Width={content.Width} Height={content.Height} MeasuredSize={content.MeasuredSize.Pixels.Width}x{content.MeasuredSize.Pixels.Height}");

		// 				if (content is DrawnUi.Draw.SkiaLayout layout && layout.Views != null)
		// 				{
		// 					Console.Out.WriteLine($"  Layout Children={layout.Views.Count}");
		// 					foreach (var child in layout.Views)
		// 					{
		// 						Console.Out.WriteLine($"    Child: {child.GetType().Name} Visible={child.IsVisible} Width={child.Width} Height={child.Height}");

		// 						if (_frameCount == 2 && child is DrawnUi.Draw.SkiaLabel label)
		// 						{
		// 							Console.Out.WriteLine(new StringBuilder()
		// 								.AppendLine($"    SkiaLabel Details:")
		// 								.AppendLine($"      Text: '{label.Text}'")
		// 								.AppendLine($"      FontSize: {label.FontSize}")
		// 								.AppendLine($"      TextColor: {label.TextColor}")
		// 								.AppendLine($"      X: {label.X}, Y: {label.Y}")
		// 								.AppendLine($"      DrawingRect: {label.DrawingRect}")
		// 								.AppendLine($"      Destination: {label.Destination}")
		// 								.AppendLine($"      IsVisible: {label.IsVisible}")
		// 								.AppendLine($"      Opacity: {label.Opacity}")
		// 								.ToString());
		// 						}
		// 					}
		// 				}
		// 			}
		// 		}
		// #endif

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

		// #if DEBUG
		// 		if (_frameCount <= 3 && !result)
		// 		{
		// 			Console.Out.WriteLine($"[DrawnView][CheckCanDraw] FALSE - IsDirty={IsDirty}");
		// 		}
		// #endif

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

