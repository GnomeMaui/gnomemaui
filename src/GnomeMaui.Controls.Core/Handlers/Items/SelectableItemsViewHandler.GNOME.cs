#nullable disable

namespace Microsoft.Maui.Controls.Handlers.Items;

public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
{
	bool _ignorePlatformSelectionChange;
	bool _ignoreVirtualSelectionChange;
	Gtk.SelectionModel _selectionModel;

	protected override Gtk.Widget CreatePlatformView()
	{
		var widget = base.CreatePlatformView();
		UpdateSelectionModel();
		return widget;
	}

	protected override void ConnectHandler(Gtk.Widget platformView)
	{
		base.ConnectHandler(platformView);

		if (ItemsView != null)
		{
			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}

		if (_selectionModel != null)
		{
			_selectionModel.OnSelectionChanged += PlatformSelectionChanged;
		}

		UpdatePlatformSelection();
	}

	protected override void DisconnectHandler(Gtk.Widget platformView)
	{
		if (_selectionModel != null)
		{
			_selectionModel.OnSelectionChanged -= PlatformSelectionChanged;
		}

		if (ItemsView != null)
		{
			ItemsView.SelectionChanged -= VirtualSelectionChanged;
		}

		base.DisconnectHandler(platformView);
	}

	public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
	{
		handler.UpdatePlatformSelection();
	}

	public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
	{
		handler.UpdatePlatformSelection();
	}

	public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
	{
		handler.UpdateSelectionModel();
	}

	void UpdateSelectionModel()
	{
		if (ItemsModel == null || ListView == null)
			return;

		// Disconnect old selection model
		if (_selectionModel != null)
		{
			_selectionModel.OnSelectionChanged -= PlatformSelectionChanged;
		}

		// Create appropriate selection model based on SelectionMode
		switch (ItemsView.SelectionMode)
		{
			case SelectionMode.None:
				_selectionModel = Gtk.NoSelection.New(ItemsModel);
				break;
			case SelectionMode.Single:
				_selectionModel = Gtk.SingleSelection.New(ItemsModel);
				break;
			case SelectionMode.Multiple:
				_selectionModel = Gtk.MultiSelection.New(ItemsModel);
				break;
		}

		// Connect new selection model
		if (_selectionModel != null)
		{
			_selectionModel.OnSelectionChanged += PlatformSelectionChanged;
		}

		// Apply selection model to ListView or GridView
		if (ListView is Gtk.ListView listView)
		{
			listView.Model = _selectionModel;
		}
		else if (ListView is Gtk.GridView gridView)
		{
			gridView.Model = _selectionModel;
		}

		UpdatePlatformSelection();
	}

	void UpdatePlatformSelection()
	{
		_ignorePlatformSelectionChange = true;

		if (_selectionModel == null || ItemsView == null)
		{
			_ignorePlatformSelectionChange = false;
			return;
		}

		switch (ItemsView.SelectionMode)
		{
			case SelectionMode.None:
				break;
			case SelectionMode.Single:
				if (_selectionModel is Gtk.SingleSelection singleSelection)
				{
					if (ItemsView.SelectedItem == null)
					{
						singleSelection.SetSelected(Gtk.Constants.INVALID_LIST_POSITION);
					}
					else
					{
						// TODO: Find the index of the selected item in ItemsModel
						// and call singleSelection.SetSelected(index);
					}
				}
				break;
			case SelectionMode.Multiple:
				if (_selectionModel is Gtk.MultiSelection multiSelection)
				{
					// TODO: Update multiple selection
					// multiSelection.UnselectAll();
					// foreach (var selectedItem in ItemsView.SelectedItems)
					// {
					//     var index = FindItemIndex(selectedItem);
					//     if (index != Gtk.Internal.Constants.GTK_INVALID_LIST_POSITION)
					//         multiSelection.SelectItem(index, false);
					// }
				}
				break;
		}

		_ignorePlatformSelectionChange = false;
	}

	void VirtualSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_ignoreVirtualSelectionChange)
		{
			_ignoreVirtualSelectionChange = false;
			return;
		}
		UpdatePlatformSelection();
	}

	void PlatformSelectionChanged(Gtk.SelectionModel sender, Gtk.SelectionModel.SelectionChangedSignalArgs args)
	{
		UpdateVirtualSelection();
	}

	void UpdateVirtualSelection()
	{
		if (_ignorePlatformSelectionChange || ItemsView == null || _selectionModel == null)
			return;

		switch (ItemsView.SelectionMode)
		{
			case SelectionMode.None:
				break;
			case SelectionMode.Single:
				UpdateVirtualSingleSelection();
				break;
			case SelectionMode.Multiple:
				UpdateVirtualMultipleSelection();
				break;
		}
	}

	void UpdateVirtualSingleSelection()
	{
		if (_selectionModel is Gtk.SingleSelection singleSelection)
		{
			var selectedPosition = singleSelection.GetSelected();
			if (selectedPosition == Gtk.Constants.INVALID_LIST_POSITION)
			{
				_ignoreVirtualSelectionChange = true;
				ItemsView.SelectedItem = null;
				_ignoreVirtualSelectionChange = false;
			}
			else
			{
				var itemPtr = ItemsModel?.GetItem(selectedPosition);
				if (itemPtr.HasValue && itemPtr.Value != IntPtr.Zero)
				{
					var gobject = (GObject.Object)GObject.Internal.InstanceWrapper.WrapHandle<GObject.Object>(itemPtr.Value, false);
					var itemWrapper = ItemWrapper.FromGObject(gobject);
					if (itemWrapper != null)
					{
						_ignoreVirtualSelectionChange = true;
						ItemsView.SelectedItem = itemWrapper.Item;
						_ignoreVirtualSelectionChange = false;
					}
				}
			}
		}
	}

	void UpdateVirtualMultipleSelection()
	{
		ItemsView.SelectionChanged -= VirtualSelectionChanged;

		if (_selectionModel is Gtk.MultiSelection multiSelection)
		{
			var selection = new List<object>();
			// TODO: Iterate through selected items in multiSelection
			// and add them to the selection list

			ItemsView.UpdateSelectedItems(selection);
		}

		ItemsView.SelectionChanged += VirtualSelectionChanged;
	}

	protected override void UpdateItemsLayout()
	{
		_ignorePlatformSelectionChange = true;
		base.UpdateItemsLayout();
		_ignorePlatformSelectionChange = false;
	}

	protected override void UpdateItemsSource()
	{
		_ignorePlatformSelectionChange = true;
		base.UpdateItemsSource();
		UpdateSelectionModel();
		_ignorePlatformSelectionChange = false;
	}
}
