using Adw;
using Gtk;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform;

public class ShellView : Gtk.Box, IFlyoutBehaviorObserver
{
	public readonly GColor DefaultBackgroundColor = new GColor(1f, 1f, 1f, 1f);

	Adw.OverlaySplitView _splitView;
	Gtk.Box? _sidebarBox;
	Gtk.Box? _contentBox;

	// Navbar/HeaderBar components
	Adw.ToolbarView? _sidebarToolbarView;
	Adw.HeaderBar? _sidebarHeaderBar;
	Adw.ToolbarView? _contentToolbarView;
	Adw.HeaderBar? _contentHeaderBar;
	Gtk.Box? _contentInnerBox;
	Gtk.ToggleButton _flyoutToggleButton = default!;
	Gtk.Button? _sidebarCloseButton;

	View? _headerView;
	FlyoutHeaderBehavior _headerBehavior;

	IView? _flyoutView;

	ShellItemHandler? _currentItemHandler;
	SearchHandler? _currentSearchHandler;
	Page? _currentPage;

	bool _isOpen;
	bool _isUpdating;
	bool _initialSelected;

	Gtk.ListBox? _flyoutListBox;
	Element? _currentSelectedItem;
	List<List<Element>> _cachedGroups = new List<List<Element>>();
	List<Element> _items = new List<Element>();

	protected Shell? Element { get; set; }

	protected IShellController ShellController => (Element as IShellController)!;

	protected IMauiContext? MauiContext { get; private set; }

	protected bool HeaderOnMenu => _headerBehavior == FlyoutHeaderBehavior.Scroll || _headerBehavior == FlyoutHeaderBehavior.CollapseOnScroll;

	public event EventHandler? Toggled;

	public ShellView()
	{
		SetOrientation(Gtk.Orientation.Vertical);
		SetSpacing(0);
		SetHexpand(true);
		SetVexpand(true);

		// Create the overlay split view
		_splitView = Adw.OverlaySplitView.New();

		// Sidebar setup - directly without NavigationPage wrapper
		_sidebarBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

		// Sidebar setup - with ToolbarView wrapper (like Navbar in TestGnome1)
		_sidebarBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

		// Create ToolbarView + HeaderBar for sidebar
		_sidebarToolbarView = Adw.ToolbarView.New();
		_sidebarHeaderBar = Adw.HeaderBar.New();
		_sidebarHeaderBar.SetTitleWidget(Gtk.Label.New("Menu"));
		_sidebarToolbarView.AddTopBar(_sidebarHeaderBar);
		_sidebarToolbarView.SetContent(_sidebarBox);

		_splitView.SetSidebar(_sidebarToolbarView);

		// Content setup - with ToolbarView wrapper
		_contentBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

		// Create ToolbarView + HeaderBar for content
		_contentToolbarView = Adw.ToolbarView.New();
		_contentHeaderBar = Adw.HeaderBar.New();
		_contentToolbarView.AddTopBar(_contentHeaderBar);

		// Create inner box for actual content
		_contentInnerBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
		_contentToolbarView.SetContent(_contentInnerBox);

		_contentBox.Append(_contentToolbarView);
		_splitView.SetContent(_contentBox);

		// Enable gesture support
		_splitView.SetEnableShowGesture(true);
		_splitView.SetEnableHideGesture(true);

		// Initial state - sidebar OPEN by default (like TestGnome1 reference)
		_splitView.SetShowSidebar(true);
		_splitView.SetCollapsed(false);
		_isOpen = true; // Sync internal state BEFORE creating toggle button

		// Create and add toggle button AFTER split view initialization
		_flyoutToggleButton = CreateFlyoutToggleButton();
		_contentHeaderBar.PackStart(_flyoutToggleButton);

		// Create sidebar close button (visible only in mobile mode)
		_sidebarCloseButton = CreateSidebarCloseButton();
		_sidebarHeaderBar.PackStart(_sidebarCloseButton);

		// Monitor show-sidebar property changes
		// REF: Program.cs, sor 59-84
		// shell.Native.OnNotify += (obj, prop) =>
		// {
		//     var name = prop.Pspec.GetName();
		//     if (name == "show-sidebar")
		//     {
		//         shellToggleButton.Active = shell.ShowSidebar;
		//         shell.IsUpdating = false;
		//     }
		//     else if (!initialSelected && (name == "root" || name == "parent"))
		//     {
		//         flyoutItems.SelectRow(flyoutItems.GetRowAtIndex(0));
		//         initialSelected = true;
		//     }
		// };
		_splitView.OnNotify += (sender, args) =>
		{
			var name = args.Pspec.GetName();
#if DEBUG
			Console.Out.WriteLine($"---- [Shell][OnNotify] Property Changed: {name}");
#endif

			if (name == "show-sidebar")
			{
				// keep the toggle button state in sync when the property changes
				if (_flyoutToggleButton != null)
				{
					_flyoutToggleButton.Active = ShowSidebar;
				}
#if DEBUG
				Console.Out.WriteLine($"---- [Shell][OnNotify] IsUpdating before clear: {_isUpdating}");
#endif
				// programmatic update finished
				_isUpdating = false;
#if DEBUG
				Console.Out.WriteLine($"---- [Shell][OnNotify] IsUpdating after clear: {_isUpdating}");
#endif
#if DEBUG
				Console.Out.WriteLine(new StringBuilder()
					.AppendLine($"[Shell][OnNotify][show-sidebar]")
					.AppendLine($" - Shell ShowSidebar: {ShowSidebar}")
					.ToString());
#endif
			}
			else if (!_initialSelected && (name == "root" || name == "parent"))
			{
				// Select initial row only once when the widget is attached to the
				// widget hierarchy (root/parent notify) to avoid selecting on every
				// collapse/expand.
				if (_flyoutListBox != null)
				{
					_flyoutListBox.SelectRow(_flyoutListBox.GetRowAtIndex(0));
					_initialSelected = true;
				}
			}
		};

		// Add split view to this container
		Append(_splitView);
		_splitView.Show();
		_contentBox.QueueResize();
	}

	public bool IsOpened
	{
		get => _splitView?.GetShowSidebar() ?? false;
		set
		{
			if (_splitView != null && _isOpen != value)
			{
				_splitView.SetShowSidebar(value);
				_isOpen = value;
			}
		}
	}

	public bool ShowSidebar
	{
		get => _splitView?.GetShowSidebar() ?? false;
		set => _splitView?.SetShowSidebar(value);
	}

	// Adw.OverlaySplitView Shell.cs: public bool Collapsed { get => _splitView?.Collapsed ?? true; set => _splitView?.Collapsed = value; }
	public bool Collapsed
	{
		get => _splitView?.GetCollapsed() ?? false;
		set => _splitView?.SetCollapsed(value);
	}

	Gtk.ToggleButton CreateFlyoutToggleButton()
	{
		// REF: ShellToggleButton.cs, sor 10-14
		// SetIconName("menu_new-symbolic");
		// SetTooltipText("Toggle Sidebar");
		// SetActive(true);
		// SetVisible(true);
		// OnClicked += ShellToggleButton_OnClicked;
		var toggleButton = Gtk.ToggleButton.New();
		toggleButton.SetIconName("menu_new-symbolic");
		toggleButton.SetTooltipText("Toggle Sidebar");
		toggleButton.SetActive(true); // Initial state: sidebar is open
		toggleButton.SetVisible(true);

		// REF: ShellToggleButton.cs, sor 18-35
		// void ShellToggleButton_OnClicked(Button sender, EventArgs args)
		// {
		//     if (_shell.IsUpdating) { return; }
		//     _shell.IsUpdating = true;
		//     var newState = !_shell.ShowSidebar;
		//     _shell.Native.SetShowSidebar(newState);
		//     SetActive(newState);
		// }
		toggleButton.OnClicked += (sender, args) =>
		{
			// If a programmatic update is in progress, ignore user clicks to avoid races
			if (_isUpdating)
			{
#if DEBUG
				Console.Out.WriteLine($"---- [ShellToggleButton][OnClicked] ignored due to IsUpdating=true");
#endif
				return;
			}

			// Mark that a toggle button update is in progress to prevent OnRowSelected from closing immediately
			_isUpdating = true;
			// Use native setter to request show/hide; rely on notify to update `Active`
			var newState = !(_splitView?.GetShowSidebar() ?? false);
			_splitView?.SetShowSidebar(newState);
			// Immediately sync button state to match the requested native state
			toggleButton.SetActive(newState);

#if DEBUG
			Console.Out.WriteLine($"[ShellToggleButton][OnClicked] after: ShowSidebar={ShowSidebar}, Active={toggleButton.Active}");
#endif
		};

		return toggleButton;
	}

	Gtk.Button CreateSidebarCloseButton()
	{
		var closeButton = Gtk.Button.New();
		closeButton.SetIconName("window-close-symbolic");
		closeButton.SetTooltipText("Close Sidebar");
		closeButton.SetVisible(false); // Hidden by default, shown by breakpoint

		closeButton.OnClicked += (sender, args) =>
		{
			if (_isUpdating)
			{
				return;
			}

			_isUpdating = true;
			_splitView?.SetShowSidebar(false);
			// OnNotify handler will reset _isUpdating and sync toggle button
		};

		return closeButton;
	}

	public void SetElement(Shell shell, IMauiContext context)
	{
		_ = shell ?? throw new ArgumentNullException($"{nameof(shell)} cannot be null here.");
		_ = context ?? throw new ArgumentNullException($"{nameof(context)} cannot be null here.");

		Element = shell;
		MauiContext = context;

		Element.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == Shell.CurrentStateProperty.PropertyName)
			{
				UpdateSearchHandler();
			}
		};
	}

	public void SetupBreakpoint(Adw.ApplicationWindow window, int breakpointWidth = 500)
	{
		if (window == null || _splitView == null || _flyoutToggleButton == null || _sidebarCloseButton == null)
		{
			return;
		}

		var breakpoint = Adw.Breakpoint.New(Adw.BreakpointCondition.Parse($"max-width: {breakpointWidth}sp"));
		breakpoint.AddSetter(_splitView, "show-sidebar", new GObject.Value(false));
		breakpoint.AddSetter(_splitView, "collapsed", new GObject.Value(true));
		breakpoint.AddSetter(_flyoutToggleButton, "active", new GObject.Value(false));
		breakpoint.AddSetter(_flyoutToggleButton, "icon-name", new GObject.Value("menu_new-symbolic"));
		breakpoint.AddSetter(_sidebarCloseButton, "visible", new GObject.Value(true));
		window.AddBreakpoint(breakpoint);
	}

	public void UpdateFlyoutBehavior(FlyoutBehavior flyoutBehavior)
	{
		switch (flyoutBehavior)
		{
			case FlyoutBehavior.Disabled:
				_splitView?.SetShowSidebar(false);
				_splitView?.SetCollapsed(true);
				_isOpen = false;
				break;
			case FlyoutBehavior.Flyout:
				// Desktop mode - side-by-side layout with sidebar open (like TestGnome1)
				//_splitView?.SetCollapsed(false);
				_splitView?.SetShowSidebar(true);
				_isOpen = true;
				break;
			case FlyoutBehavior.Locked:
				// Locked mode - side-by-side mode, sidebar always visible
				_splitView?.SetCollapsed(false);
				_splitView?.SetShowSidebar(true);
				_isOpen = true;
				break;
		}
	}

	public void UpdateDrawerWidth(double drawerwidth)
	{
		if (drawerwidth > 0)
		{
			_splitView.SetMinSidebarWidth(drawerwidth);
			_splitView.SetMaxSidebarWidth(drawerwidth);
		}
	}

	public void UpdateFlyout(IView? flyout)
	{
		_flyoutView = flyout;

		if (_flyoutView != null && _sidebarBox != null && MauiContext != null)
		{
			var platformView = _flyoutView.ToPlatform(MauiContext);

			// Clear existing children
			while (_sidebarBox.GetFirstChild() != null)
			{
				var child = _sidebarBox.GetFirstChild();
				_sidebarBox.Remove(child!);
			}

			_sidebarBox.Append(platformView);
			platformView.Show();
		}
	}

	public void UpdateBackgroundColor(GColor color)
	{
		// Background color is handled by GTK4/Adwaita theming
		// Could be implemented using CSS providers if needed
	}

	public void UpdateCurrentItem(ShellItem? newItem)
	{
		if (newItem == null || MauiContext == null || _contentInnerBox == null)
		{
			return;
		}

		// Reuse existing handler if available, like Windows implementation
		if (_currentItemHandler == null)
		{
			_currentItemHandler = (ShellItemHandler)newItem.ToHandler(MauiContext);
			var platformView = newItem.ToPlatform(MauiContext);
			_contentInnerBox.Append(platformView);
			platformView.Show();
			platformView.QueueResize();
			_contentInnerBox.QueueResize();
		}
		else if (_currentItemHandler.VirtualView != newItem)
		{
			// Just update the virtual view, DON'T dispose and recreate
			_currentItemHandler.SetVirtualView(newItem);
		}

		UpdateSearchHandler();
	}

	public void UpdateFlyoutFooter(Shell shell)
	{
		// Footer implementation - could use a separate box at the bottom of sidebar
		if (ShellController.FlyoutFooter != null && MauiContext != null && _sidebarBox != null)
		{
			var footerView = ShellController.FlyoutFooter.ToPlatform(MauiContext);
			// Append at the end of sidebar
			_sidebarBox.Append(footerView);
		}
	}

	public void UpdateFlyoutHeader(Shell shell)
	{
		_headerBehavior = shell.FlyoutHeaderBehavior;

		// Update sidebar header title (use Shell.Title or default "Menu")
		if (_sidebarHeaderBar != null)
		{
			var title = !string.IsNullOrEmpty(shell.Title) ? shell.Title : "Menu";
			_sidebarHeaderBar.SetTitleWidget(Gtk.Label.New(title));
		}

		if (_flyoutView != null)
		{
			return;
		}

		if (_headerView != null && _headerView.Handler is IPlatformViewHandler nativeHandler)
		{
			//nativeHandler.Dispose();
			_headerView.Handler = null;
		}

		_headerView = ShellController.FlyoutHeader;

		if (HeaderOnMenu && _headerView != null && MauiContext != null && _sidebarBox != null)
		{
			var headerPlatformView = _headerView.ToPlatform(MauiContext);
			_sidebarBox.Prepend(headerPlatformView);
			headerPlatformView.Show();
		}

		UpdateItems();
	}

	public void UpdateItems()
	{
		if (_flyoutView != null)
		{
			return;
		}

		if (_sidebarBox == null || MauiContext == null)
		{
			return;
		}

		// Clear existing children (keep header if not in menu)
		Gtk.Widget? headerWidget = null;
		if (_headerView != null && !HeaderOnMenu)
		{
			headerWidget = _sidebarBox.GetFirstChild();
		}

		while (_sidebarBox.GetFirstChild() != null)
		{
			var child = _sidebarBox.GetFirstChild();
			if (child == headerWidget)
			{
				break;
			}

			_sidebarBox.Remove(child!);
		}

		// Create flyout items list
		var newGrouping = ShellController.GenerateFlyoutGrouping();
		if (IsItemChanged(newGrouping))
		{
			_cachedGroups = newGrouping;
			_items.Clear();
			foreach (var group in newGrouping)
			{
				foreach (var item in group)
				{
					_items.Add(item);
				}
			}
		}

		var adaptor = new ShellFlyoutItemAdaptor(Element!, _items, HeaderOnMenu);
		var listBox = adaptor.CreateListBox();

		listBox.OnRowActivated += OnFlyoutItemSelected;

		_sidebarBox.Append(listBox);
		listBox.Show();

		_flyoutListBox = listBox;

	}

	bool IsItemChanged(List<List<Element>> groups)
	{
		if (_cachedGroups == null)
		{
			return true;
		}

		if (_cachedGroups == groups)
		{
			return false;
		}

		if (_cachedGroups.Count != groups.Count)
		{
			return true;
		}

		for (int i = 0; i < groups.Count; i++)
		{
			if (_cachedGroups[i].Count != groups[i].Count)
			{
				return true;
			}
			// REF: Program.cs, sor 146-190
			// flyoutItems.OnRowSelected += (sender, args) =>
			// {
			//     if (args.Row != null)
			//     {
			//         if (shell.IsUpdating) { return; }
			//         
			//         var index = args.Row.GetIndex();
			//         var item = shellContents[index];
			//         var page = (Page)Activator.CreateInstance(item.ContentTemplate!)!;
			//         window.SetTitle(page.Title);
			//         contentNavbar.SetContent(page);
			//         
			//         if (shell.Collapsed)
			//         {
			//             shell.IsUpdating = true;
			//             shell.Native.SetShowSidebar(false);
			//             shell.Collapsed = true;
			//             shell.Native.QueueResize();
			//             contentNavbar.Native.GrabFocus();
			//             shellToggleButton.Active = false;
			//         }
			//     }
			// };

			for (int j = 0; j < groups[i].Count; j++)
			{
				if (_cachedGroups[i][j] != groups[i][j])
				{
					return true;
				}
			}
		}

		return false;
	}

	void OnFlyoutItemSelected(Gtk.ListBox sender, Gtk.ListBox.RowActivatedSignalArgs args)
	{
		if (args.Row != null)
		{
			// Ignore row selection during toggle button updates
			if (_isUpdating)
			{
				return;
			}

			var index = args.Row.GetIndex();

			// Skip header row if present (MAUI specific)
			int offset = HeaderOnMenu && _headerView != null ? 1 : 0;
			index -= offset;

			if (index >= 0 && index < _items.Count)
			{
				var selectedItem = _items[index];

				// Don't close menu if clicking on already selected item (MAUI specific)
				if (selectedItem == _currentSelectedItem)
				{
					return;
				}
				_currentSelectedItem = selectedItem;

				// MAUI equivalent of: var item = shellContents[index]; page creation; title; content
				((IShellController)Element!).OnFlyoutItemSelected(selectedItem);

				// Decide based on the split view state, not the window width.
				if (Collapsed)
				{
					// mark programmatic update to prevent toggle races
					_isUpdating = true;

					// call native setter explicitly and force visual update
					_splitView?.SetShowSidebar(false);
					// ensure the split view is collapsed so the overlay hides reliably
					Collapsed = true;
					_splitView?.QueueResize();

					// return focus to main content so the list doesn't keep the overlay open
					_contentBox?.GrabFocus();
					// reflect state on the toggle button
					_flyoutToggleButton.Active = false;
				}
			}
		}
	}

	public void UpdateFlyoutBackDrop(Brush backdrop)
	{
		// Backdrop is handled by AdwNavigationSplitView automatically
		// Custom backdrop styling could be added via CSS if needed
	}

	public void UpdateSearchHandler()
	{
		var newPage = Element?.GetCurrentShellPage() as Page;

		if (newPage != null && _currentPage != newPage)
		{
			if (_currentPage != null)
			{
				_currentPage.PropertyChanged -= OnPagePropertyChanged;
			}

			_currentPage = newPage;
			_currentPage.PropertyChanged += OnPagePropertyChanged;

			SetSearchHandler();
		}
	}

	void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
		{
			SetSearchHandler();
		}
	}

	void SetSearchHandler()
	{
		var newSearchHandler = Element?.GetEffectiveValue<SearchHandler?>(Shell.SearchHandlerProperty, null);

		if (newSearchHandler != _currentSearchHandler)
		{
			if (_currentSearchHandler is not null)
			{
				_currentSearchHandler.PropertyChanged -= OnCurrentSearchHandlerPropertyChanged;
			}

			_currentSearchHandler = newSearchHandler;

			// TODO: Implement search bar using Gtk.SearchEntry
			if (_currentSearchHandler != null)
			{
				_currentSearchHandler.PropertyChanged += OnCurrentSearchHandlerPropertyChanged;
			}
		}
	}

	void OnCurrentSearchHandlerPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		// TODO: Update search bar properties
	}

	void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
	{
		UpdateFlyoutBehavior(behavior);
	}
}
