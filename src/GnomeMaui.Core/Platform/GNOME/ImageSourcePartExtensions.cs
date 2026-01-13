using Microsoft.Maui;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform
{
	public static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult<SKImage>?> UpdateSourceAsync(
			this IImageSourcePart image,
			Gtk.Widget destinationContext,
			IImageSourceServiceProvider services,
			Action<SKImage?> setImage,
			CancellationToken cancellationToken = default)
		{
			image.UpdateIsLoading(false);

			var imageSource = image.Source;
			if (imageSource == null)
			{
				return null;
			}

			var events = image as IImageSourcePartEvents;

			events?.LoadingStarted();
			image.UpdateIsLoading(true);

			try
			{
				var service = services.GetImageSourceService(imageSource.GetType());
				if (service is null)
					throw new InvalidOperationException($"Unable to find image source service for {imageSource.GetType()}.");

				// Use dynamic dispatch to call the appropriate GetImageAsync method
				dynamic dynamicService = service;
				var result = await dynamicService.GetImageAsync(imageSource, cancellationToken) as IImageSourceServiceResult<SKImage>;

				var picture = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && picture != null && imageSource == image.Source;

				if (applied)
				{
					setImage.Invoke(picture);
					//picture?.Invalidate();
				}

				events?.LoadingCompleted(applied);
				return result;
			}
			catch (Exception ex)
			{
				events?.LoadingFailed(ex);
			}
			finally
			{
				image.UpdateIsLoading(false);
			}

			return null;
		}
	}
}
