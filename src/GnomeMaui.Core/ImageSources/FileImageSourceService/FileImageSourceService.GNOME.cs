using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class FileImageSourceService
{
	public override Task<IImageSourceServiceResult<SKImage>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IFileImageSource)imageSource, cancellationToken);

	public Task<IImageSourceServiceResult<SKImage>?> GetImageAsync(IFileImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource.IsEmpty)
			return FromResult(null);

		var filename = imageSource.File;
		if (string.IsNullOrEmpty(filename))
		{
			Logger?.LogWarning("Unable to load image file: filename is null or empty.");
			return FromResult(null);
		}

		var fullPath = GetFullPath(filename);

		if (!File.Exists(fullPath))
		{
			Logger?.LogWarning("Unable to load image file '{File}': file does not exist.", fullPath);
			return FromResult(null);
		}

		var skImage = SKImage.FromEncodedData(fullPath);
		if (skImage == null)
			return FromResult(null);

		var result = new ImageSourceServiceResult(skImage);
		return FromResult(result);
	}

	static Task<IImageSourceServiceResult<SKImage>?> FromResult(IImageSourceServiceResult<SKImage>? result) =>
		Task.FromResult(result);

	static string GetFullPath(string filename)
	{
		if (Path.IsPathRooted(filename))
			return filename;

		var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Images", filename);
		if (File.Exists(resourcePath))
			return resourcePath;

		resourcePath = Path.Combine(AppContext.BaseDirectory, filename);
		if (File.Exists(resourcePath))
			return resourcePath;

		return filename;
	}
}
