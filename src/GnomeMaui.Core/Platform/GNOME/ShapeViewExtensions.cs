using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform;

public static class ShapeViewExtensions
{
	public static void UpdateShape(this SkiaGLGraphicsView platformView, IShapeView shapeView)
		=> platformView.Drawable = new ShapeDrawable(shapeView);

	public static void InvalidateShape(this SkiaGLGraphicsView platformView, IShapeView shapeView)
		=> platformView.Invalidate();
}