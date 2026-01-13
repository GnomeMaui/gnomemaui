using Gtk;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform;

public class NavigationStack
{
	readonly Gtk.Box _container;
	readonly Stack<Gtk.Widget> _stack = new Stack<Gtk.Widget>();

	public NavigationStack(Gtk.Box container)
	{
		_container = container;
	}

	public Task Push(Gtk.Widget page, bool animated)
	{
		if (_stack.Count > 0)
		{
			var current = _stack.Peek();
			current.Unparent();
		}

		_stack.Push(page);
		page.SetHexpand(true);
		page.SetVexpand(true);
		_container.Append(page);
		page.Show();

		// TODO: Add animation support if required
		return Task.CompletedTask;
	}

	public Task Pop(bool animated)
	{
		if (_stack.Count == 0)
		{
			return Task.CompletedTask;
		}

		var page = _stack.Pop();
		page.Unparent();

		if (_stack.Count > 0)
		{
			var previous = _stack.Peek();
			_container.Append(previous);
			previous.SetHexpand(true);
			previous.SetVexpand(true);
			previous.Show();
		}

		// TODO: Add animation support
		return Task.CompletedTask;
	}

	public void Pop(Gtk.Widget page)
	{
		if (_stack.Contains(page))
		{
			// Remove from stack
			var tempStack = new Stack<Gtk.Widget>();
			while (_stack.Count > 0)
			{
				var item = _stack.Pop();
				if (item != page)
				{
					tempStack.Push(item);
				}
			}

			while (tempStack.Count > 0)
			{
				_stack.Push(tempStack.Pop());
			}

			page.Unparent();
		}
	}

	public void Insert(Gtk.Widget before, Gtk.Widget page)
	{
		// Insert page into stack before the specified item
		var tempStack = new Stack<Gtk.Widget>();
		bool found = false;

		while (_stack.Count > 0)
		{
			var item = _stack.Pop();
			tempStack.Push(item);
			if (item == before)
			{
				tempStack.Push(page);
				found = true;
			}
		}

		if (!found)
		{
			tempStack.Push(page);
		}

		while (tempStack.Count > 0)
		{
			_stack.Push(tempStack.Pop());
		}
	}
}
