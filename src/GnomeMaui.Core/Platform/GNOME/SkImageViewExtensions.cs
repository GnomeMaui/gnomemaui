using System;

namespace Microsoft.Maui.Platform
{
	public static class SkImageViewExtensions
	{
		public static SKImageView Create(this ImageHandler _) => new();

		public static void Clear(this SKImageView view)
		{
			view.Image = null;
		}

		public static void UpdateAspect(this SKImageView view, IImage image)
		{
			view.Aspect = image.Aspect;
		}

		public static void UpdateWidth(this SKImageView view, IImage image)
		{
			if (Primitives.Dimension.IsExplicitSet(image.Width))
			{
				view.WidthRequest = (int)image.Width;
			}
			else
			{
				view.WidthRequest = -1;
			}
		}

		public static void UpdateHeight(this SKImageView view, IImage image)
		{
			if (Primitives.Dimension.IsExplicitSet(image.Height))
			{
				view.HeightRequest = (int)image.Height;
			}
			else
			{
				view.HeightRequest = -1;
			}
		}

		public static void UpdateIsAnimationPlaying(this SKImageView view, IImageSourcePart image)
		{
			// TODO: Implement animation support for GNOME
			// SKImage.AnimatedImageFormat can be used for animated images (GIF, WebP)
#if DEBUG
			if (image.IsAnimationPlaying)
			{
				Console.Out.WriteLine($"[SkImageViewExtensions][UpdateIsAnimationPlaying] Animation not yet supported");
			}
#endif
		}
	}
}
