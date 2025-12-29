using Microsoft.Maui;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Maui.Handlers;

public partial class SKImageSourceService
{
	public override Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default)
	{
		var picture = imageSource switch
		{
			ISKImageImageSource img => img.Image?.ToPicture(),
			ISKBitmapImageSource bmp => bmp.Bitmap?.ToPicture(),
			ISKPixmapImageSource pix => pix.Pixmap?.ToPicture(),
			ISKPictureImageSource pic => pic.Picture?.ToPicture(pic.Dimensions),
			_ => null,
		};

		return picture != null
			? FromResult(new ImageSourceServiceResult(picture))
			: FromResult(null);
	}

	static Task<IImageSourceServiceResult<Gtk.Picture>?> FromResult(ImageSourceServiceResult? result) =>
		Task.FromResult<IImageSourceServiceResult<Gtk.Picture>?>(result);
}

public static class SKImageSourceExtensions
{
	public static Gtk.Picture? ToPicture(this SKImage? image)
	{
		if (image == null)
		{
			return null;
		}

		using var data = image.Encode();
		using var loader = GdkPixbuf.PixbufLoader.New();

		var bytes = data.ToArray();
		loader.Write(bytes.AsSpan());
		loader.Close();

		var pixbuf = loader.GetPixbuf();
		if (pixbuf is null)
			return null;

		var texture = Gdk.Texture.NewForPixbuf(pixbuf);
		return Gtk.Picture.NewForPaintable(texture);
	}

	public static Gtk.Picture? ToPicture(this SKBitmap? bitmap)
	{
		if (bitmap == null)
		{
			return null;
		}

		using var image = SKImage.FromBitmap(bitmap);
		return image.ToPicture();
	}

	public static Gtk.Picture? ToPicture(this SKPixmap? pixmap)
	{
		if (pixmap == null)
		{
			return null;
		}

		using var image = SKImage.FromPixels(pixmap);
		return image.ToPicture();
	}

	public static Gtk.Picture? ToPicture(this SKPicture? picture, SKSizeI dimensions)
	{
		if (picture == null)
		{
			return null;
		}

		using var surface = SKSurface.Create(new SKImageInfo(dimensions.Width, dimensions.Height));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.Transparent);
		canvas.DrawPicture(picture);
		canvas.Flush();

		using var image = surface.Snapshot();
		return image.ToPicture();
	}
}
