#nullable disable
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Handlers.Items;

public partial class ReorderableItemsViewHandler<TItemsView> : GroupableItemsViewHandler<TItemsView> where TItemsView : ReorderableItemsView
{
	Gtk.DragSource _dragSource;
	Gtk.DropTarget _dropTarget;

	protected override void ConnectHandler(Gtk.Widget platformView)
	{
		base.ConnectHandler(platformView);

		// TODO: Set up drag source and drop target signals
		// if (_dragSource != null)
		// {
		//     _dragSource.OnDragBegin += HandleDragBegin;
		//     _dragSource.OnDragEnd += HandleDragEnd;
		// }
		// if (_dropTarget != null)
		// {
		//     _dropTarget.OnDrop += HandleDrop;
		// }
	}

	protected override void DisconnectHandler(Gtk.Widget platformView)
	{
		// TODO: Disconnect drag and drop signals
		// if (_dragSource != null)
		// {
		//     _dragSource.OnDragBegin -= HandleDragBegin;
		//     _dragSource.OnDragEnd -= HandleDragEnd;
		//     _dragSource.Dispose();
		//     _dragSource = null;
		// }
		// if (_dropTarget != null)
		// {
		//     _dropTarget.OnDrop -= HandleDrop;
		//     _dropTarget.Dispose();
		//     _dropTarget = null;
		// }

		base.DisconnectHandler(platformView);
	}

	public static void MapCanReorderItems(ReorderableItemsViewHandler<TItemsView> handler, ReorderableItemsView itemsView)
	{
		handler.UpdateCanReorderItems();
	}

	void UpdateCanReorderItems()
	{
		if (ItemsView == null || ListView == null)
			return;

		if (ItemsView.CanReorderItems)
		{
			// Built in reordering only supports ungrouped sources & observable collections.
			var supportsReorder = !ItemsView.IsGrouped && ItemsView.ItemsSource is INotifyCollectionChanged;

			if (supportsReorder)
			{
				// Create drag source and drop target
				// _dragSource = Gtk.DragSource.New();
				// _dropTarget = Gtk.DropTarget.New(GObject.Internal.Object.GetGType(), Gdk.DragAction.Move);

				// TODO: Add drag source and drop target to ListView
				// ListView.AddController(_dragSource);
				// ListView.AddController(_dropTarget);
			}
		}
		else
		{
			// Remove drag and drop controllers
			if (_dragSource != null)
			{
				_dragSource.Dispose();
				_dragSource = null;
			}
			if (_dropTarget != null)
			{
				_dropTarget.Dispose();
				_dropTarget = null;
			}
		}
	}

	void HandleDragBegin()
	{
		// TODO: Handle drag begin
	}

	void HandleDragEnd()
	{
		// TODO: Handle drag end and send reorder completed event
		ItemsView?.SendReorderCompleted();
	}

	void HandleDrop()
	{
		// TODO: Handle drop and reorder items in ItemsSource
	}
}
