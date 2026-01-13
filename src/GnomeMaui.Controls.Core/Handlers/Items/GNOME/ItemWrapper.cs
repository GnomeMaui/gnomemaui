#nullable disable
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Controls.Handlers.Items;

/// <summary>
/// Wrapper class to store MAUI data items with GObject.Object for use in Gio.ListStore
/// Uses GObject SetData/GetData with GCHandle to attach managed objects to GObject instances
/// </summary>
internal class ItemWrapper : IDisposable
{
	private const string ItemDataKey = "maui-item-data";
	private const string PositionDataKey = "maui-position-data";

	private GObject.Object _gobject;
	private GCHandle _itemHandle;
	private bool _disposed;

	private ItemWrapper(GObject.Object gobject, object item, int position)
	{
		_gobject = gobject;

		// Pin the managed object to prevent GC and get an IntPtr
		_itemHandle = GCHandle.Alloc(new ItemData { Item = item, Position = position });

		// Store the GCHandle as IntPtr in GObject user data
		_gobject.SetData(ItemDataKey, GCHandle.ToIntPtr(_itemHandle));
	}

	public GObject.Object NativeObject => _gobject;

	public object Item
	{
		get
		{
			if (_disposed || !_itemHandle.IsAllocated)
				return null;

			var data = _itemHandle.Target as ItemData;
			return data?.Item;
		}
	}

	public int Position
	{
		get
		{
			if (_disposed || !_itemHandle.IsAllocated)
				return -1;

			var data = _itemHandle.Target as ItemData;
			return data?.Position ?? -1;
		}
	}

	public static ItemWrapper Create(object item, int position)
	{
		// Create a new GObject.Object instance to hold our data
		var gobject = GObject.Object.NewWithProperties(GObject.Object.GetGType(), [], []);
		return new ItemWrapper(gobject, item, position);
	}

	public static ItemWrapper FromGObject(GObject.Object gobject)
	{
		if (gobject == null)
			return null;

		// Try to retrieve the GCHandle from GObject user data
		var dataPtr = gobject.GetData(ItemDataKey);
		if (dataPtr == IntPtr.Zero)
			return null;

		try
		{
			var handle = GCHandle.FromIntPtr(dataPtr);
			if (!handle.IsAllocated)
				return null;

			var data = handle.Target as ItemData;
			if (data == null)
				return null;

			// Create a new wrapper that references this GObject
			return new ItemWrapper(gobject, data.Item, data.Position) { _itemHandle = handle };
		}
		catch
		{
			return null;
		}
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		// Free the GCHandle to allow GC
		if (_itemHandle.IsAllocated)
		{
			_itemHandle.Free();
		}

		// Clear the data from GObject
		if (_gobject != null)
		{
			_gobject.SetData(ItemDataKey, IntPtr.Zero);
		}
	}

	private class ItemData
	{
		public object Item { get; set; }
		public int Position { get; set; }
	}
}
