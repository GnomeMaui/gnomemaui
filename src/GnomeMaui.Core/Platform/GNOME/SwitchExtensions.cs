using GnomeMaui.CSS;
using Microsoft.Maui.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

public static class SwitchExtensions
{
	const string Prefix = "switch";

	public static Gtk.Switch Create(this SwitchHandler handler)
	{
		var @switch = handler.VirtualView;
		var gtkSwitch = Gtk.Switch.New();
		gtkSwitch.AddCssClass($"{CssCache.Prefix}{Prefix}");
		gtkSwitch.SetActive(@switch?.IsOn ?? false);
		return gtkSwitch;
	}

	public static void UpdateIsOn(this Gtk.Switch gtkSwitch, ISwitch @switch)
	{
		gtkSwitch.SetActive(@switch.IsOn);
	}

	public static void UpdateTrackColor(this Gtk.Switch gtkSwitch, ISwitch @switch)
	{
		var trackColor = @switch.TrackColor;

		if (trackColor != null)
		{
			var (r, g, b, a) = ((int)(trackColor.Red * 255), (int)(trackColor.Green * 255), (int)(trackColor.Blue * 255), trackColor.Alpha);
			CssCache.AddClassSelector($"{CssCache.Prefix}{Prefix} > slider {{ " +
				$"background-color: rgba({r}, {g}, {b}, {a}); }}");
		}
	}

	public static void UpdateThumbColor(this Gtk.Switch gtkSwitch, ISwitch @switch)
	{
		var thumbColor = @switch.ThumbColor;

		if (thumbColor != null)
		{
			var (r, g, b, a) = ((int)(thumbColor.Red * 255), (int)(thumbColor.Green * 255), (int)(thumbColor.Blue * 255), thumbColor.Alpha);
			CssCache.AddClassSelector($"{CssCache.Prefix}{Prefix}:checked > slider {{ " +
				$"background-color: rgba({r}, {g}, {b}, {a}); }}");
		}
	}
}
