using System.Runtime.InteropServices;

namespace Microsoft.Maui.Graphics.Platform;

public static partial class Module
{
	[LibraryImport("libc", EntryPoint = "setlocale", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	private static partial IntPtr Setlocale(int category, string locale);

	const int LC_NUMERIC = 1;
	static bool LocaleFixed = false;
	static readonly object LockObject = new();

	/// <summary>
	/// Fixes LC_NUMERIC locale for SkiaSharp parsing functions.
	/// 
	/// Call after GirCore moules initialization, for example:
	/// 
	/// Gtk.Module.Initialize();
	/// Adw.Module.Initialize();
	/// ...
	/// SkiaSharp.Views.GirCore.Module.Initialize();
	/// 
	/// Call this once before using any SkiaSharp string-to-number parsing.
	/// </summary>
	public static void Initialize()
	{
		if (LocaleFixed)
		{
			return;
		}

		lock (LockObject)
		{
			if (!LocaleFixed)
			{
				_ = Setlocale(LC_NUMERIC, "C");
				LocaleFixed = true;
			}
		}
	}
}