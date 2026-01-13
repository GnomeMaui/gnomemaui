namespace Microsoft.Maui.Handlers;

public partial class LayoutHandler : ViewHandler<ILayout, LayoutWidget>
{
	public void Add(IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
		PlatformView.CachedChildren.Insert(targetIndex, (Gtk.Widget)child.ToPlatform(MauiContext));
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		PlatformView.CrossPlatformLayout = VirtualView;

		var children = PlatformView.CachedChildren;

		// KRITIKUS: Először Unparent MINDEN régi gyereket, mielőtt a listát töröljük
		foreach (var oldChild in children)
		{
			if (oldChild.GetParent() != null)
			{
				oldChild.Unparent();
			}
		}

		children.Clear();

		foreach (var child in VirtualView.OrderByZIndex())
		{
			// Prefer an already-created handler/platform view if possible to avoid recreating native widgets
			Gtk.Widget? platformChild;
			if (child.Handler?.PlatformView is Gtk.Widget existing)
			{
				platformChild = existing;
			}
			else
			{
				platformChild = child.ToPlatform(MauiContext) as Gtk.Widget;
			}

			if (platformChild == null)
			{
				continue;
			}

			// Avoid duplicates
			if (children.Contains(platformChild))
			{
				continue;
			}

			// Ha a widget-nek már van parent-je, leválasztjuk (RÉGI widget újrahasználat esetén)
			if (platformChild.GetParent() != null)
			{
				platformChild.Unparent();
			}

			children.Add(platformChild);
		}

		// Do not parent native children here; parenting is deferred to the platform container's map/realize handlers.
		//PlatformView.Show();
	}

	protected override LayoutWidget CreatePlatformView()
	{
		if (VirtualView == null)
		{
			throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
		}

		var view = new LayoutWidget
		{
			CrossPlatformLayout = VirtualView
		};

		return view;
	}

	protected override void DisconnectHandler(LayoutWidget platformView)
	{
		// KRITIKUS: Unparent MINDEN gyereket mielőtt a handler disconnect-olódik
		foreach (var child in platformView.CachedChildren)
		{
			if (child.GetParent() != null)
			{
				child.Unparent();
			}
		}

		// If we're being disconnected from the xplat element, then we should no longer be managing its children
		platformView.CachedChildren.Clear();
		base.DisconnectHandler(platformView);
	}

	public void Remove(IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

		if (child?.Handler?.PlatformView is Gtk.Widget view)
		{
			// Unparent the widget to prevent GTK warnings about "has a parent during dispose"
			view.Unparent();
			PlatformView.CachedChildren.Remove(view);
		}
	}

	public void Clear()
	{
		if (PlatformView == null)
			return;

		// Unparent all children before clearing the list
		foreach (var child in PlatformView.CachedChildren)
		{
			child.Unparent();
		}

		PlatformView.CachedChildren.Clear();
	}

	public void Insert(int index, IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
		var platformChild = (Gtk.Widget)child.ToPlatform(MauiContext);

		// Ha a widget-nek már van parent-je, leválasztjuk (RÉGI widget újrahasználat esetén)
		if (platformChild.GetParent() != null)
		{
			platformChild.Unparent();
		}

		PlatformView.CachedChildren.Insert(targetIndex, platformChild);
	}

	public void Update(int index, IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		var platformChild = (Gtk.Widget)child.ToPlatform(MauiContext);

		// Ha a widget-nek már van parent-je, leválasztjuk (RÉGI widget újrahasználat esetén)
		if (platformChild.GetParent() != null)
		{
			platformChild.Unparent();
		}

		PlatformView.CachedChildren[index] = platformChild;
		EnsureZIndexOrder(child);
	}

	public void UpdateZIndex(IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		EnsureZIndexOrder(child);
	}

	void EnsureZIndexOrder(IView child)
	{
		if (PlatformView.CachedChildren.Count == 0)
		{
			return;
		}

		var children = PlatformView.CachedChildren;
		var currentIndex = children.IndexOf(child.ToPlatform(MauiContext!));

		if (currentIndex == -1)
		{
			return;
		}

		var targetIndex = VirtualView.GetLayoutHandlerIndex(child);

		if (currentIndex != targetIndex)
		{
			var item = children[currentIndex];
			children.RemoveAt(currentIndex);
			children.Insert(targetIndex, item);
		}
	}

	public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
	{
		handler.PlatformView?.UpdatePlatformViewBackground(layout);
	}

	public static partial void MapInputTransparent(ILayoutHandler handler, ILayout layout)
	{
		handler.PlatformView?.UpdatePlatformViewBackground(layout);
	}
}
