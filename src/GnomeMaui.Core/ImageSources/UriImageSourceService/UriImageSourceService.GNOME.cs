using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class UriImageSourceService
{
	public override Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IUriImageSource)imageSource, cancellationToken);

	public Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IUriImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource.IsEmpty)
			return FromResult(null);

		var uri = imageSource.Uri;
#if DEBUG
		Console.Out.WriteLine($"[UriImageSourceService][GetImageAsync] uri: {uri}");
#endif

		if (uri is null)
		{
			Logger?.LogWarning("Unable to load image URI: uri is null.");
			return FromResult(null);
		}

		var picture = uri.IsFile
			? Gtk.Picture.NewForFilename(uri.LocalPath)
			: Gtk.Picture.NewForFilename(uri.AbsoluteUri);

		var result = new ImageSourceServiceResult(picture);
		return FromResult(result);
	}

	static Task<IImageSourceServiceResult<Gtk.Picture>?> FromResult(IImageSourceServiceResult<Gtk.Picture>? result) =>
		Task.FromResult(result);
}
