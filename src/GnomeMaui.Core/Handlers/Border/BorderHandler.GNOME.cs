using Microsoft.Maui.Platform;
using System;

namespace Microsoft.Maui.Handlers;

public partial class BorderHandler : ViewHandler<IBorderView, ContentWidget>
{
	protected override ContentWidget CreatePlatformView() => this.Create();

	public override bool NeedsContainer =>
		VirtualView?.Stroke != null ||
		VirtualView?.StrokeThickness > 0 ||
		VirtualView?.Shape != null ||
		base.NeedsContainer;

	protected override void SetupContainer()
	{
		base.SetupContainer();

		if (VirtualView != null)
		{
			ContainerView?.UpdateBorder(VirtualView);
		}
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

		PlatformView.CrossPlatformLayout = VirtualView;
	}

	static partial void UpdateContent(IBorderHandler handler)
	{
		handler.UpdateContent();
	}

	// Stroke metódusok - StrokeExtensions kezeli, nincs szükség Map-re
	// A StrokeExtensions automatikusan meghívódik a base handler által

	protected override void DisconnectHandler(ContentWidget platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.Content = null;
	}
}