namespace Microsoft.Maui.Platform;

public class WindowRootView : Gtk.Box
{
	// Layout komponensek
	readonly Adw.ToolbarView _toolbarView = default!;
	MauiToolbar? _toolbar;
	Gtk.Widget? _content;
	Gio.Menu? _menuBar;
	ITitleBar? _titleBar;

	// Events
	internal event EventHandler? OnApplyTemplateFinished;
	internal event EventHandler? OnWindowTitleBarContentSizeChanged;
	internal event EventHandler? ContentChanged;
	internal event EventHandler? BackRequested;

	public WindowRootView()
	{
		SetOrientation(Gtk.Orientation.Vertical);
		SetSpacing(0);

		_toolbarView = Adw.ToolbarView.New();
		Append(_toolbarView);

		// Default toolbar for the window
		_toolbar = new MauiToolbar();
		_toolbarView.AddTopBar(_toolbar);
	}

	// Toolbar handling
	internal MauiToolbar? Toolbar
	{
		get => _toolbar;
		set
		{
			var previous = _toolbar;
			if (previous != null)
			{
				previous.SetMenuBar(null);
				previous.BackButtonClicked -= OnToolbarBackButtonClicked;
			}

			_toolbar = value;

			if (_toolbar != null)
			{
				_toolbar.SetMenuBar(_menuBar);
				_toolbar.BackButtonClicked += OnToolbarBackButtonClicked;
			}

			// If current content is a RootNavigationView, update its toolbar reference
			if (_content is RootNavigationView currentNav)
			{
				currentNav.SetToolbar(_toolbar);
			}
		}
	}

	internal Gio.Menu? MenuBar
	{
		get => _menuBar;
		set
		{
			_menuBar = value;
			_toolbar?.SetMenuBar(value);
		}
	}

	// Content handling
	internal Gtk.Widget? Content
	{
		get => _content;
		set
		{
			if (_content != null)
			{
				Remove(_content);
			}

			_content = value;

			if (_content != null)
			{
				// Append content below the toolbar so we avoid Adw.ToolbarView.SetContent
				Append(_content);

				if (_content is Gtk.Widget cw)
				{
					cw.SetHexpand(true);
					cw.SetVexpand(true);
					cw.Show();
				}

				if (_content is RootNavigationView navView)
				{
					// Ensure navigation view receives toolbar updates and back requests
					navView.BackRequested += (s, e) => BackRequested?.Invoke(s, e);
					navView.SetToolbar(_toolbar);
				}
			}

			ContentChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	// Title management
	internal string? WindowTitle
	{
		get => _toolbar?.Title;
		set => _toolbar?.Title = value;
	}

	// Layout updates (no-op for GNOME since Adw.HeaderBar manages titlebar sizing)
	internal void UpdateAppTitleBar(double height, bool useCustom) { }

	// TitleBar integration
	internal void SetTitleBar(ITitleBar? titlebar, IMauiContext? mauiContext)
	{
		_titleBar = titlebar;
		// Future: support custom titlebar integration
	}

	internal void SetTitleBarVisibility(bool visible)
	{
		_toolbar?.SetVisible(visible);
	}

	void OnToolbarBackButtonClicked(object? sender, EventArgs e)
	{
		BackRequested?.Invoke(sender, EventArgs.Empty);
	}
}