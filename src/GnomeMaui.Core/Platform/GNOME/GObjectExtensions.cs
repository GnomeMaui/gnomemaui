using System.Runtime.InteropServices;

namespace Microsoft.Maui.Platform;

internal static class GObjectExtensions
{
	// Set object
	internal static void SetManagedData<T>(this GObject.Object gobject, string key, T data) where T : class
	{
		var handle = GCHandle.Alloc(data);
		var ptr = GCHandle.ToIntPtr(handle);
		gobject.SetData(key, ptr);
	}

	// Get object
	internal static T? GetManagedData<T>(this GObject.Object gobject, string key) where T : class
	{
		var ptr = gobject.GetData(key);

		if (ptr == IntPtr.Zero)
			return null;

		var handle = GCHandle.FromIntPtr(ptr);
		return handle.Target as T;
	}

	// Cleanup
	internal static void RemoveManagedData(this GObject.Object gobject, string key)
	{
		var ptr = gobject.GetData(key);

		if (ptr != IntPtr.Zero)
		{
			var handle = GCHandle.FromIntPtr(ptr);
			handle.Free();
			gobject.SetData(key, IntPtr.Zero);
		}
	}
}
