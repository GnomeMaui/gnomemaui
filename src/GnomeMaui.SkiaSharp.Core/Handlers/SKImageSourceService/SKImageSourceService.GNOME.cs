using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Maui.Handlers;

public partial class SKImageSourceService
{
	public override Task<IImageSourceServiceResult<SKImageView>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default)
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

	static Task<IImageSourceServiceResult<SKImageView>?> FromResult(ImageSourceServiceResult? result) =>
		Task.FromResult<IImageSourceServiceResult<SKImageView>?>(result);
}

public static class SKImageSourceExtensions
{
	public static SKImageView? ToPicture(this SKImage? image)
	{
		if (image == null)
			return null;

		var view = new SKImageView();
		view.Image = image;
		return view;
	}

	public static SKImageView? ToPicture(this SKBitmap? bitmap)
	{
		if (bitmap == null)
			return null;

		var image = SKImage.FromBitmap(bitmap);
		if (image == null)
			return null;

		var view = new SKImageView();
		view.Image = image;
		return view;
	}

	public static SKImageView? ToPicture(this SKPixmap? pixmap)
	{
		if (pixmap == null)
			return null;

		var image = SKImage.FromPixels(pixmap);
		if (image == null)
			return null;

		var view = new SKImageView();
		view.Image = image;
		return view;
	}

	public static SKImageView? ToPicture(this SKPicture? picture, SKSizeI dimensions)
	{
		if (picture == null)
			return null;

		using var surface = SKSurface.Create(new SKImageInfo(dimensions.Width, dimensions.Height));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.Transparent);
		canvas.DrawPicture(picture);
		canvas.Flush();

		var image = surface.Snapshot();
		if (image == null)
			return null;

		var view = new SKImageView();
		view.Image = image;
		return view;
	}
}
