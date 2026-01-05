using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiTest1;

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
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
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
						adwWindow.DefaultWidth = 1280;
						adwWindow.DefaultHeight = 800;
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
