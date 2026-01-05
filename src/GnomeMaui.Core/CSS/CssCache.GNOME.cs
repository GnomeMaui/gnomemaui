using System.Collections.Concurrent;
using System.Text;

namespace GnomeMaui.CSS;

public static class CssCache
{
	public const string CssClass = "maui";
	public const string Prefix = $"{CssClass}-";
	private static readonly Gtk.CssProvider _provider;
	private static readonly ConcurrentDictionary<string, byte> _cache = new();
	private static readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
	private static readonly StringBuilder _allCss = new();

	static CssCache()
	{
		_provider = Gtk.CssProvider.New();

		// Load reset.css from application assembly if available
		LoadResetCss();

		Gtk.StyleContext.AddProviderForDisplay(
			Gdk.Display.GetDefault()!,
			_provider,
			Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION);
	}

	private static void LoadResetCss()
	{
		var assembly = MauiAdwApplication.ApplicationAssembly;
		if (assembly == null)
			return;

		var resourceName = assembly.GetManifestResourceNames()
			.FirstOrDefault(name => name.EndsWith("GNOME.reset.css"));

		if (resourceName != null)
		{
			using var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream != null)
			{
				using var reader = new StreamReader(stream);
				var resetCss = reader.ReadToEnd();
				_allCss.AppendLine(resetCss);
#if DEBUG
				Console.WriteLine("[CssCache] Loaded reset.css:");
				Console.WriteLine(resetCss);
#endif
			}
		}
	}

	public static void AddClassSelector(string css)
	{
		var cssClass = $".{css}";
		if (_cache.ContainsKey(cssClass))
			return;

		if (_cache.TryAdd(cssClass, 0))
		{
			Console.WriteLine($"Adding CSS:\n{cssClass}");
			_lock.EnterWriteLock();
			try
			{
				_allCss.AppendLine(cssClass);
				_provider.LoadFromData(_allCss.ToString(), _allCss.Length);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
	}

	public static void AddElementSelector(string css)
	{
		if (_cache.ContainsKey(css))
			return;

		if (_cache.TryAdd(css, 0))
		{
			Console.WriteLine($"Adding CSS:\n{css}");
			_lock.EnterWriteLock();
			try
			{
				_allCss.AppendLine(css);
				_provider.LoadFromData(_allCss.ToString(), _allCss.Length);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
	}

	public static string GetInstanceClass(Gtk.Widget widget)
	{
		return $"{Prefix}{widget.Handle.DangerousGetHandle()}";
	}
}