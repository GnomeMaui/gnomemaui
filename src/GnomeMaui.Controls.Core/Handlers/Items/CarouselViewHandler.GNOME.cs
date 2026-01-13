#nullable disable
using Microsoft.Maui.Controls.Internals;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>, IDisposable
	{
		private Adw.Carousel _carousel;
		private bool _isUpdatingPosition;
		private bool _isUpdatingFromNative;

		protected override Gtk.Widget CreatePlatformView()
		{
			_carousel = Adw.Carousel.New();
			_carousel.Hexpand = true;
			_carousel.Vexpand = true;

			// Connect to position changed signal
			_carousel.OnNotify += (sender, args) => OnCarouselPropertyChanged(args);

			return _carousel;
		}

		protected override void ConnectHandler(Gtk.Widget platformView)
		{
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Gtk.Widget platformView)
		{
			// Signal handlers will be disconnected when the widget is disposed
			base.DisconnectHandler(platformView);
		}

		protected override Gtk.Widget SelectListViewBase()
		{
			// CarouselView doesn't use ListView/GridView like ItemsView
			// Items are added directly to the Carousel widget
			return _carousel;
		}

		protected override IItemsLayout Layout => VirtualView?.ItemsLayout;

		protected override void UpdateItemsSource()
		{
			if (_carousel == null || VirtualView == null)
				return;

			// Remove all existing pages
			while (_carousel.GetNPages() > 0)
			{
				var page = _carousel.GetNthPage(0);
				_carousel.Remove(page);
			}

			// Clear wrappers
			CleanUpItemsSource();

			if (VirtualView.ItemsSource == null)
				return;

			// Add new pages
			int position = 0;
			foreach (var item in VirtualView.ItemsSource)
			{
				var itemView = CreateItemView(item, position);
				if (itemView != null)
				{
					_carousel.Append(itemView);
				}
				position++;
			}

			// Subscribe to collection changes
			if (VirtualView.ItemsSource is INotifyCollectionChanged observable)
			{
				observable.CollectionChanged += OnCollectionChanged;
			}

			// Update current position
			UpdatePositionFromVirtualView();
		}

		private Gtk.Widget CreateItemView(object item, int position)
		{
			if (VirtualView.ItemTemplate == null)
				return null;

			// Create the View from template
			var content = VirtualView.ItemTemplate.CreateContent();
			View view = null;

			if (content is View v)
			{
				view = v;
			}

			if (view == null)
				return null;

			// Set binding context
			view.BindingContext = item;

			// Create handler and get native widget
			var handler = view.ToHandler(MauiContext);
			if (handler?.PlatformView is Gtk.Widget widget)
			{
				widget.Show();
				return widget;
			}

			return null;
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_carousel == null)
				return;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null)
					{
						int index = e.NewStartingIndex;
						foreach (var item in e.NewItems)
						{
							var itemView = CreateItemView(item, index);
							if (itemView != null)
							{
								_carousel.Insert(itemView, index);
							}
							index++;
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null)
					{
						foreach (var _ in e.OldItems)
						{
							if (_carousel.GetNPages() > e.OldStartingIndex)
							{
								var page = _carousel.GetNthPage((uint)e.OldStartingIndex);
								_carousel.Remove(page);
							}
						}
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					UpdateItemsSource();
					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.NewItems != null && e.NewItems.Count > 0)
					{
						// Remove old and insert new
						var oldPage = _carousel.GetNthPage((uint)e.NewStartingIndex);
						_carousel.Remove(oldPage);

						var newView = CreateItemView(e.NewItems[0], e.NewStartingIndex);
						if (newView != null)
						{
							_carousel.Insert(newView, e.NewStartingIndex);
						}
					}
					break;
			}
		}

		private void OnCarouselPropertyChanged(GObject.Object.NotifySignalArgs args)
		{
			if (_isUpdatingPosition || _isUpdatingFromNative)
				return;

			if (args.Pspec.GetName() == "position")
			{
				UpdatePositionToVirtualView();
			}
		}

		private void UpdatePositionFromVirtualView()
		{
			if (_carousel == null || VirtualView == null || _isUpdatingFromNative)
				return;

			_isUpdatingPosition = true;

			int position = VirtualView.Position;
			if (position >= 0 && position < _carousel.GetNPages())
			{
				var widget = _carousel.GetNthPage((uint)position);
				_carousel.ScrollTo(widget, VirtualView.IsScrollAnimated);
			}

			_isUpdatingPosition = false;
		}

		private void UpdatePositionToVirtualView()
		{
			if (_carousel == null || VirtualView == null || _isUpdatingPosition)
				return;

			_isUpdatingFromNative = true;

			double nativePosition = _carousel.GetPosition();
			int position = (int)Math.Round(nativePosition);

			if (VirtualView.Position != position)
			{
				VirtualView.Position = position;

				// Update CurrentItem based on position
				if (VirtualView.ItemsSource != null)
				{
					var itemsList = VirtualView.ItemsSource.Cast<object>().ToList();
					if (position >= 0 && position < itemsList.Count)
					{
						VirtualView.CurrentItem = itemsList[position];
					}
				}
			}

			_isUpdatingFromNative = false;
		}

		private void UpdateCurrentItemFromVirtualView()
		{
			if (_carousel == null || VirtualView == null || VirtualView.ItemsSource == null)
				return;

			var currentItem = VirtualView.CurrentItem;
			if (currentItem == null)
				return;

			// Find the index of CurrentItem in ItemsSource
			int index = 0;
			foreach (var item in VirtualView.ItemsSource)
			{
				if (item == currentItem)
				{
					VirtualView.Position = index;
					UpdatePositionFromVirtualView();
					return;
				}
				index++;
			}
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateCurrentItemFromVirtualView();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdatePositionFromVirtualView();
		}

		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			// AdwCarousel doesn't have a direct bounce property
			// The behavior is controlled by SpringParams
			// We could adjust the ScrollParams here if needed
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler._carousel != null)
			{
				handler._carousel.Interactive = carouselView.IsSwipeEnabled;
			}
		}

		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler._carousel != null)
			{
				// PeekAreaInsets can be approximated using Spacing
				// Convert Thickness to spacing (use the larger of left/right for horizontal)
				var insets = carouselView.PeekAreaInsets;
				uint spacing = (uint)Math.Max(Math.Max(insets.Left, insets.Right),
											  Math.Max(insets.Top, insets.Bottom));
				handler._carousel.Spacing = spacing;
			}
		}

		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView)
		{
			// AdwCarousel doesn't have native loop/wrap support
			// This would require custom implementation to wrap around
			// For now, we'll note that Loop is not directly supported
		}

		public void Dispose()
		{
			if (VirtualView?.ItemsSource is INotifyCollectionChanged observable)
			{
				observable.CollectionChanged -= OnCollectionChanged;
			}
			GC.SuppressFinalize(this);
		}
	}
}