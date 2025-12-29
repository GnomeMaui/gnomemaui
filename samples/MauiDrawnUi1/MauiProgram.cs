using DrawnUi.Draw;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiDrawnUi1;

public static class MauiProgram
{
	internal static long StartTime;

	public static MauiApp CreateMauiApp()
	{
		foreach (var resource in typeof(MauiProgram).Assembly.GetManifestResourceNames())
		{
			Console.WriteLine($"Resource: {resource}");
		}

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.UseDrawnUi()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "FontText");
				fonts.AddFont("OpenSans-Semibold.ttf", "FontTextBold");
			});

#if GNOME
		builder.ConfigureLifecycleEvents(events =>
		{
			events.AddGnome(gnomeLifecycleBuilder =>
			{
				gnomeLifecycleBuilder.OnWindowAdded((application, args) =>
				{
					Console.WriteLine($"Gnome Window Added: {args.Window.GetType().FullName}: {args.Window.Title}");

					if (args.Window is Adw.ApplicationWindow adwWindow)
					{
						adwWindow.DefaultWidth = 800;
						adwWindow.DefaultHeight = 600;
					}
				});
			});
		});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
