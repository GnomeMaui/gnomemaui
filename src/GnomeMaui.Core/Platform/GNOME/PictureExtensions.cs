using System;

namespace Microsoft.Maui.Platform
{
	public static class PictureExtensions
	{
		public static void Clear(this Gtk.Picture picture)
		{
			picture.Paintable = default!;
		}

		public static void UpdateAspect(this Gtk.Picture picture, IImage image)
		{
			picture.ContentFit = image.Aspect.ToContentFit();
		}

		public static void UpdateIsAnimationPlaying(this Gtk.Picture picture, IImageSourcePart image)
		{
			// TODO: Implement animation support for GNOME
			// GdkPixbuf.PixbufAnimation can be used for animated images
#if DEBUG
			if (image.IsAnimationPlaying)
			{
				Console.Out.WriteLine($"[PictureExtensions][UpdateIsAnimationPlaying] Animation not yet supported");
			}
#endif
		}

		public static Gtk.ContentFit ToContentFit(this Aspect aspect)
		{
			return aspect switch
			{
				Aspect.AspectFit => Gtk.ContentFit.Contain,
				Aspect.AspectFill => Gtk.ContentFit.Cover,
				Aspect.Fill => Gtk.ContentFit.Fill,
				Aspect.Center => Gtk.ContentFit.ScaleDown,
				_ => Gtk.ContentFit.Contain
			};
		}
	}
}
