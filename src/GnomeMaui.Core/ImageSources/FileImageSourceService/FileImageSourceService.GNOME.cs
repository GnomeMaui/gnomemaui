using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class FileImageSourceService
{
	public override Task<IImageSourceServiceResult<SKImageView>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IFileImageSource)imageSource, cancellationToken);

	public Task<IImageSourceServiceResult<SKImageView>?> GetImageAsync(IFileImageSource imageSource, CancellationToken cancellationToken = default)
	{
		Console.WriteLine($"[FileImageSourceService][GetImageAsync] called");
		if (imageSource.IsEmpty)
			return FromResult(null);

		var filename = imageSource.File;
#if DEBUG
		Console.Out.WriteLine($"[FileImageSourceService][GetImageAsync] filename: {filename}");
#endif
		if (string.IsNullOrEmpty(filename))
		{
			Logger?.LogWarning("Unable to load image file: filename is null or empty.");
			return FromResult(null);
		}

		var fullPath = GetFullPath(filename);
#if DEBUG
		Console.Out.WriteLine($"[FileImageSourceService][GetImageAsync] fullPath: {fullPath}");
#endif

		if (!File.Exists(fullPath))
		{
			Logger?.LogWarning("Unable to load image file '{File}': file does not exist.", fullPath);
			return FromResult(null);
		}

		var skImage = SKImage.FromEncodedData(fullPath);
		if (skImage == null)
			return FromResult(null);

		var view = new SKImageView();
		view.Image = skImage;

		var result = new ImageSourceServiceResult(view);
		return FromResult(result);
	}

	static Task<IImageSourceServiceResult<SKImageView>?> FromResult(IImageSourceServiceResult<SKImageView>? result) =>
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
