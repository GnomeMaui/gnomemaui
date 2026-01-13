using System.Collections.Specialized;
using System.Text;

namespace Microsoft.Maui.Controls.Platform;

public class ShellItemView : Gtk.Box
{
	Gtk.Notebook? _tabBar;
	bool _isTabBarVisible;
	int _lastSelected = 0;
	bool _isHandlerConnected = false;

	readonly Lock _updateLock = new();
	readonly Gtk.Stack _sectionStack;
	readonly Dictionary<ShellSection, Gtk.Widget> _shellSectionViewCache = [];

	protected Shell Shell { get; private set; } = default!;
	protected ShellItem ShellItem { get; private set; } = default!;
	protected IMauiContext MauiContext { get; private set; } = default!;
	protected IShellItemController ShellItemController => (ShellItem as IShellItemController)!;

	public ShellItemView(ShellItem item, IMauiContext context)
	{
		ArgumentNullException.ThrowIfNull(item);
		ArgumentNullException.ThrowIfNull(context);

		SetOrientation(Gtk.Orientation.Vertical);
		SetSpacing(0);
		ShellItem = item;
		MauiContext = context;
		Shell = (Shell)item.Parent;

		_isTabBarVisible = true;

		SetHexpand(true);
		SetVexpand(true);

		_sectionStack = Gtk.Stack.New();
		_sectionStack.SetVexpand(true);
		_sectionStack.SetHexpand(true);
		// We can set transition if we want
		// _sectionStack.SetTransitionType(Gtk.StackTransitionType.SlideLeft);
		Append(_sectionStack);

		if (ShellItem?.Items is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged += OnShellItemsCollectionChanged;
		}
	}

	public void UpdateTabBar(bool isVisible)
	{
		lock (_updateLock)
		{
			// Prevent signal loop
			EnsureHandlerDisconnected();

			if (isVisible)
			{
				EnsureTabBar();

				// Restore selection
				if (_tabBar != null && ShellItem.CurrentItem != null)
				{
					var idx = ShellItem.Items.IndexOf(ShellItem.CurrentItem);
					if (idx >= 0 && idx < _tabBar.GetNPages())
					{
						_tabBar.SetCurrentPage(idx);
						_lastSelected = idx;
					}
				}

				EnsureHandlerConnected();
			}
			else
			{
				HideTabBar();
			}

			_isTabBarVisible = isVisible;
		}
	}

	public void UpdateCurrentItem(ShellSection? section)
	{
		if (section == null)
		{
			return;
		}

		lock (_updateLock)
		{
			// Temporarily disconnect the switch-page signal to prevent interference
			EnsureHandlerDisconnected();

			Gtk.Widget? targetView = null;
			if (_shellSectionViewCache.ContainsKey(section))
			{
				targetView = _shellSectionViewCache[section];
			}
			else
			{
				// We still create a Box wrapper, or append directly. Existing code used a wrapper box.
				var wrapper = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
				var sectionPlatformView = section.ToPlatform(MauiContext);
				wrapper.Append(sectionPlatformView);

				// Add to stack
				_sectionStack.AddChild(wrapper);

				// Ensure visibility after adding to stack
				wrapper.Show();
				sectionPlatformView.Show();

				_shellSectionViewCache[section] = wrapper;
				targetView = wrapper;
			}

			if (targetView != null)
			{
				_sectionStack.SetVisibleChild(targetView);
			}

			var selectedIdx = ShellItem.Items.IndexOf(section);
			_lastSelected = selectedIdx < 0 ? 0 : selectedIdx;

			if (_tabBar != null)
			{
				_tabBar.SetCurrentPage(selectedIdx);
			}

			// No need to append anything to 'this' anymore, _sectionStack is already there.

			if (targetView != null)
			{
				// If wrapper
				if (targetView is Gtk.Box box)
				{
					var child = box.GetFirstChild();
					if (child != null)
					{
						child.QueueResize();
					}
				}
				targetView.QueueResize();
			}

		}

		// Reconnect the switch-page signal OUTSIDE the lock to avoid deadlock
		EnsureHandlerConnected();
	}

	void OnTabItemSelected(Gtk.Notebook sender, Gtk.Notebook.SwitchPageSignalArgs args)
	{
		if (!_updateLock.TryEnter())
		{
			return;
		}

		try
		{
			var pageNum = (int)args.PageNum;


			if (pageNum == _lastSelected)
			{
				return;
			}

			_lastSelected = pageNum;

			if (pageNum < ShellItem.Items.Count)
			{
				var shellSection = ShellItem.Items[pageNum];
				Shell.CurrentItem = shellSection;
			}
		}
		finally
		{
			_updateLock.Exit();
		}
	}

	void OnShellItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		lock (_updateLock)
		{
			if (_isTabBarVisible)
			{
				EnsureHandlerDisconnected();
				RepopulateTabBar();
				EnsureHandlerConnected();
			}
		}
	}

	void EnsureTabBar()
	{
		if (!ShellItemController.ShowTabs)
		{
			return;
		}

		if (_tabBar == null)
		{
			_tabBar = Gtk.Notebook.New();
			_tabBar.SetHexpand(true);
			_tabBar.SetVexpand(true);
			Prepend(_tabBar);

			RepopulateTabBar();
		}
	}

	void RepopulateTabBar()
	{
		if (_tabBar == null) return;

		// Clear existing
		while (_tabBar.GetNPages() > 0)
		{
			_tabBar.RemovePage(0);
		}

		// Add tabs for each shell section
		foreach (var section in ShellItem.Items)
		{
			var label = Gtk.Label.New(section.Title);
			var content = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

			_tabBar.AppendPage(content, label);
		}
	}

	void HideTabBar()
	{
		if (_tabBar != null)
		{
			EnsureHandlerDisconnected();
			Remove(_tabBar);
			_tabBar = null;
		}
	}

	void EnsureHandlerConnected()
	{
		if (_tabBar != null && !_isHandlerConnected)
		{
			_tabBar.OnSwitchPage += OnTabItemSelected;
			_isHandlerConnected = true;
		}
	}

	void EnsureHandlerDisconnected()
	{
		if (_tabBar != null && _isHandlerConnected)
		{
			_tabBar.OnSwitchPage -= OnTabItemSelected;
			_isHandlerConnected = false;
		}
	}

	public override void Dispose()
	{
		if (ShellItem.Items is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged -= OnShellItemsCollectionChanged;
		}

		EnsureHandlerDisconnected();

		foreach (var cachedView in _shellSectionViewCache.Values)
		{
			cachedView.Dispose();
		}
		_shellSectionViewCache.Clear();

		base.Dispose();
		GC.SuppressFinalize(this);
	}
}
