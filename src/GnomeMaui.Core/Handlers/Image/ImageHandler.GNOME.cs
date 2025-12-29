using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers;

public partial class ImageHandler : ViewHandler<IImage, Gtk.Picture>
{
	protected override Gtk.Picture CreatePlatformView() => Gtk.Picture.New();

	protected override void DisconnectHandler(Gtk.Picture platformView)
	{
		base.DisconnectHandler(platformView);
		SourceLoader.Reset();
	}

	public override bool NeedsContainer =>
		VirtualView?.Background != null ||
		base.NeedsContainer;

	public static void MapBackground(IImageHandler handler, IImage image)
	{
		handler.UpdateValue(nameof(IViewHandler.ContainerView));
		handler.ToPlatform().UpdateBackground(image);
	}

	public static void MapAspect(IImageHandler handler, IImage image) =>
		handler.PlatformView?.UpdateAspect(image);

	public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
		handler.PlatformView?.UpdateIsAnimationPlaying(image);

	public static void MapSource(IImageHandler handler, IImage image) =>
		MapSourceAsync(handler, image).FireAndForget(handler);

	public static async Task MapSourceAsync(IImageHandler handler, IImage image) =>
		await handler.SourceLoader.UpdateImageSourceAsync();

	partial class ImageImageSourcePartSetter
	{
		public override void SetImageSource(Gtk.Picture? platformImage)
		{
#if DEBUG
			Console.Out.WriteLine($"[ImageHandler][SetImageSource] platformImage: {platformImage}");
#endif
			if (Handler?.PlatformView is not Gtk.Picture picture)
				return;

			if (platformImage is not null && platformImage.Paintable is not null)
			{
				picture.Paintable = platformImage.Paintable;
			}
			else
			{
				picture.Clear();
			}
		}
	}
}