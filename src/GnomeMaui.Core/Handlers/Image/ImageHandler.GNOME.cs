using GnomeMaui.CSS;
using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers;

public partial class ImageHandler : ViewHandler<IImage, SKImageView>
{
	protected override SKImageView CreatePlatformView() => this.Create();

	protected override void DisconnectHandler(SKImageView platformView)
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

	public static void MapWidth(IImageHandler handler, IImage view)
	{
		if (handler.ContainerView is Gtk.Widget container)
		{
			container.WidthRequest = Primitives.Dimension.IsExplicitSet(view.Width) ? (int)view.Width : -1;
		}
		else
		{
			ViewHandler.MapWidth(handler, view);
		}

		handler.PlatformView?.UpdateWidth(view);
	}

	public static void MapHeight(IImageHandler handler, IImage view)
	{
		if (handler.ContainerView is Gtk.Widget container)
		{
			container.HeightRequest = Primitives.Dimension.IsExplicitSet(view.Height) ? (int)view.Height : -1;
		}
		else
		{
			ViewHandler.MapHeight(handler, view);
		}

		handler.PlatformView?.UpdateHeight(view);
	}

	public static void MapIsAnimationPlaying(IImageHandler handler, IImage image)
	{ }
	//  =>
	// 	handler.PlatformView?.UpdateIsAnimationPlaying(image);

	public static void MapSource(IImageHandler handler, IImage image) =>
		MapSourceAsync(handler, image).FireAndForget(handler);

	public static async Task MapSourceAsync(IImageHandler handler, IImage image) =>
		await handler.SourceLoader.UpdateImageSourceAsync();

	partial class ImageImageSourcePartSetter
	{
		public override void SetImageSource(SkiaSharp.SKImage? platformImage)
		{
			if (Handler?.PlatformView is not SKImageView view)
			{
				return;
			}

			if (platformImage is not null)
			{
				view.Image = platformImage;
			}

			if (Handler?.VirtualView is IImage image && image.Source is IStreamImageSource)
				view.InvalidateMeasure(image);
		}
	}
}