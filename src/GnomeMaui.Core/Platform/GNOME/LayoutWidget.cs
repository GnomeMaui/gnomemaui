namespace Microsoft.Maui.Platform;

/// <summary>
/// Base widget for MAUI layouts on GNOME platform.
/// Uses custom BaseLayoutManager for cross-platform layout delegation.
/// </summary>
public class LayoutWidget : Gtk.Widget, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable, GObject.InstanceFactory, IPlatformMeasureInvalidationController
{
	// MUST be strong reference to prevent GC from collecting BaseWidget while GTK uses CustomAllocate callbacks
	// Use C# GetHashCode() as primary key to avoid GTK IntPtr reuse issues
	private static readonly Dictionary<int, LayoutWidget> _widgetsByHash = [];
	private static readonly Dictionary<IntPtr, int> _ptrToHash = [];
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
		var hash = this.GetHashCode();

		_widgetsByHash[hash] = this;
		_ptrToHash[ptr] = hash;

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
		var hash = this.GetHashCode();

		_widgetsByHash[hash] = this;
		_ptrToHash[ptr] = hash;

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
		// Lookup hash from IntPtr, then get widget by hash              
		if (_ptrToHash.TryGetValue(handle, out var hash) &&
			_widgetsByHash.TryGetValue(hash, out var widget))
		{
			// Safety check: verify the widget's IntPtr still matches    
			// This protects against GTK IntPtr reuse                    
			if (widget.Handle.DangerousGetHandle() == handle)
			{
				return widget;
			}

			// IntPtr was reused, cleanup stale mapping                  
			_ptrToHash.Remove(handle);
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

		// Unparent all children to prevent "Finalizing ... but it still has children left" warning
		var child = GetFirstChild();
		if (child != null)
		{
			while (child != null)
			{
				var next = child.GetNextSibling();
				child.Unparent();

				// Optional: Ensure child is disposed if it's managed
				// if (child is IDisposable disposable) disposable.Dispose();

				child = next;
			}
		}

		CachedChildren.Clear();

		// Cleanup layout manager to prevent memory leak
		_layoutManager?.Dispose();

		// Remove from widget instances
		var ptr = Handle.DangerousGetHandle();
		var hash = this.GetHashCode();

		_ptrToHash.Remove(ptr);
		_widgetsByHash.Remove(hash);

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
