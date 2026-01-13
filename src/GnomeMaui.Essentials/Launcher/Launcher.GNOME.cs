using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		Task<bool> PlatformCanOpenAsync(Uri uri)
		{
			if (uri == null)
				return Task.FromResult(false);

			var scheme = uri.Scheme;
			if (string.IsNullOrEmpty(scheme))
				return Task.FromResult(false);

			var appInfo = Gio.AppInfoHelper.GetDefaultForUriScheme(scheme);
			return Task.FromResult(appInfo != null);
		}

		async Task<bool> PlatformOpenAsync(Uri uri)
		{
			if (uri == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();
			var launcher = Gtk.UriLauncher.New(uri.AbsoluteUri);

			try
			{
				Gio.Internal.AsyncReadyCallback callback = (sourceObject, res, userData) =>
				{
					try
					{
						var result = (Gio.AsyncResult)GObject.Internal.InstanceWrapper.WrapHandle<Gio.AsyncResultHelper>(res, false);
						var success = launcher.LaunchFinish(result);
						tcs.SetResult(success);
					}
					catch (GLib.GException ex)
					{
						tcs.SetResult(false);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				};

				Gtk.Internal.UriLauncher.Launch(
					launcher.Handle.DangerousGetHandle(),
					IntPtr.Zero,
					IntPtr.Zero,
					callback,
					IntPtr.Zero
				);

				return await tcs.Task;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		async Task<bool> PlatformOpenAsync(OpenFileRequest request)
		{
			if (request?.File == null || string.IsNullOrEmpty(request.File.FullPath))
				return false;

			var tcs = new TaskCompletionSource<bool>();
			var file = Gio.FileHelper.NewForPath(request.File.FullPath);
			var launcher = Gtk.FileLauncher.New(file);

			try
			{
				Gio.Internal.AsyncReadyCallback callback = (sourceObject, res, userData) =>
				{
					try
					{
						var result = (Gio.AsyncResult)GObject.Internal.InstanceWrapper.WrapHandle<Gio.AsyncResultHelper>(res, false);
						var success = launcher.LaunchFinish(result);
						tcs.SetResult(success);
					}
					catch (GLib.GException ex)
					{
						tcs.SetResult(false);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				};

				Gtk.Internal.FileLauncher.Launch(
					launcher.Handle.DangerousGetHandle(),
					IntPtr.Zero,
					IntPtr.Zero,
					callback,
					IntPtr.Zero
				);

				return await tcs.Task;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		async Task<bool> PlatformTryOpenAsync(Uri uri)
		{
			var canOpen = await PlatformCanOpenAsync(uri);

			if (canOpen)
				return await PlatformOpenAsync(uri);

			return false;
		}
	}
}
