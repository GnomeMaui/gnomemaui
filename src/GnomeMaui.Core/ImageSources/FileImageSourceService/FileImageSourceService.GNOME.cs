using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class FileImageSourceService
{
	public override Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IFileImageSource)imageSource, cancellationToken);

	public Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IFileImageSource imageSource, CancellationToken cancellationToken = default)
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

		var picture = Gtk.Picture.NewForFilename(fullPath);
		var result = new ImageSourceServiceResult(picture);
		return FromResult(result);
	}

	static Task<IImageSourceServiceResult<Gtk.Picture>?> FromResult(IImageSourceServiceResult<Gtk.Picture>? result) =>
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
