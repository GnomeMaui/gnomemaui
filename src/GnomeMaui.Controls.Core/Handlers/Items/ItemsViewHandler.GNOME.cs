#nullable disable
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Microsoft.Maui.Controls.Handlers.Items;

public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, Gtk.Widget> where TItemsView : ItemsView
{
	protected Gtk.ScrolledWindow ScrolledWindow;
	protected Gtk.Widget ListView; // Gtk.ListView or Gtk.GridView
	protected Gio.ListStore ItemsModel;
	protected Gtk.ListItemFactory ItemFactory;
	private ListItemFactoryManager FactoryManager;
	protected Gtk.Widget EmptyViewWidget;
	protected View FormsEmptyView;
	protected bool EmptyViewDisplayed;
	protected IEnumerable ItemsSource;
	private readonly List<ItemWrapper> _itemWrappers = new();
	private Gtk.SelectionModel _selectionModel;

	protected TItemsView ItemsView => VirtualView;
	protected abstract IItemsLayout Layout { get; }

	protected override Gtk.Widget CreatePlatformView()
	{
		ScrolledWindow = Gtk.ScrolledWindow.New();
		ScrolledWindow.Hexpand = true;
		ScrolledWindow.Vexpand = true;
		ScrolledWindow.Valign = Gtk.Align.Fill;
		ScrolledWindow.Halign = Gtk.Align.Fill;
		ListView = SelectListViewBase();

		ScrolledWindow.SetChild(ListView);
		ListView.Show();
		ScrolledWindow.Show();

		return ScrolledWindow;
	}

	protected override void ConnectHandler(Gtk.Widget platformView)
	{
		base.ConnectHandler(platformView);
		VirtualView.ScrollToRequested += ScrollToRequested;
	}

	protected override void DisconnectHandler(Gtk.Widget platformView)
	{
		VirtualView.ScrollToRequested -= ScrollToRequested;
		FactoryManager?.Dispose();
		FactoryManager = null;
		CleanUpItemsSource();
		_selectionModel?.Dispose();
		_selectionModel = null;
		FormsEmptyView?.Handler?.DisconnectHandler();
		base.DisconnectHandler(platformView);
	}

	public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateItemsSource();
	}

	public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateHorizontalScrollBarVisibility();
	}

	public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateVerticalScrollBarVisibility();
	}

	public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateItemTemplate();
	}

	public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateEmptyView();
	}

	public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateEmptyView();
	}

	public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		// TODO: Implement flow direction
	}

	public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.PlatformView.SetVisible(itemsView.IsVisible);
	}

	public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateItemsUpdatingScrollMode();
	}

	void UpdateItemsUpdatingScrollMode()
	{
		// Behavior is handled in OnCollectionChanged (we react to adds/removes there).
		// Keep this method as the platform mapping entry point in case
		// we need to add/remove platform-specific subscriptions later.
	}

	protected abstract Gtk.Widget SelectListViewBase();

	protected virtual void UpdateItemsSource()
	{
		if (ListView == null)
			return;

		CleanUpItemsSource();

		if (ItemsView.ItemsSource == null)
			return;

		ItemsModel = Gio.ListStore.New(GObject.Internal.Object.GetGType());

		// Populate ItemsModel from ItemsView.ItemsSource
		int position = 0;
		foreach (var item in ItemsView.ItemsSource)
		{
			var wrapper = ItemWrapper.Create(item, position);
			_itemWrappers.Add(wrapper);
			ItemsModel.Append(wrapper.NativeObject);
			position++;
		}

		// Subscribe to collection changes
		if (ItemsView.ItemsSource is INotifyCollectionChanged observable)
		{
			observable.CollectionChanged += OnCollectionChanged;
		}

		// Apply model to ListView (only create selection model once)
		if (_selectionModel == null)
		{
			_selectionModel = Gtk.SingleSelection.New(ItemsModel);

			if (ListView is Gtk.ListView listView)
			{
				listView.Model = _selectionModel;
			}
			else if (ListView is Gtk.GridView gridView)
			{
				gridView.Model = _selectionModel;
			}
		}
		else
		{
			// Update existing selection model with new ItemsModel
			if (_selectionModel is Gtk.SingleSelection singleSelection)
			{
				singleSelection.Model = ItemsModel;
			}
		}
		UpdateEmptyViewVisibility();
	}

	protected virtual void UpdateItemTemplate()
	{
		if (ItemsView == null || ListView == null)
			return;

		// Dispose old factory
		FactoryManager?.Dispose();
		FactoryManager = null;

		// Create new factory
		FactoryManager = new ListItemFactoryManager(ItemsView, MauiContext);
		ItemFactory = FactoryManager.CreateFactory();

		// Apply factory to ListView
		if (ListView is Gtk.ListView listView)
		{
			listView.Factory = ItemFactory;
		}
		else if (ListView is Gtk.GridView gridView)
		{
			gridView.Factory = ItemFactory;
		}
	}

	protected virtual void UpdateEmptyView()
	{
		if (ItemsView == null || ListView == null)
			return;

		var emptyView = ItemsView.EmptyView;
		if (emptyView == null)
			return;

		switch (emptyView)
		{
			case string text:
				EmptyViewWidget = Gtk.Label.New(text);
				break;
			case View view:
				EmptyViewWidget = RealizeEmptyView(view);
				break;
			default:
				EmptyViewWidget = RealizeEmptyViewTemplate(emptyView, ItemsView.EmptyViewTemplate);
				break;
		}

		UpdateEmptyViewVisibility();
	}

	protected virtual void UpdateEmptyViewVisibility()
	{
		uint itemCount = ItemsModel?.GetNItems() ?? 0;
		bool isEmpty = itemCount == 0;

		if (isEmpty)
		{
			if (FormsEmptyView != null)
			{
				if (EmptyViewDisplayed)
					ItemsView.RemoveLogicalChild(FormsEmptyView);

				if (ItemsView.EmptyViewTemplate == null)
					ItemsView.AddLogicalChild(FormsEmptyView);
			}

			if (EmptyViewWidget != null)
			{
				EmptyViewWidget.SetVisible(true);
				// TODO: Add EmptyViewWidget to container as overlay
			}

			EmptyViewDisplayed = true;
		}
		else
		{
			if (EmptyViewDisplayed)
			{
				if (EmptyViewWidget != null)
					EmptyViewWidget.SetVisible(false);

				ItemsView.RemoveLogicalChild(FormsEmptyView);
			}

			EmptyViewDisplayed = false;
		}
	}

	protected virtual void CleanUpItemsSource()
	{
		// Unsubscribe from collection changes
		if (ItemsSource is INotifyCollectionChanged observable)
		{
			observable.CollectionChanged -= OnCollectionChanged;
		}

		// Dispose all item wrappers
		foreach (var wrapper in _itemWrappers)
		{
			wrapper.Dispose();
		}
		_itemWrappers.Clear();

		if (ItemsModel != null)
		{
			ItemsModel.Dispose();
			ItemsModel = null;
		}

		// Don't dispose selection model here, reuse it
		// _selectionModel will be updated with new ItemsModel

		ItemsSource = null;
	}

	void UpdateVerticalScrollBarVisibility()
	{
		if (ScrolledWindow == null)
			return;

		switch (ItemsView.VerticalScrollBarVisibility)
		{
			case ScrollBarVisibility.Always:
				ScrolledWindow.VscrollbarPolicy = Gtk.PolicyType.Always;
				break;
			case ScrollBarVisibility.Never:
				ScrolledWindow.VscrollbarPolicy = Gtk.PolicyType.Never;
				break;
			case ScrollBarVisibility.Default:
				ScrolledWindow.VscrollbarPolicy = Gtk.PolicyType.Automatic;
				break;
		}
	}

	void UpdateHorizontalScrollBarVisibility()
	{
		if (ScrolledWindow == null)
			return;

		switch (ItemsView.HorizontalScrollBarVisibility)
		{
			case ScrollBarVisibility.Always:
				ScrolledWindow.HscrollbarPolicy = Gtk.PolicyType.Always;
				break;
			case ScrollBarVisibility.Never:
				ScrolledWindow.HscrollbarPolicy = Gtk.PolicyType.Never;
				break;
			case ScrollBarVisibility.Default:
				ScrolledWindow.HscrollbarPolicy = Gtk.PolicyType.Automatic;
				break;
		}
	}

	Gtk.Widget RealizeEmptyViewTemplate(object bindingContext, DataTemplate emptyViewTemplate)
	{
		if (emptyViewTemplate == null)
		{
			return Gtk.Label.New(bindingContext?.ToString() ?? string.Empty);
		}

		var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
		var view = template.CreateContent() as View;
		view.BindingContext = bindingContext;

		return RealizeEmptyView(view);
	}

	Gtk.Widget RealizeEmptyView(View view)
	{
		FormsEmptyView = view ?? throw new ArgumentNullException(nameof(view));

		var handler = view.ToHandler(MauiContext);
		var platformView = handler.PlatformView as Gtk.Widget;

		return platformView;
	}

	protected virtual void UpdateItemsLayout()
	{
		UpdateItemTemplate();
		// Don't call UpdateItemsSource() here - it's already called by MapItemsSource
		// and we don't want to recreate the items when layout changes
		UpdateHorizontalScrollBarVisibility();
		UpdateVerticalScrollBarVisibility();
		UpdateEmptyView();
	}

	void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
	{
		// Map ScrollToRequested to platform scroll.
		if (args == null)
			return;

		if (args.Mode == ScrollToMode.Position)
		{
			int index = args.Index;
			ScrollToIndex(index);
			return;
		}

		// Item-based scrolling: try to find index of item
		if (args.Item != null)
		{
			for (int i = 0; i < _itemWrappers.Count; i++)
			{
				if (object.Equals(_itemWrappers[i].Item, args.Item))
				{
					ScrollToIndex(i);
					return;
				}
			}
		}
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (ItemsModel == null)
			return;

		switch (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
				if (e.NewItems != null)
				{
					int position = e.NewStartingIndex;
					foreach (var item in e.NewItems)
					{
						var wrapper = ItemWrapper.Create(item, position);
						_itemWrappers.Insert(position, wrapper);
						ItemsModel.Insert((uint)position, wrapper.NativeObject);
						position++;
					}
					// Update positions for items after insertion
					UpdatePositions(e.NewStartingIndex + e.NewItems.Count);

					// Honor ItemsUpdatingScrollMode: when items are added, keep either the
					// first or last item in view depending on the selected mode.
					if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
					{
						ScrollToIndex(0);
					}
					else if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
					{
						int last = (int)(ItemsModel?.GetNItems() ?? 0u) - 1;
						if (last >= 0)
							ScrollToIndex(last);
					}
				}
				break;

			case NotifyCollectionChangedAction.Remove:
				if (e.OldItems != null)
				{
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						int index = e.OldStartingIndex;
						if (index >= 0 && index < _itemWrappers.Count)
						{
							var wrapper = _itemWrappers[index];
							_itemWrappers.RemoveAt(index);
							wrapper.Dispose();
							ItemsModel.Remove((uint)index);
						}
					}
					// Update positions for remaining items
					UpdatePositions(e.OldStartingIndex);
				}
				break;

			case NotifyCollectionChangedAction.Replace:
				if (e.NewItems != null && e.NewStartingIndex >= 0)
				{
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						int index = e.NewStartingIndex + i;
						if (index < _itemWrappers.Count)
						{
							// Dispose old wrapper
							var oldWrapper = _itemWrappers[index];
							oldWrapper.Dispose();

							// Create new wrapper
							var newWrapper = ItemWrapper.Create(e.NewItems[i], index);
							_itemWrappers[index] = newWrapper;

							// Replace in model
							ItemsModel.Remove((uint)index);
							ItemsModel.Insert((uint)index, newWrapper.NativeObject);
						}
					}
				}
				break;

			case NotifyCollectionChangedAction.Move:
				if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0)
				{
					var wrapper = _itemWrappers[e.OldStartingIndex];
					_itemWrappers.RemoveAt(e.OldStartingIndex);
					_itemWrappers.Insert(e.NewStartingIndex, wrapper);

					// GTK doesn't have a move operation, so remove and insert
					ItemsModel.Remove((uint)e.OldStartingIndex);
					ItemsModel.Insert((uint)e.NewStartingIndex, wrapper.NativeObject);

					// Update positions
					int startIndex = Math.Min(e.OldStartingIndex, e.NewStartingIndex);
					UpdatePositions(startIndex);
				}
				break;

			case NotifyCollectionChangedAction.Reset:
				CleanUpItemsSource();
				UpdateItemsSource();
				break;
		}

		UpdateEmptyViewVisibility();
	}

	private void UpdatePositions(int startIndex)
	{
		for (int i = startIndex; i < _itemWrappers.Count; i++)
		{
			// Note: ItemWrapper.Position is read-only, so we need to recreate
			// the wrapper with the new position if we want to update it.
			// For now, we'll leave positions as-is since they're mainly for debugging.
		}
	}


	private void ScrollToIndex(int index)
	{
		if (ListView == null)
			return;

		if (index < 0)
			return;

		MainThread.BeginInvokeOnMainThread(() =>
		{
			if (ListView is Gtk.ListView listView)
			{
				try
				{
					listView.ScrollTo((uint)index, Gtk.ListScrollFlags.None, null);
				}
				catch { }
			}
			else if (ListView is Gtk.GridView gridView)
			{
				try
				{
					gridView.ScrollTo((uint)index, Gtk.ListScrollFlags.None, null);
				}
				catch { }
			}
			else
			{
				var vadj = ScrolledWindow?.GetVadjustment();
				if (vadj != null)
				{
					var maxV = vadj.GetUpper() - vadj.GetPageSize();
					var target = index == 0 ? 0 : Math.Min(maxV, vadj.GetUpper());
					vadj.SetValue(target);
				}
			}
		});
	}
}
