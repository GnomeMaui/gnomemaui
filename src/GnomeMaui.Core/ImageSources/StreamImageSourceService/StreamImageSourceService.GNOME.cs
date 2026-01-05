using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class StreamImageSourceService
{
	public override Task<IImageSourceServiceResult<SKImageView>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IStreamImageSource)imageSource, cancellationToken);

	public async Task<IImageSourceServiceResult<SKImageView>?> GetImageAsync(IStreamImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource.IsEmpty)
			return null;

		try
		{
			var stream = await imageSource.GetStreamAsync(cancellationToken);
			if (stream is null)
			{
				Logger?.LogWarning("Unable to load image stream: stream is null.");
				return null;
			}

			using var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream, cancellationToken);
			memoryStream.Position = 0;

			// Directly create SKImage from stream
			var skImage = SkiaSharp.SKImage.FromEncodedData(memoryStream);
			if (skImage is null)
			{
				Logger?.LogWarning("Unable to load image stream: SKImage creation failed.");
				return null;
			}

#if DEBUG
			Console.Out.WriteLine($"[StreamImageSourceService][GetImageAsync] SKImage created: {skImage.Width}x{skImage.Height}");
#endif

			var view = new SKImageView();
			view.Image = skImage;

#if DEBUG
			Console.Out.WriteLine($"[StreamImageSourceService][GetImageAsync] SKImageView created, Image property set");
#endif

			return new ImageSourceServiceResult(view);
		}
		catch (Exception ex)
		{
			Logger?.LogWarning(ex, "Unable to load image stream.");
			return null;
		}
	}
}
