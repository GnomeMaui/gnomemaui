using Microsoft.Maui.Controls.Handlers;
using System.ComponentModel;
using System.Text;
using GColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform;

public class ShellView : Gtk.Box, IFlyoutBehaviorObserver
{
	public readonly GColor DefaultBackgroundColor = new(1f, 1f, 1f, 1f);

	readonly Adw.OverlaySplitView _splitView;
	readonly Gtk.Box _sidebarBox;
	readonly Gtk.Box _contentBox;
	readonly Gtk.Box _contentInnerBox;
	readonly Gtk.ToggleButton _flyoutToggleButton = default!;

	Adw.Breakpoint? _autoFlyoutBreakpoint;

	MauiToolbar? _contentToolbar;
	View? _headerView;
	Gtk.Widget? _headerPlatformView;
	View? _footerView;
	Gtk.Widget? _footerPlatformView;
	FlyoutHeaderBehavior _headerBehavior;

	Gtk.ScrolledWindow? _sidebarScrolledWindow;
	Gtk.Box? _sidebarScrollableBox;

	IView? _flyoutView;
	FlyoutBehavior _currentFlyoutBehavior = FlyoutBehavior.Flyout;

	ShellItemHandler? _currentItemHandler;
	SearchHandler? _currentSearchHandler;
	Page? _currentPage;

	bool _isOpen;
	bool _isUpdating;
	bool _initialSelected;

	Gtk.ListBox? _flyoutListBox;
	Element? _currentSelectedItem;
	List<List<Element>> _cachedGroups = [];
	readonly List<Element> _items = [];

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

		// Sidebar box
		_sidebarBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
		_sidebarBox.Hexpand = true;
		_sidebarBox.Vexpand = true;
		_sidebarBox.Halign = Gtk.Align.Fill;
		_sidebarBox.Valign = Gtk.Align.Fill;

		_splitView.SetSidebar(_sidebarBox);

		// Content setup - global toolbar will be added directly (no wrapper)
		_contentBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

		// Create inner box for actual content
		_contentInnerBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
		_splitView.SetContent(_contentBox);

		// Enable gesture support
		_splitView.SetEnableShowGesture(true);
		_splitView.SetEnableHideGesture(true);

		// Initial state - sidebar CLOSED by default (FlyoutIsPresented will control actual state)
		// collapsed=true means overlay mode (sidebar on top of content, like Windows)
		_splitView.SetCollapsed(true);
		_splitView.SetShowSidebar(false);
		_isOpen = false; // Sync internal state BEFORE creating toggle button

		// Create toggle button (will be added to global toolbar in SetElement)
		_flyoutToggleButton = CreateFlyoutToggleButton();

		_splitView.OnNotify += Notified;

		// Add split view to this container
		Append(_splitView);
		_splitView.Show();
		_contentBox.QueueResize();
	}

	private void Notified(GObject.Object sender, NotifySignalArgs args)
	{
		var name = args.Pspec.GetName();
		if (name == "show-sidebar")
		{
			// keep the toggle button state in sync when the property changes
			if (_flyoutToggleButton != null)
			{
				_flyoutToggleButton.Active = ShowSidebar;
			}
			// programmatic update finished
			_isUpdating = false;
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
		var toggleButton = Gtk.ToggleButton.New();
		toggleButton.SetIconName("menu_new-symbolic");
		toggleButton.SetTooltipText("Toggle Sidebar");
		toggleButton.SetActive(false); // Initial state: sidebar is closed (matches initial _isOpen = false)
		toggleButton.SetVisible(true);

		toggleButton.OnClicked += (sender, args) =>
		{
			// If a programmatic update is in progress, ignore user clicks to avoid races
			if (_isUpdating)
			{
				return;
			}

			// Mark that a toggle button update is in progress to prevent OnRowSelected from closing immediately
			_isUpdating = true;
			// Use native setter to request show/hide; rely on notify to update `Active`
			var newState = !(_splitView?.GetShowSidebar() ?? false);
			_splitView?.SetShowSidebar(newState);
			// Immediately sync button state to match the requested native state
			toggleButton.SetActive(newState);
		};

		return toggleButton;
	}

	public void SetElement(Shell shell, IMauiContext? context)
	{
		ArgumentNullException.ThrowIfNull(shell, $"{nameof(shell)} cannot be null here.");
		ArgumentNullException.ThrowIfNull(context, $"{nameof(context)} cannot be null here.");

		Element = shell;
		MauiContext = context;

		// NEW: Get global toolbar from WindowRootView and add DIRECTLY to _contentBox
		_contentToolbar = context.GetNavigationRootManager()?.Toolbar;
		if (_contentToolbar != null && _contentBox != null)
		{
			// Unparent from WindowRootView
			if (_contentToolbar.GetParent() != null)
			{
				_contentToolbar.Unparent();
			}

			// Add toolbar DIRECTLY to _contentBox (not wrapped in ToolbarView)
			_contentBox.Prepend(_contentToolbar);

			// Add flyout toggle button to global toolbar
			_contentToolbar.PackStart(_flyoutToggleButton);

			// Add content box after toolbar
			if (_contentInnerBox != null)
			{
				_contentBox.Append(_contentInnerBox);
			}
		}

		Element.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == Shell.CurrentStateProperty.PropertyName)
			{
				UpdateSearchHandler();
			}
			else if (e.PropertyName == PlatformConfiguration.GNOMESpecific.Shell.AutoFlyoutProperty.PropertyName)
			{
				UpdateAutoFlyout();
			}
		};

		// Setup initial breakpoint if configured
		UpdateAutoFlyout();
	}

	void UpdateAutoFlyout()
	{
		var breakpointWidth = PlatformConfiguration.GNOMESpecific.Shell.GetAutoFlyout(Element);
		// Find the native window
		Adw.ApplicationWindow? nativeWindow = null;
		if (Application.Current?.Windows is { } windows)
		{
			foreach (var window in windows)
			{
				if (window?.Handler?.PlatformView is Adw.ApplicationWindow appWindow)
				{
					nativeWindow = appWindow;
					break;
				}
			}
		}

		if (nativeWindow == null)
		{
			return;
		}

		// Remove old breakpoint if exists
		if (_autoFlyoutBreakpoint != null)
		{
			// NOTE: GirCore may not have RemoveBreakpoint, so we might need to recreate the window
			// For now, we'll just replace it
			_autoFlyoutBreakpoint = null;
		}

		// If breakpoint is disabled (0 or negative), don't add new one
		if (breakpointWidth <= 0)
		{
			return;
		}

		// Create Adwaita breakpoint with native condition
		var condition = Adw.BreakpointCondition.Parse($"max-width: {breakpointWidth}sp");
		_autoFlyoutBreakpoint = Adw.Breakpoint.New(condition);

		// Add setters for overlay mode (narrow screen)
		_autoFlyoutBreakpoint.AddSetter(_splitView, "show-sidebar", new GObject.Value(false));
		_autoFlyoutBreakpoint.AddSetter(_splitView, "collapsed", new GObject.Value(true));
		_autoFlyoutBreakpoint.AddSetter(_flyoutToggleButton, "active", new GObject.Value(false));

		// Add breakpoint to window
		nativeWindow.AddBreakpoint(_autoFlyoutBreakpoint);
	}

	public void UpdateFlyoutBehavior(FlyoutBehavior flyoutBehavior)
	{
		_currentFlyoutBehavior = flyoutBehavior;

		switch (flyoutBehavior)
		{
			case FlyoutBehavior.Disabled:
				_splitView?.SetShowSidebar(false);
				_splitView?.SetCollapsed(true);
				_isOpen = false;
				_flyoutToggleButton?.SetVisible(false);
				break;
			case FlyoutBehavior.Flyout:
				// Flyout mode - toggle-able sidebar with overlay (like Windows)
				// Collapsed=true means overlay (sidebar appears on top of content)
				// Actual open/closed state controlled by IsPresented
				_splitView?.SetCollapsed(true);
				_flyoutToggleButton?.SetVisible(true);
				// Don't change ShowSidebar here - let IsPresented control it
				break;
			case FlyoutBehavior.Locked:
				// Locked mode - side-by-side mode, sidebar always visible
				_splitView?.SetCollapsed(false);
				_splitView?.SetShowSidebar(true);
				_isOpen = true;
				_flyoutToggleButton?.SetVisible(false);
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
		if (_flyoutView != null)
		{
			return;
		}

		var newFooterView = shell?.FlyoutFooter as View;

		// Check if this is a DIFFERENT footer view
		if (_footerView != null && _footerView != newFooterView)
		{
			if (_footerView.Handler is IPlatformViewHandler nativeHandler)
			{
				_footerView.Handler = null;
			}
			_footerPlatformView = null;
		}

		_footerView = newFooterView;

		// Rebuild sidebar structure
		RebuildSidebarStructure();
	}

	public void UpdateFlyoutHeader(Shell shell)
	{
		_headerBehavior = shell.FlyoutHeaderBehavior;

		if (_flyoutView != null)
		{
			return;
		}

		var newHeaderView = ShellController.FlyoutHeader;

		// Check if this is a DIFFERENT header view - only then dispose the old one
		if (_headerView != null && _headerView != newHeaderView)
		{
			if (_headerView.Handler is IPlatformViewHandler nativeHandler)
			{
				_headerView.Handler = null;
			}
			_headerPlatformView = null;
		}

		_headerView = newHeaderView;

		// Rebuild sidebar structure based on FlyoutHeaderBehavior
		RebuildSidebarStructure();
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

		// Update items list
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

		// Rebuild sidebar structure with updated items
		RebuildSidebarStructure();
	}

	void RebuildSidebarStructure()
	{
		if (_sidebarBox == null || MauiContext == null)
		{
			return;
		}

		// 1. Clear all existing children from _sidebarBox
		while (_sidebarBox.GetFirstChild() != null)
		{
			var child = _sidebarBox.GetFirstChild();
			child?.Unparent(); // CRITICAL: Unparent instead of Remove
		}

		// Disconnect old ListBox event if exists
		if (_flyoutListBox != null)
		{
			_flyoutListBox.OnRowActivated -= OnFlyoutItemSelected;
			_flyoutListBox = null;
		}

		// Clear cached platform views AND disconnect handlers to force recreation
		if (_headerView != null && _headerPlatformView != null)
		{
			_headerView.Handler = null; // Disconnect handler to force new widget creation
		}
		if (_footerView != null && _footerPlatformView != null)
		{
			_footerView.Handler = null; // Disconnect handler to force new widget creation
		}

		_headerPlatformView = null;
		_footerPlatformView = null;
		_sidebarScrolledWindow = null;
		_sidebarScrollableBox = null;

		// 2. Create header platform view if needed
		if (_headerView != null)
		{
			_headerPlatformView = _headerView.ToPlatform(MauiContext);
		}

		// 3. Create footer platform view if needed
		if (_footerView != null)
		{
			_footerPlatformView = _footerView.ToPlatform(MauiContext);
		}

		// 4. Create ListBox with items
		if (_items.Count > 0)
		{
			var adaptor = new ShellFlyoutItemAdaptor(Element!, _items, false); // HeaderOnMenu always false - we handle header separately
			_flyoutListBox = adaptor.CreateListBox();
			_flyoutListBox.OnRowActivated += OnFlyoutItemSelected;
		}

		// 5. Build structure based on FlyoutHeaderBehavior
		switch (_headerBehavior)
		{
			case FlyoutHeaderBehavior.Default:
			case FlyoutHeaderBehavior.Fixed:
				// Fixed: Header + ScrolledWindow(ListBox) + Footer
				BuildFixedHeaderStructure();
				break;

			case FlyoutHeaderBehavior.Scroll:
				// Scroll: ScrolledWindow(Header + ListBox + Footer)
				BuildScrollHeaderStructure();
				break;

			case FlyoutHeaderBehavior.CollapseOnScroll:
				// TODO: CollapseOnScroll implementation
				// For now, use Scroll behavior
				BuildScrollHeaderStructure();
				break;
		}

		// Select initial row
		if (_flyoutListBox != null && !_initialSelected)
		{
			_flyoutListBox.SelectRow(_flyoutListBox.GetRowAtIndex(0));
			_initialSelected = true;
		}
	}

	void BuildFixedHeaderStructure()
	{
		// Structure:
		// _sidebarBox
		// ├─ _headerPlatformView (if exists) - FIXED, not scrollable
		// ├─ Gtk.ScrolledWindow
		// │  └─ _flyoutListBox - SCROLLABLE
		// └─ _footerPlatformView (if exists) - FIXED, not scrollable

		// Add fixed header at top
		if (_headerPlatformView != null)
		{
			_headerPlatformView.SetVexpand(false); // Only take needed height
			_headerPlatformView.SetHexpand(true);  // Fill width
			_sidebarBox.Append(_headerPlatformView);
			_headerPlatformView.Show();
		}

		// Create ScrolledWindow for ListBox
		if (_flyoutListBox != null)
		{
			_sidebarScrolledWindow = Gtk.ScrolledWindow.New();
			_sidebarScrolledWindow.SetVexpand(true);
			_sidebarScrolledWindow.SetHexpand(true);
			_sidebarScrolledWindow.SetChild(_flyoutListBox);

			_sidebarBox.Append(_sidebarScrolledWindow);
			_sidebarScrolledWindow.Show();
			_flyoutListBox.Show();
		}

		// Add fixed footer at bottom
		if (_footerPlatformView != null)
		{
			_footerPlatformView.SetVexpand(false); // Only take needed height
			_footerPlatformView.SetHexpand(true);  // Fill width
			_sidebarBox.Append(_footerPlatformView);
			_footerPlatformView.Show();
		}
	}

	void BuildScrollHeaderStructure()
	{
		// Structure:
		// _sidebarBox
		// └─ Gtk.ScrolledWindow
		//    └─ Gtk.Box (_sidebarScrollableBox)
		//       ├─ _headerPlatformView (if exists) - SCROLLS together
		//       ├─ _flyoutListBox - SCROLLS
		//       └─ _footerPlatformView (if exists) - SCROLLS together

		// Create ScrolledWindow
		_sidebarScrolledWindow = Gtk.ScrolledWindow.New();
		_sidebarScrolledWindow.SetVexpand(true);
		_sidebarScrolledWindow.SetHexpand(true);

		// Create inner scrollable box
		_sidebarScrollableBox = Gtk.Box.New(Gtk.Orientation.Vertical, 0);

		// Add header to scrollable box
		if (_headerPlatformView != null)
		{
			_headerPlatformView.SetVexpand(false); // Only take needed height
			_headerPlatformView.SetHexpand(true);  // Fill width
			_sidebarScrollableBox.Append(_headerPlatformView);
			_headerPlatformView.Show();
		}

		// Add ListBox to scrollable box
		if (_flyoutListBox != null)
		{
			_sidebarScrollableBox.Append(_flyoutListBox);
			_flyoutListBox.Show();
		}

		// Add footer to scrollable box
		if (_footerPlatformView != null)
		{
			_footerPlatformView.SetVexpand(false); // Only take needed height
			_footerPlatformView.SetHexpand(true);  // Fill width
			_sidebarScrollableBox.Append(_footerPlatformView);
			_footerPlatformView.Show();
		}

		// Set scrollable box as ScrolledWindow child
		_sidebarScrolledWindow.SetChild(_sidebarScrollableBox);

		// Add ScrolledWindow to sidebar
		_sidebarBox.Append(_sidebarScrolledWindow);
		_sidebarScrolledWindow.Show();
		_sidebarScrollableBox.Show();
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

			// NO header offset needed - header is NOT in ListBox anymore
			// It's a separate widget in the sidebar structure

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

	public override void Dispose()
	{
		// 1. Disconnect event handlers
		_splitView.OnNotify -= Notified;
		Element?.PropertyChanged -= OnPagePropertyChanged;
		_currentPage?.PropertyChanged -= OnPagePropertyChanged;
		_currentSearchHandler?.PropertyChanged -= OnCurrentSearchHandlerPropertyChanged;
		_flyoutListBox?.OnRowActivated -= OnFlyoutItemSelected;

		// 2. Unparent child widgets (inside-out order)

		// Unparent _contentInnerBox children
		if (_contentInnerBox != null)
		{
			while (_contentInnerBox.GetFirstChild() != null)
			{
				var child = _contentInnerBox.GetFirstChild();
				child?.Unparent();
			}
		}

		// Unparent _sidebarBox children
		if (_sidebarBox != null)
		{
			while (_sidebarBox.GetFirstChild() != null)
			{
				var child = _sidebarBox.GetFirstChild();
				child?.Unparent();
			}
		}

		// Unparent _contentToolbar (remove from _contentBox)
		if (_contentToolbar != null && _contentToolbar.GetParent() != null)
		{
			_contentToolbar.Unparent();
		}

		// Unparent _contentInnerBox (remove from _contentBox)
		if (_contentInnerBox != null && _contentInnerBox.GetParent() != null)
		{
			_contentInnerBox.Unparent();
		}

		// 3. Call base dispose
		base.Dispose();
		GC.SuppressFinalize(this);
	}
}
