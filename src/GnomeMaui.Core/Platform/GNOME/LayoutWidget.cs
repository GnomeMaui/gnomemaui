namespace Microsoft.Maui.Platform;

/// <summary>
/// Base widget for MAUI layouts on GNOME platform.
/// Uses custom BaseLayoutManager for cross-platform layout delegation.
/// </summary>
public class LayoutWidget : Gtk.Widget, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, GObject.InstanceFactory, IPlatformMeasureInvalidationController
{
	// MUST be strong reference to prevent GC from collecting BaseWidget while GTK uses CustomAllocate callbacks
	private static readonly Dictionary<IntPtr, LayoutWidget> _widgetInstances = [];
	private static GObject.Type? _registeredType;
	private LayoutWidgetManager _layoutManager;

	static LayoutWidget()
	{
		RegisterType();
	}

	private static void RegisterType()
	{
		if (_registeredType != null)
			return;

		var parentType = Gtk.Widget.GetGType();
		var parentTypeInfo = GObject.Internal.TypeQueryOwnedHandle.Create();
		GObject.Internal.Functions.TypeQuery(parentType, parentTypeInfo);

		var handle = GObject.Internal.TypeInfoOwnedHandle.Create();
		handle.SetClassSize((ushort)parentTypeInfo.GetClassSize());
		handle.SetInstanceSize((ushort)parentTypeInfo.GetInstanceSize());

		var typeid = GObject.Internal.Functions.TypeRegisterStatic(
			parentType,
			GLib.Internal.NonNullableUtf8StringOwnedHandle.Create(nameof(LayoutWidget)),
			handle,
			0);

		_registeredType = new GObject.Type(typeid);

		GObject.Internal.DynamicInstanceFactory.Register(_registeredType.Value, Create);
	}

	public static object Create(IntPtr handle, bool ownsHandle)
	{
		return new LayoutWidget(new Gtk.Internal.WidgetHandle(handle, ownsHandle));
	}

	public LayoutWidget(Gtk.Internal.WidgetHandle handle) : base(handle)
	{
		var ptr = handle.DangerousGetHandle();
		_widgetInstances[ptr] = this;

		_layoutManager = new LayoutWidgetManager();
		SetLayoutManager(_layoutManager.GetLayoutManager());
		SetHexpand(true);
		SetVexpand(true);
		OnMap += BaseWidget_OnMap;
		Show();
	}

	public LayoutWidget() : base(
		new Gtk.Internal.WidgetHandle(
			GObject.Internal.Object.NewWithProperties(
				_registeredType!.Value,
				0,
				GLib.Internal.Utf8StringArraySizedOwnedHandle.Create([]),
				GObject.Internal.ValueArray2OwnedHandle.Create([])),
			true))
	{
		var ptr = Handle.DangerousGetHandle();
		_widgetInstances[ptr] = this;

		_layoutManager = new LayoutWidgetManager();
		SetLayoutManager(_layoutManager.GetLayoutManager());
		SetHexpand(true);
		SetVexpand(true);
		OnMap += BaseWidget_OnMap;

		Show();
	}

	void BaseWidget_OnMap(Gtk.Widget sender, EventArgs args)
	{
		EnsureChildrenParented();
	}

	internal static LayoutWidget? GetInstance(IntPtr handle)
	{
		if (_widgetInstances.TryGetValue(handle, out var widget))
		{
			return widget;
		}
		return null;
	}

	public List<Gtk.Widget> CachedChildren { get; } = new();

	public ICrossPlatformLayout? CrossPlatformLayout
	{
		get => _crossPlatformLayout;
		set
		{
			_crossPlatformLayout = value;
			_layoutManager.SetCrossPlatformLayout(value!);
		}
	}
	private ICrossPlatformLayout? _crossPlatformLayout;

	public IVisualTreeElement? GetElement()
	{
		return _crossPlatformLayout as IVisualTreeElement;
	}

	void EnsureChildrenParented()
	{
		foreach (var child in CachedChildren)
		{
			if (child.GetParent() == null)
				child.SetParent(this);
		}
	}

	public override void Dispose()
	{
		OnMap -= BaseWidget_OnMap;

		// Cleanup layout manager to prevent memory leak
		_layoutManager?.Dispose();

		// Remove from widget instances
		var ptr = Handle.DangerousGetHandle();
		_widgetInstances.Remove(ptr);

		base.Dispose();
	}

	/// <summary>
	/// Invalidates the measure for this layout and determines if the invalidation should propagate to ancestors.
	/// </summary>
	/// <param name="isPropagating">True if this invalidation is propagating from a descendant view</param>
	/// <returns>True to continue propagation to ancestors</returns>
	bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
	{
		// Clear the cached width to force fresh measurement
		_layoutManager?.InvalidateCache();

		// Trigger GTK layout update
		QueueResize();

		// Continue propagation to ensure all ancestor layouts are invalidated
		// This is necessary for async-loaded content like images
		return true;
	}
}
