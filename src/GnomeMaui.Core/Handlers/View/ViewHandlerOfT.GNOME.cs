using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers;

public partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
{
	Gtk.Widget? IPlatformViewHandler.PlatformView => (Gtk.Widget?)base.PlatformView;

	public override void PlatformArrange(Rect rect)
	{
		this.PlatformArrangeHandler(rect);
	}

	public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		=> this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

	protected override void SetupContainer()
	{
		if (PlatformView == null || ContainerView != null)
			return;

		var oldParent = PlatformView.GetParent();

		// Remove from current parent if exists
		PlatformView?.Unparent();

		// Create container
		ContainerView ??= new Platform.WrapperView();

		if (ContainerView is Platform.WrapperView wrapperView)
		{
			wrapperView.Child = PlatformView;
		}

		// Re-add container to old parent
		if (oldParent != null)
		{
			ContainerView.SetParent(oldParent);
		}
	}

	protected override void RemoveContainer()
	{
		if (PlatformView == null || ContainerView == null)
		{
			CleanupContainerView();
			return;
		}

		if (PlatformView.GetParent() != ContainerView)
		{
			CleanupContainerView();
			return;
		}

		var oldParent = ContainerView?.GetParent();

		// Remove container from its parent
		ContainerView?.Unparent();

		CleanupContainerView();

		// Re-add PlatformView to old parent
		if (oldParent != null)
		{
			PlatformView?.SetParent(oldParent);
		}
	}

	void CleanupContainerView()
	{
		if (ContainerView is Platform.WrapperView wrapperView)
		{
			wrapperView.Child = null;
			wrapperView.Dispose();
		}

		ContainerView = null;
	}
}