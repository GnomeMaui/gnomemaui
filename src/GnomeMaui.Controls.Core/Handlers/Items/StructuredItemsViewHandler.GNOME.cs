#nullable disable
using Gtk;
using Microsoft.Maui.Controls.Platform;
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		View _currentHeader;
		View _currentFooter;
		WeakNotifyPropertyChangedProxy _layoutPropertyChangedProxy;
		PropertyChangedEventHandler _layoutPropertyChanged;
		Gtk.Box _containerBox;

		~StructuredItemsViewHandler() => _layoutPropertyChangedProxy?.Unsubscribe();

		protected override IItemsLayout Layout => ItemsView?.ItemsLayout;

		protected override Gtk.Widget CreatePlatformView()
		{
			// Create a vertical box to hold header, list view, and footer
			_containerBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

			ScrolledWindow = Gtk.ScrolledWindow.New();
			ListView = SelectListViewBase();

			ScrolledWindow.SetChild(ListView);
			_containerBox.Append(ScrolledWindow);

			return _containerBox;
		}

		protected override void ConnectHandler(Gtk.Widget platformView)
		{
			base.ConnectHandler(platformView);

			if (Layout is not null)
			{
				_layoutPropertyChanged ??= LayoutPropertyChanged;
				_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
			}
			else
			{
				_layoutPropertyChangedProxy?.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		protected override void DisconnectHandler(Gtk.Widget platformView)
		{
			base.DisconnectHandler(platformView);

			_layoutPropertyChangedProxy?.Unsubscribe();
			_layoutPropertyChangedProxy = null;
		}

		void LayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GridItemsLayout.SpanProperty.PropertyName)
				UpdateItemsLayoutSpan();
			else if (e.PropertyName == GridItemsLayout.HorizontalItemSpacingProperty.PropertyName ||
					e.PropertyName == GridItemsLayout.VerticalItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
			else if (e.PropertyName == LinearItemsLayout.ItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
		}

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeader();
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateFooter();
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateItemsLayout();
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			// TODO: Implement item sizing strategy
		}

		protected override Gtk.Widget SelectListViewBase()
		{
			switch (VirtualView.ItemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical:
					return CreateVerticalListView(listItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					return CreateHorizontalListView(listItemsLayout);
			}

			throw new NotImplementedException("The layout is not implemented");
		}

		protected virtual void UpdateHeader()
		{
			if (_containerBox == null)
				return;

			if (_currentHeader != null)
			{
				ItemsView.RemoveLogicalChild(_currentHeader);
				_currentHeader = null;
			}

			var header = ItemsView.Header ?? ItemsView.HeaderTemplate;

			// Remove existing header widget if any
			// TODO: Track and remove previous header widget

			switch (header)
			{
				case null:
					// No header
					break;

				case string text:
					var headerLabel = Gtk.Label.New(text);
					_containerBox.Prepend(headerLabel);
					break;

				case View view:
					_currentHeader = view;
					ItemsView.AddLogicalChild(_currentHeader);
					var headerHandler = view.ToHandler(MauiContext);
					var headerWidget = headerHandler.PlatformView as Gtk.Widget;
					if (headerWidget != null)
						_containerBox.Prepend(headerWidget);
					break;

				default:
					var headerTemplate = ItemsView.HeaderTemplate;
					if (headerTemplate != null)
					{
						var templateView = headerTemplate.CreateContent() as View;
						templateView.BindingContext = header;
						var headerHandler2 = templateView.ToHandler(MauiContext);
						var headerWidget2 = headerHandler2.PlatformView as Gtk.Widget;
						if (headerWidget2 != null)
							_containerBox.Prepend(headerWidget2);
					}
					break;
			}
		}

		protected virtual void UpdateFooter()
		{
			if (_containerBox == null)
				return;

			if (_currentFooter != null)
			{
				ItemsView.RemoveLogicalChild(_currentFooter);
				_currentFooter = null;
			}

			var footer = ItemsView.Footer ?? ItemsView.FooterTemplate;

			// Remove existing footer widget if any
			// TODO: Track and remove previous footer widget

			switch (footer)
			{
				case null:
					// No footer
					break;

				case string text:
					var footerLabel = Gtk.Label.New(text);
					_containerBox.Append(footerLabel);
					break;

				case View view:
					_currentFooter = view;
					ItemsView.AddLogicalChild(_currentFooter);
					var footerHandler = view.ToHandler(MauiContext);
					var footerWidget = footerHandler.PlatformView as Gtk.Widget;
					if (footerWidget != null)
						_containerBox.Append(footerWidget);
					break;

				default:
					var footerTemplate = ItemsView.FooterTemplate;
					if (footerTemplate != null)
					{
						var templateView = footerTemplate.CreateContent() as View;
						templateView.BindingContext = footer;
						var footerHandler2 = templateView.ToHandler(MauiContext);
						var footerWidget2 = footerHandler2.PlatformView as Gtk.Widget;
						if (footerWidget2 != null)
							_containerBox.Append(footerWidget2);
					}
					break;
			}
		}

		static Gtk.Widget CreateGridView(GridItemsLayout gridItemsLayout)
		{
			var gridView = Gtk.GridView.New(null, null);
			gridView.Hexpand = true;
			gridView.Vexpand = true;
			gridView.Halign = Gtk.Align.Fill;
			gridView.Valign = Gtk.Align.Fill;

			// TODO: Configure grid view orientation and span
			// gridView.SetOrientation(gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal 
			//     ? Gtk.Orientation.Horizontal 
			//     : Gtk.Orientation.Vertical);

			return gridView;
		}

		static Gtk.Widget CreateVerticalListView(LinearItemsLayout listItemsLayout)
		{
			var listView = Gtk.ListView.New(null, null);
			listView.SetOrientation(Gtk.Orientation.Vertical);
			listView.Hexpand = true;
			listView.Vexpand = true;
			listView.Halign = Gtk.Align.Fill;
			listView.Valign = Gtk.Align.Fill;
			return listView;
		}

		static Gtk.Widget CreateHorizontalListView(LinearItemsLayout listItemsLayout)
		{
			var listView = Gtk.ListView.New(null, null);
			listView.SetOrientation(Gtk.Orientation.Horizontal);
			listView.Hexpand = true;
			listView.Vexpand = true;
			listView.Halign = Gtk.Align.Fill;
			listView.Valign = Gtk.Align.Fill;
			return listView;
		}

		void UpdateItemsLayoutSpan()
		{
			// TODO: Update GridView span property
		}

		void UpdateItemsLayoutItemSpacing()
		{
			// TODO: Update item spacing based on layout type
		}
	}
}
