using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Platform;

public class ShellSectionView : Gtk.Box
{
	Gtk.Notebook? _topTabBar;

	ShellContent? _currentContent;
	Gtk.Widget? _currentView;

	int _lastSelected = 0;
	IList<ShellContent>? _cachedContents;

	Dictionary<IView, Gtk.Widget> _pageMap = new Dictionary<IView, Gtk.Widget>();
	Dictionary<IView, IViewHandler?> _handlerMap = new Dictionary<IView, IViewHandler?>();

	protected ShellSection ShellSection { get; private set; }
	protected IMauiContext? MauiContext { get; set; }

	public ShellSectionView(ShellSection section, IMauiContext context)
	{
		SetOrientation(Gtk.Orientation.Vertical);
		SetSpacing(0);
		ShellSection = section;
		MauiContext = context;

		SetHexpand(true);
		SetVexpand(true);

		UpdateCurrentItem();

		ShellSection.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				UpdateCurrentItem();
			}
		};
	}

	protected void CreateTabBar()
	{
		if (ShellSection.Items.Count <= 1 || !IsItemChanged())
		{
			return;
		}

		if (_topTabBar == null)
		{
			_topTabBar = Gtk.Notebook.New();
			_topTabBar.SetHexpand(true);
			_topTabBar.OnSwitchPage += OnTabItemSelected;
			Prepend(_topTabBar);
		}

		// Clear existing tabs
		while (_topTabBar.GetNPages() > 0)
		{
			_topTabBar.RemovePage(0);
		}

		// Add tabs for each shell content
		foreach (var content in ShellSection.Items)
		{
			var label = Gtk.Label.New(content.Title);
			var contentBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

			_topTabBar.AppendPage(contentBox, label);
		}

		_cachedContents = ShellSection.Items;

		if (_lastSelected >= 0 && _lastSelected < _topTabBar.GetNPages())
		{
			_topTabBar.SetCurrentPage(_lastSelected);
		}
	}

	void OnTabItemSelected(Gtk.Notebook sender, Gtk.Notebook.SwitchPageSignalArgs args)
	{
		var pageNum = (int)args.PageNum;

		if (pageNum == _lastSelected)
		{
			return;
		}

		_lastSelected = pageNum;

		if (pageNum < ShellSection.Items.Count)
		{
			ShellSection.CurrentItem = ShellSection.Items[pageNum];
		}
	}

	void UpdateCurrentItem()
	{
		CreateTabBar();
		UpdateContent(ShellSection.CurrentItem);
	}

	void UpdateContent(ShellContent? shellContent)
	{
		if (shellContent == null)
		{
			return;
		}

		if (_currentContent == shellContent)
		{
			return;
		}

		if (_currentView != null)
		{
			Remove(_currentView);
		}

		_currentView = GetShellContentView(shellContent);
		Append(_currentView);

		_currentContent = shellContent;
	}

	bool IsItemChanged()
	{
		var contents = ShellSection.Items;
		if (_cachedContents == null)
		{
			return true;
		}

		if (_cachedContents.Count != contents.Count)
		{
			return true;
		}

		for (int i = 0; i < contents.Count; i++)
		{
			if (_cachedContents[i] != contents[i])
			{
				return true;
			}
		}

		return false;
	}

	Gtk.Widget GetShellContentView(ShellContent shellContent)
	{
		var page = ((IShellContentController)shellContent).GetOrCreateContent();

		if (_pageMap.ContainsKey(page))
		{
			return _pageMap[page];
		}
		else
		{
			var content = page.ToPlatform(MauiContext!);
			_pageMap[page] = content;
			_handlerMap[page] = page.Handler;
			return content;
		}
	}

	public override void Dispose()
	{
		_topTabBar?.OnSwitchPage -= OnTabItemSelected;

		foreach (var view in _pageMap.Values)
		{
			view.Dispose();
		}
		_pageMap.Clear();
		_handlerMap.Clear();
		base.Dispose();
		GC.SuppressFinalize(this);
	}
}
