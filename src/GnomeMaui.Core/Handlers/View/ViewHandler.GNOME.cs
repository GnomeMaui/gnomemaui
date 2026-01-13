namespace Microsoft.Maui.Handlers;

public partial class ViewHandler
{
	public static void MapToolbar(IViewHandler handler, IView view)
	{
		if (view is IToolbarElement tb)
		{
			MapToolbar(handler, tb);
		}
	}

	internal static void MapToolbar(IElementHandler handler, IToolbarElement toolbarElement)
	{
		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} null");

		if (toolbarElement.Toolbar != null)
		{
			var toolBar = toolbarElement.Toolbar.ToPlatform(handler.MauiContext);

			// First try to set toolbar directly on the platform view if it's an IToolbarContainer
			if (handler is IPlatformViewHandler platformViewHandler &&
				platformViewHandler.PlatformView is IToolbarContainer toolbarContainer)
			{
				toolbarContainer.SetToolbar((MauiToolbar)toolBar!);
			}
			else
			{
				// Otherwise set it on the NavigationRootManager
				handler.MauiContext.GetNavigationRootManager().SetToolbar((MauiToolbar?)toolBar);
			}
		}
	}

	public static void MapTranslationX(IViewHandler handler, IView view) { }
	public static void MapTranslationY(IViewHandler handler, IView view) { }
	public static void MapScale(IViewHandler handler, IView view) { }
	public static void MapScaleX(IViewHandler handler, IView view) { }
	public static void MapScaleY(IViewHandler handler, IView view) { }
	public static void MapRotation(IViewHandler handler, IView view) { }
	public static void MapRotationX(IViewHandler handler, IView view) { }
	public static void MapRotationY(IViewHandler handler, IView view) { }
	public static void MapAnchorX(IViewHandler handler, IView view) { }
	public static void MapAnchorY(IViewHandler handler, IView view) { }
}
