using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class FontImageSourceService
{
	public override Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IFontImageSource)imageSource, cancellationToken);

	public Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IFontImageSource imageSource, CancellationToken cancellationToken = default)
	{
		if (imageSource.IsEmpty)
			return FromResult(null);

		// TODO: Implement font-based image rendering for GNOME
		// This would require rendering text using Pango/Cairo to a surface
		// and then converting to Gdk.Texture/Gtk.Picture
#if DEBUG
		Console.Out.WriteLine($"[FontImageSourceService][GetImageAsync] Font image source not yet supported");
#endif
		Logger?.LogWarning("Font image sources are not yet supported on GNOME platform.");
		return FromResult(null);
	}

	static Task<IImageSourceServiceResult<Gtk.Picture>?> FromResult(IImageSourceServiceResult<Gtk.Picture>? result) =>
		Task.FromResult(result);
}
