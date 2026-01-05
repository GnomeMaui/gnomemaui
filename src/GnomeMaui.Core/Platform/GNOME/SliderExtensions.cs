using GnomeMaui.CSS;
using Microsoft.Maui.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

public static class SliderExtensions
{
	const string Prefix = "slider";

	public static Gtk.Scale Create(this SliderHandler handler)
	{
		// Use default slider values: Minimum=0, Maximum=1, Value=0
		var slider = handler.VirtualView;
		var min = slider?.Minimum ?? 0;
		var max = slider?.Maximum ?? 1;
		var value = slider?.Value ?? 0;

		var adjustment = Gtk.Adjustment.New(value, min, max, 0.1, 0.1, 0);
		var scale = Gtk.Scale.New(Gtk.Orientation.Horizontal, adjustment);
		scale.SetDrawValue(false);
		scale.SetHexpand(true);
		scale.AddCssClass($"{CssCache.Prefix}{Prefix}");
		return scale;
	}

	public static void UpdateMinimum(this Gtk.Scale scale, ISlider slider)
	{
		var adjustment = scale.GetAdjustment();
		adjustment.SetLower(slider.Minimum);
		adjustment.SetValue(slider.Value);
	}

	public static void UpdateMaximum(this Gtk.Scale scale, ISlider slider)
	{
		var adjustment = scale.GetAdjustment();
		adjustment.SetUpper(slider.Maximum);
		adjustment.SetValue(slider.Value);
	}

	public static void UpdateValue(this Gtk.Scale scale, ISlider slider)
	{
		var value = slider.Value;
		if (Math.Abs(scale.GetValue() - value) > double.Epsilon)
		{
			scale.SetValue(value);
		}
	}

	public static void UpdateMinimumTrackColor(this Gtk.Scale _, ISlider slider)
	{
		if (slider.MinimumTrackColor == null)
			return;

		var color = slider.MinimumTrackColor;
		// GTK4: scale > trough > highlight is the filled/minimum track
		CssCache.AddClassSelector($"{CssCache.Prefix}{Prefix} > trough > highlight {{ " +
			$"background-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); " +
			$"border-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); " +
			$"}}");
	}

	public static void UpdateMaximumTrackColor(this Gtk.Scale _, ISlider slider)
	{
		if (slider.MaximumTrackColor == null)
			return;

		var color = slider.MaximumTrackColor;
		// GTK4: scale > trough is the background/maximum track
		CssCache.AddClassSelector($"{CssCache.Prefix}{Prefix} > trough {{ " +
			$"background-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); " +
			$"border-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); " +
			$"}}");
	}

	public static void UpdateThumbColor(this Gtk.Scale _, ISlider slider)
	{
		if (slider.ThumbColor == null)
			return;

		var color = slider.ThumbColor;
		// GTK4: scale > trough > slider is the thumb
		CssCache.AddClassSelector($"{CssCache.Prefix}{Prefix} > trough > slider {{ " +
			$"background-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); " +
			$"border-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); " +
			$"}}");
	}

	public static void UpdateThumbImageSource(this Gtk.Scale _, ISlider slider, IImageSourceServiceProvider provider)
	{
		if (slider.ThumbImageSource == null)
			return;

		// Direct file image source
		if (slider.ThumbImageSource is IFileImageSource fileImageSource &&
			!string.IsNullOrEmpty(fileImageSource.File) &&
			System.IO.File.Exists(fileImageSource.File))
		{
			SetThumbImageCss(fileImageSource.File);
			return;
		}

		// Async image loading for other sources
		LoadThumbImageAsync(slider, provider).FireAndForget();
	}

	static void SetThumbImageCss(string imagePath)
	{
		// GTK4: scale > trough > slider is the thumb
		// Use CSS to set background-image
		CssCache.AddClassSelector($"{CssCache.Prefix}{Prefix} > trough > slider {{ " +
			$"background-image: url('{imagePath}'); " +
			$"background-size: contain; " +
			$"background-position: center; " +
			$"background-repeat: no-repeat; " +
			$"}}");
	}

	static async Task LoadThumbImageAsync(ISlider slider, IImageSourceServiceProvider provider)
	{
		var imageSource = slider.ThumbImageSource;
		if (imageSource == null)
			return;

		try
		{
			var service = provider.GetImageSourceService(imageSource.GetType());
			if (service is null)
				throw new InvalidOperationException($"Unable to find image source service for {imageSource.GetType()}.");

			// Use dynamic dispatch to call GetImageAsync which returns IImageSourceServiceResult<Gtk.Picture>
			dynamic dynamicService = service;
			var result = await dynamicService.GetImageAsync(imageSource, CancellationToken.None) as IImageSourceServiceResult<Gtk.Picture>;

			var picture = result?.Value;
			if (picture == null)
				return;

			// Get the file path from the Gtk.Picture
			var file = picture.GetFile();
			var path = file?.GetPath();

			if (!string.IsNullOrEmpty(path))
			{
				SetThumbImageCss(path);
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[SliderExtensions] Failed to load thumb image: {ex.Message}");
		}
	}
}
