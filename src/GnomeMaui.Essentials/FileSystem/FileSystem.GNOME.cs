using Microsoft.Maui.ApplicationModel;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Storage;

partial class FileSystemImplementation : IFileSystem
{
	string PlatformCacheDirectory
	{
		get
		{
			var cache = GLib.Functions.GetUserCacheDir();
			if (!string.IsNullOrEmpty(cache))
			{
				var appCache = Path.Combine(cache, AppInfo.PackageName);
				Directory.CreateDirectory(appCache);
				return appCache;
			}

			// Fallback
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var fallbackCache = Path.Combine(home, ".cache", AppInfo.PackageName);
			Directory.CreateDirectory(fallbackCache);
			return fallbackCache;
		}
	}

	string PlatformAppDataDirectory
	{
		get
		{
			var dataDir = GLib.Functions.GetUserDataDir();
			if (!string.IsNullOrEmpty(dataDir))
			{
				var appData = Path.Combine(dataDir, AppInfo.PackageName);
				Directory.CreateDirectory(appData);
				return appData;
			}

			// Fallback
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var fallbackData = Path.Combine(home, ".local", "share", AppInfo.PackageName);
			Directory.CreateDirectory(fallbackData);
			return fallbackData;
		}
	}

	Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
	{
		var file = PlatformGetFullAppPackageFilePath(filename);
		return Task.FromResult((Stream)File.OpenRead(file));
	}

	Task<bool> PlatformAppPackageFileExistsAsync(string filename)
	{
		var file = PlatformGetFullAppPackageFilePath(filename);
		return Task.FromResult(File.Exists(file));
	}

	static string PlatformGetFullAppPackageFilePath(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
			throw new ArgumentNullException(nameof(filename));

		filename = NormalizePath(filename);

		// On Linux, app package files are in the application's base directory
		return Path.Combine(AppContext.BaseDirectory, filename);
	}

	static string NormalizePath(string filename) =>
		filename
			.Replace('\\', Path.DirectorySeparatorChar)
			.Replace('/', Path.DirectorySeparatorChar);
}

public partial class FileBase
{
	static string PlatformGetContentType(string extension)
	{
		return MimeTypes.GetMimeType(extension);
	}

	internal void Init(FileBase file)
	{
		FullPath = file.FullPath;
		ContentType = file.ContentType;
		FileName = file.FileName;
	}

	internal virtual Task<Stream> PlatformOpenReadAsync()
	{
		return Task.FromResult((Stream)File.OpenRead(FullPath));
	}

	void PlatformInit(FileBase file)
	{
		Init(file);
	}
}
