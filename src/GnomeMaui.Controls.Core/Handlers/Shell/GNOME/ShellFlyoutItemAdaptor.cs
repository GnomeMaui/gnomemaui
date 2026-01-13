using System.Collections;
using System.Text;

namespace Microsoft.Maui.Controls.Platform;

class ShellFlyoutItemAdaptor
{
	readonly Shell _shell;
	readonly bool _hasHeader;
	readonly IEnumerable _items;

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

		// Remove existing child first to avoid "widget already has parent" error
		var existingChild = row.GetChild();
		if (existingChild != null)
		{
			row.SetChild(null);
		}

		// Check if box has a parent - should NEVER happen for freshly created widget
		if (box.GetParent() != null)
		{
			return row; // Return empty row to avoid GTK violation
		}

		row.SetChild(box);
		return row;
	}
}
