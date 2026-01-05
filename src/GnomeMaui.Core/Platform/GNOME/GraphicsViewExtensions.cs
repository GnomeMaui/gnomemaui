using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this PlatformTouchGraphicsView platformGraphicsView, IGraphicsView graphicsView)
		{
			platformGraphicsView.Drawable = graphicsView.Drawable;
		}

		internal static void UpdateBackground(this PlatformTouchGraphicsView platformGraphicsView, IGraphicsView graphicsView)
		{
			if (graphicsView.Background?.BackgroundColor is Color backgroundColor)
				platformGraphicsView.BackgroundColor = backgroundColor;
		}
	}
}
