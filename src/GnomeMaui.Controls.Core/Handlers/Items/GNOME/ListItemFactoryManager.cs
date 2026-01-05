#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	/// <summary>
	/// Manages GTK4 SignalListItemFactory for MAUI ItemTemplate rendering
	/// </summary>
	internal class ListItemFactoryManager : IDisposable
	{
		private readonly ItemsView _itemsView;
		private readonly IMauiContext _mauiContext;
		private Gtk.SignalListItemFactory _factory;
		private readonly Dictionary<Gtk.ListItem, View> _itemViews = new();
		private readonly Dictionary<Gtk.ListItem, IPlatformViewHandler> _itemHandlers = new();
		private bool _disposed;

		public ListItemFactoryManager(ItemsView itemsView, IMauiContext mauiContext)
		{
			_itemsView = itemsView ?? throw new ArgumentNullException(nameof(itemsView));
			_mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
		}

		public Gtk.ListItemFactory CreateFactory()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(ListItemFactoryManager));

			_factory = Gtk.SignalListItemFactory.New();

			// Setup: Create widget structure (once per reusable cell)
			_factory.OnSetup += OnSetup;

			// Bind: Connect data to widget (each display)
			_factory.OnBind += OnBind;

			// Unbind: Clear data when scrolled out
			_factory.OnUnbind += OnUnbind;

			// Teardown: Cleanup
			_factory.OnTeardown += OnTeardown;

			return _factory;
		}

		private void OnSetup(object sender, Gtk.SignalListItemFactory.SetupSignalArgs args)
		{
			var listItem = args.Object as Gtk.ListItem;
			if (listItem == null)
				return;

#if DEBUG
			Console.Out.WriteLine($"[ListItemFactoryManager][OnSetup] Position: {listItem.Position}");
#endif

			// Create a container widget for the MAUI view
			var container = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
			listItem.Child = container;
		}

		private void OnBind(object sender, Gtk.SignalListItemFactory.BindSignalArgs args)
		{
			var listItem = args.Object as Gtk.ListItem;
			if (listItem == null)
				return;

			var container = listItem.Child as Gtk.Box;
			if (container == null)
				return;

			// Get the data item from GObject
			var gobject = listItem.Item as GObject.Object;
			if (gobject == null)
				return;

			var itemWrapper = ItemWrapper.FromGObject(gobject);
			if (itemWrapper == null)
				return;

			var dataItem = itemWrapper.Item;

#if DEBUG
			Console.Out.WriteLine(new StringBuilder()
				.AppendLine($"[ListItemFactoryManager][OnBind]")
				.AppendLine($" - Position: {listItem.Position}")
				.AppendLine($" - Data: {dataItem}")
				.ToString());
#endif

			// Check if we need to create a new view or reuse existing
			View mauiView = null;
			IPlatformViewHandler handler = null;

			if (_itemViews.TryGetValue(listItem, out var existingView))
			{
				// Reuse existing view, just update binding context
				mauiView = existingView;
				handler = _itemHandlers[listItem];

				if (mauiView.BindingContext != dataItem)
				{
					mauiView.BindingContext = dataItem;
				}
			}
			else
			{
				// Create new view from template
				mauiView = CreateViewFromTemplate(dataItem);
				if (mauiView == null)
					return;

				// Create native handler
				handler = mauiView.ToHandler(_mauiContext);
				var widget = handler.PlatformView as Gtk.Widget;

				if (widget != null)
				{
					container.Append(widget);
				}

				// Cache for reuse
				_itemViews[listItem] = mauiView;
				_itemHandlers[listItem] = handler;

				// Add to logical tree
				_itemsView.AddLogicalChild(mauiView);
			}
		}

		private void OnUnbind(object sender, Gtk.SignalListItemFactory.UnbindSignalArgs args)
		{
			var listItem = args.Object as Gtk.ListItem;
			if (listItem == null)
				return;

#if DEBUG
			Console.Out.WriteLine($"[ListItemFactoryManager][OnUnbind] Position: {listItem.Position}");
#endif

			// Optionally clear binding context when scrolled out
			if (_itemViews.TryGetValue(listItem, out var view))
			{
				// Don't clear binding context here, we'll reuse the view
				// view.BindingContext = null;
			}
		}

		private void OnTeardown(object sender, Gtk.SignalListItemFactory.TeardownSignalArgs args)
		{
			var listItem = args.Object as Gtk.ListItem;
			if (listItem == null)
				return;

#if DEBUG
			Console.Out.WriteLine($"[ListItemFactoryManager][OnTeardown] Position: {listItem.Position}");
#endif

			// Remove from logical tree
			if (_itemViews.TryGetValue(listItem, out var view))
			{
				_itemsView.RemoveLogicalChild(view);
			}

			// Cleanup
			if (_itemHandlers.TryGetValue(listItem, out var handler))
			{
				handler?.DisconnectHandler();
			}

			_itemViews.Remove(listItem);
			_itemHandlers.Remove(listItem);

			// Clear container
			var container = listItem.Child as Gtk.Box;
			if (container != null)
			{
				// Remove all children
				var child = container.GetFirstChild();
				while (child != null)
				{
					var next = child.GetNextSibling();
					container.Remove(child);
					child = next;
				}
			}

			listItem.Child = null;
		}

		private View CreateViewFromTemplate(object dataItem)
		{
			if (_itemsView.ItemTemplate == null)
			{
				// No template, create default Label
				return new Label
				{
					Text = dataItem?.ToString() ?? string.Empty,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Start
				};
			}

			// Use DataTemplate
			var template = _itemsView.ItemTemplate.SelectDataTemplate(dataItem, _itemsView);
			var content = template.CreateContent();

			if (content is View view)
			{
				view.BindingContext = dataItem;
				return view;
			}

			return null;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			if (_factory != null)
			{
				_factory.OnSetup -= OnSetup;
				_factory.OnBind -= OnBind;
				_factory.OnUnbind -= OnUnbind;
				_factory.OnTeardown -= OnTeardown;
				_factory.Dispose();
				_factory = null;
			}

			// Cleanup all cached items
			foreach (var handler in _itemHandlers.Values)
			{
				handler?.DisconnectHandler();
			}

			_itemViews.Clear();
			_itemHandlers.Clear();
		}
	}
}
