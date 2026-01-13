namespace DrawnUi.Infrastructure
{
	public partial class Files
	{
		public static void RefreshSystem(FileDescriptor file)
		{
			// GNOME/Linux doesn't require special system refresh for file changes
			// The file system is automatically updated
		}

		public static string GetPublicDirectory()
		{
			var downloads = GLib.Functions.GetUserSpecialDir(GLib.UserDirectory.DirectoryDownload);

			if (!string.IsNullOrEmpty(downloads) && Directory.Exists(downloads))
				return downloads;

			// Fallback to user home directory
			return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		}

		public static List<string> ListAssets(string sub)
		{
			// List files from Resources/Raw subfolder
			var assets = new List<string>();

			try
			{
				var basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Raw");
				if (!string.IsNullOrEmpty(sub))
					basePath = Path.Combine(basePath, sub);

				if (Directory.Exists(basePath))
				{
					assets.AddRange(Directory.GetFiles(basePath)
						.Select(Path.GetFileName)
						.Where(f => f != null)
						.Select(f => f!));
				}
			}
			catch (Exception e)
			{
				Super.Log(e);
			}

			return assets;
		}

		public static void Share(string message, IEnumerable<string> fullFilenames)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
			});
		}
	}
}
