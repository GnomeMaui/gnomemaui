using Gtk;
using Microsoft.Maui.Controls.Internals;
using System.Collections;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellFlyoutItemAdaptor
	{
		Shell _shell;
		bool _hasHeader;
		IEnumerable _items;

		public ShellFlyoutItemAdaptor(Shell shell, IEnumerable items, bool hasHeader)
		{
			_shell = shell;
			_items = items;
			_hasHeader = hasHeader;
		}

		public Gtk.ListBox CreateListBox()
		{
			var listBox = Gtk.ListBox.New();
			listBox.SetSelectionMode(Gtk.SelectionMode.Single);

			if (_hasHeader && ((IShellController)_shell).FlyoutHeader is View headerView)
			{
				var headerRow = Gtk.ListBoxRow.New();
				var headerPlatform = headerView.ToPlatform(_shell.Handler?.MauiContext!);
				headerRow.SetChild(headerPlatform);
				headerRow.SetSelectable(false);
				listBox.Append(headerRow);
			}

			foreach (var item in _items)
			{
				if (item is Element element)
				{
					var row = CreateFlyoutRow(element);
					listBox.Append(row);
				}
			}

			return listBox;
		}

		Gtk.ListBoxRow CreateFlyoutRow(Element element)
		{
			var row = Gtk.ListBoxRow.New();

			var box = Gtk.Box.New(Gtk.Orientation.Horizontal, 12);
			box.SetMarginStart(12);
			box.SetMarginEnd(12);
			box.SetMarginTop(8);
			box.SetMarginBottom(8);

			if (element is BaseShellItem shellItem)
			{
				// Icon
				if (shellItem.Icon != null)
				{
					var icon = Gtk.Image.New();
					icon.SetPixelSize(24);
					// TODO: Load icon from ImageSource
					box.Append(icon);
				}

				// Title
				var label = Gtk.Label.New(shellItem.Title);
				label.SetHexpand(true);
				label.SetXalign(0);
				box.Append(label);
			}

			row.SetChild(box);
			return row;
		}
	}
}
