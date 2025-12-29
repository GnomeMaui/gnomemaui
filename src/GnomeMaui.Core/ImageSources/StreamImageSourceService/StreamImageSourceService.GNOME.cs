using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui;

public partial class StreamImageSourceService
{
	public override Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
		GetImageAsync((IStreamImageSource)imageSource, cancellationToken);

	public async Task<IImageSourceServiceResult<Gtk.Picture>?> GetImageAsync(IStreamImageSource imageSource, CancellationToken cancellationToken = default)
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
			var bytes = memoryStream.ToArray();

			var gBytes = GLib.Bytes.New(bytes);
			var inputStream = Gio.MemoryInputStream.NewFromBytes(gBytes);
			var pixbuf = GdkPixbuf.Pixbuf.NewFromStream(inputStream, null);

			if (pixbuf is null)
			{
				Logger?.LogWarning("Unable to load image stream: pixbuf is null.");
				return null;
			}

			var texture = Gdk.Texture.NewForPixbuf(pixbuf);
			var picture = Gtk.Picture.NewForPaintable(texture);

			return new ImageSourceServiceResult(picture);
		}
		catch (Exception ex)
		{
			Logger?.LogWarning(ex, "Unable to load image stream.");
			return null;
		}
	}
}
