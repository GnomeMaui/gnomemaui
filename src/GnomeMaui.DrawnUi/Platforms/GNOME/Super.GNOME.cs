namespace DrawnUi.Draw;

using GLib;
using System.Runtime.Versioning;

public partial class Super
{
	public static event EventHandler OnFrame;

	public static int RefreshRate { get; protected set; }

	public static bool UsingDisplaySync { get; protected set; }

	private static uint _timeoutSourceId = 0;
	private static object _lockInit = new();

	public static void Init()
	{
		lock (_lockInit)
		{
			if (Initialized)
			{
				return;
			}

			Initialized = true;
		}

#if DEBUG
		Console.Out.WriteLine("[Super][Init] GNOME Initializing...");
#endif

		RefreshRate = GetDisplayRefreshRate(60);

		var displayInfo = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo;
		Super.Screen.Density = displayInfo.Density;
		Super.Screen.WidthDip = displayInfo.Width / Super.Screen.Density;
		Super.Screen.HeightDip = displayInfo.Height / Super.Screen.Density;

#if DEBUG
		Console.Out.WriteLine($"[Super][Init] Density: {Super.Screen.Density}, WidthDip: {Super.Screen.WidthDip}, HeightDip: {Super.Screen.HeightDip}");
		Console.Out.WriteLine($"[Super][Init] TouchEffect.Density: {AppoMobi.Maui.Gestures.TouchEffect.Density}");
#endif

		InitShared();

		// Start frame loop using GLib timeout
		var intervalMs = Math.Max(8, 1000 / Math.Max(30, RefreshRate));
		_timeoutSourceId = Functions.TimeoutAdd(Constants.PRIORITY_DEFAULT, (uint)intervalMs, new SourceFunc(() =>
		{
			OnFrame?.Invoke(null, EventArgs.Empty);
			return Constants.SOURCE_CONTINUE;
		}));

#if DEBUG
		Console.Out.WriteLine($"[Super][Init] Initialization complete. Frame loop: {intervalMs}ms");
#endif
	}

	public static int GetDisplayRefreshRate(int fallback)
	{
		var displayInfo = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo;
		int refreshRateHz = (int)displayInfo.RefreshRate;
		return refreshRateHz > 0 ? refreshRateHz : fallback;
	}
}
