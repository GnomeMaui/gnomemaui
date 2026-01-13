using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace MauiBlazorApp1;

public static class MauiProgram
{
	internal static long StartTime;

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

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
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
