using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class UriImageSourceService
{
	public override Task<IImageSourceServiceResult<SKImage>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IUriImageSource)imageSource, cancellationToken);

	public async Task<IImageSourceServiceResult<SKImage>?> GetImageAsync(IUriImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource.IsEmpty)
			return null;

		var uri = imageSource.Uri;

		if (uri is null)
		{
			Logger?.LogWarning("Unable to load image URI: uri is null.");
			return null;
		}

		try
		{
			// Cast to IStreamImageSource and use GetStreamAsync like Windows/iOS platforms
			if (imageSource is not IStreamImageSource streamImageSource)
			{
				Logger?.LogWarning($"Unable to load image as stream from URI: {uri}");
				return null;
			}

			var stream = await streamImageSource.GetStreamAsync(cancellationToken);
			if (stream is null)
			{
				Logger?.LogWarning($"Unable to load image stream from URI: {uri}");
				return null;
			}

			using var memoryStream = new System.IO.MemoryStream();
			await stream.CopyToAsync(memoryStream, cancellationToken);
			memoryStream.Position = 0;

			// Directly create SKImage from stream
			var skImage = SkiaSharp.SKImage.FromEncodedData(memoryStream);
			if (skImage is null)
			{
				Logger?.LogWarning($"Unable to create SKImage from URI: {uri}");
				return null;
			}

			return new ImageSourceServiceResult(skImage);
		}
		catch (Exception ex)
		{
			Logger?.LogWarning(ex, $"Unable to load image from URI: {uri}");
			return null;
		}
	}
}
