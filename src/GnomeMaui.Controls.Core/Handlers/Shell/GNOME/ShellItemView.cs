using Gtk;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellItemView : Gtk.Box
	{
		Gtk.Box? _currentSectionView;
		Gtk.Notebook? _tabBar;
		bool _isTabBarVisible;
		int _lastSelected = 0;

		Dictionary<ShellSection, Gtk.Box> _shellSectionViewCache = new Dictionary<ShellSection, Gtk.Box>();

		protected Shell Shell { get; private set; }
		protected ShellItem ShellItem { get; private set; }
		protected IMauiContext MauiContext { get; private set; }
		protected IShellItemController ShellItemController => (ShellItem as IShellItemController)!;

		public ShellItemView(ShellItem item, IMauiContext context)
		{
			SetOrientation(Gtk.Orientation.Vertical);
			SetSpacing(0);
			ShellItem = item;
			MauiContext = context;
			Shell = (Shell)item.Parent;

			_isTabBarVisible = true;

			SetHexpand(true);
			SetVexpand(true);

			if (ShellItem.Items is INotifyCollectionChanged notifyCollectionChanged)
			{
				notifyCollectionChanged.CollectionChanged += OnShellItemsCollectionChanged;
			}

			UpdateTabBar(true);
		}

		public void UpdateTabBar(bool isVisible)
		{
			if (isVisible)
			{
				ShowTabBar();
			}
			else
			{
				HideTabBar();
			}

			_isTabBarVisible = isVisible;
		}

		public void UpdateCurrentItem(ShellSection? section)
		{
			if (section == null)
			{
				return;
			}

			if (_currentSectionView != null)
			{
				Remove(_currentSectionView);
				_currentSectionView = null;
			}

			if (_shellSectionViewCache.ContainsKey(section))
			{
				_currentSectionView = _shellSectionViewCache[section];
			}
			else
			{
				_currentSectionView = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
				var sectionPlatformView = section.ToPlatform(MauiContext);
				_currentSectionView.Append(sectionPlatformView);
				try
				{ sectionPlatformView.Show(); }
				catch { }
				_shellSectionViewCache[section] = _currentSectionView;
			}

			var selectedIdx = ShellItem.Items.IndexOf(section);
			_lastSelected = selectedIdx < 0 ? 0 : selectedIdx;

			if (_tabBar != null)
			{
				_tabBar.SetCurrentPage(selectedIdx);
			}

			Append(_currentSectionView);
			_currentSectionView.Show();

			var child = _currentSectionView.GetFirstChild();
			if (child != null)
			{
				child.QueueResize();
				_currentSectionView.QueueResize();
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

			if (pageNum < ShellItem.Items.Count)
			{
				var shellSection = ShellItem.Items[pageNum];
				Shell.CurrentItem = shellSection;
			}
		}

		void OnShellItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTabBar(_isTabBarVisible);
		}

		void ShowTabBar()
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
				_tabBar.OnSwitchPage += OnTabItemSelected;
				Prepend(_tabBar);
			}

			// Clear existing tabs
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

			if (_lastSelected >= 0 && _lastSelected < _tabBar.GetNPages())
			{
				_tabBar.SetCurrentPage(_lastSelected);
			}
		}

		void HideTabBar()
		{
			if (_tabBar != null)
			{
				Remove(_tabBar);
				_tabBar = null;
			}
		}

		public override void Dispose()
		{
			if (ShellItem.Items is INotifyCollectionChanged notifyCollectionChanged)
			{
				notifyCollectionChanged.CollectionChanged -= OnShellItemsCollectionChanged;
			}

			if (_tabBar != null)
			{
				_tabBar.OnSwitchPage -= OnTabItemSelected;
			}

			foreach (var cachedView in _shellSectionViewCache.Values)
			{
				cachedView.Dispose();
			}
			_shellSectionViewCache.Clear();
		}
	}
}
