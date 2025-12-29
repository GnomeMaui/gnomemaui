using Microsoft.Maui;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform
{
	public static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult<Gtk.Picture>?> UpdateSourceAsync(
			this IImageSourcePart image,
			Gtk.Widget destinationContext,
			IImageSourceServiceProvider services,
			Action<Gtk.Picture?> setImage,
			CancellationToken cancellationToken = default)
		{
#if DEBUG
			Console.Out.WriteLine($"[ImageSourcePartExtensions][UpdateSourceAsync] Starting image load");
#endif
			image.UpdateIsLoading(false);

			var imageSource = image.Source;
			if (imageSource == null)
			{
#if DEBUG
				Console.Out.WriteLine($"[ImageSourcePartExtensions][UpdateSourceAsync] imageSource is null");
#endif
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
#if DEBUG
				Console.Out.WriteLine($"[ImageSourcePartExtensions][UpdateSourceAsync] Service: {service.GetType().Name}");
#endif

				// Use dynamic dispatch to call the appropriate GetImageAsync method
				dynamic dynamicService = service;
				var result = await dynamicService.GetImageAsync(imageSource, cancellationToken) as IImageSourceServiceResult<Gtk.Picture>;

				var picture = result?.Value;

#if DEBUG
				Console.Out.WriteLine($"[ImageSourcePartExtensions][UpdateSourceAsync] Picture loaded: {picture != null}");
#endif

				var applied = !cancellationToken.IsCancellationRequested && picture != null && imageSource == image.Source;

				if (applied)
				{
					setImage.Invoke(picture);
#if DEBUG
					Console.Out.WriteLine($"[ImageSourcePartExtensions][UpdateSourceAsync] Image set successfully");
#endif
				}

				events?.LoadingCompleted(applied);
				return result;
			}
			catch (Exception ex)
			{
#if DEBUG
				Console.Out.WriteLine($"[ImageSourcePartExtensions][UpdateSourceAsync] Exception: {ex}");
#endif
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
