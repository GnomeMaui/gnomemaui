using Gtk;

namespace Microsoft.Maui.Platform;

public class MauiToolbar : Adw.Bin
{
	public Adw.HeaderBar CommandBar => (Adw.HeaderBar)this.Child!;

	Gtk.Label? _titleLabel;
	Gtk.Image? _titleIcon;
	Gtk.Widget? _titleView;
	Gtk.Box? _titleBox;
	Gtk.MenuButton? _menuButton;
	Gtk.Button? _togglePaneButton;
	Gtk.Button? _backButton;
	Graphics.Color? _iconColor;
	Graphics.Color? _textColor;
	Graphics.Color? _backgroundColor;

	public MauiToolbar()
	{
		Valign = Align.Fill;
		Halign = Align.Fill;
		this.Child = Adw.HeaderBar.New();
		InitializeComponents();
	}

	void InitializeComponents()
	{
		_titleBox = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
		_titleBox.SetHalign(Gtk.Align.Center);

		_titleLabel = Gtk.Label.New(string.Empty);
		_titleLabel.SetVisible(false);
		_titleBox.Append(_titleLabel);

		CommandBar.SetTitleWidget(_titleBox);
		CommandBar.SetShowBackButton(false);

		_backButton = Gtk.Button.NewFromIconName("go-previous-symbolic");
		_backButton.SetVisible(false);
		_backButton.GetStyleContext().AddClass("suggested-action");
		_backButton.OnClicked += (s, e) => BackButtonClicked?.Invoke(this, EventArgs.Empty);
		CommandBar.PackStart(_backButton);
	}

	internal string? Title
	{
		get => _titleLabel?.GetLabel();
		set
		{
			if (_titleLabel != null)
			{
				_titleLabel.SetLabel(value ?? string.Empty);
				_titleLabel.SetVisible(!string.IsNullOrWhiteSpace(value));
			}
		}
	}

	internal Gtk.Image? TitleIconImage
	{
		get => _titleIcon;
	}

	internal string? TitleIconImageSource
	{
		get => _titleIcon?.GetIconName();
		set
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				if (_titleIcon == null)
				{
					_titleIcon = Gtk.Image.NewFromIconName(value);
					_titleBox?.Prepend(_titleIcon);
				}
				else
				{
					_titleIcon.SetFromIconName(value);
				}
				_titleIcon.SetVisible(true);
			}
			else if (_titleIcon != null)
			{
				_titleIcon.SetVisible(false);
			}
		}
	}

	internal Gtk.Widget? TitleView
	{
		get => _titleView;
		set
		{
			_titleView = value;

			if (value != null)
			{
				CommandBar.SetTitleWidget(value);
			}
			else if (_titleBox != null)
			{
				CommandBar.SetTitleWidget(_titleBox);
			}
		}
	}

	// Bar színek beállítása
	internal void SetBarTextColor(Graphics.Color? color)
	{
		_textColor = color;

		if (color != null && _titleLabel != null)
		{
			// GNOME-ban CSS-sel állítható be a szín
			var cssProvider = Gtk.CssProvider.New();
			var css = $"label {{ color: rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha}); }}";
			cssProvider.LoadFromString(css);
			_titleLabel.GetStyleContext().AddProvider(cssProvider, Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION);
		}
	}

	internal void SetBarBackground(Graphics.Color? color)
	{
		_backgroundColor = color;

		if (color != null)
		{
			// HeaderBar háttérszín beállítása CSS-sel
			var cssProvider = Gtk.CssProvider.New();
			var css = $"headerbar {{ background-color: rgba({color.Red * 255}, {color.Green * 255}, {color.Blue * 255}, {color.Alpha}); }}";
			cssProvider.LoadFromString(css);
			CommandBar.GetStyleContext().AddProvider(cssProvider, Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION);
		}
	}

	// Back button kezelés - custom back button használata
	public bool IsBackButtonVisible
	{
		get => _backButton?.GetVisible() ?? false;
		set
		{
			if (_backButton != null)
			{
				_backButton.SetVisible(value);
			}
		}
	}

	public event EventHandler? BackButtonClicked;

	public bool IsBackEnabled
	{
		get => CommandBar.GetSensitive();
		set => CommandBar.SetSensitive(value);
	}

	internal Gtk.Button? TogglePaneButton
	{
		get => _togglePaneButton;
		set
		{
			_togglePaneButton = value;
			UpdateIconColor();
		}
	}

	internal Graphics.Color? IconColor
	{
		get => _iconColor;
		set
		{
			_iconColor = value;
			UpdateIconColor();
		}
	}

	void UpdateIconColor()
	{
		if (_iconColor != null)
		{
			var cssProvider = Gtk.CssProvider.New();
			var colorString = $"rgba({_iconColor.Red * 255}, {_iconColor.Green * 255}, {_iconColor.Blue * 255}, {_iconColor.Alpha})";
			var css = $"headerbar button {{ color: {colorString}; }}";
			cssProvider.LoadFromString(css);
			CommandBar.GetStyleContext().AddProvider(cssProvider, Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION);

			if (_togglePaneButton != null)
			{
				_togglePaneButton.GetStyleContext().AddProvider(cssProvider, Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION);
			}
		}
	}

	internal void SetMenuBar(Gio.Menu? menu)
	{
		if (menu != null && menu.GetNItems() > 0)
		{
			if (_menuButton == null)
			{
				_menuButton = Gtk.MenuButton.New();
				_menuButton.SetIconName("open-menu-symbolic");
				_menuButton.SetTooltipText("Menu");
				CommandBar.PackEnd(_menuButton);
			}

			_menuButton.SetMenuModel(menu);
			_menuButton.SetVisible(true);
		}
		else if (_menuButton != null)
		{
			_menuButton.SetVisible(false);
		}
	}

	internal void PackStart(Gtk.Widget widget) => CommandBar.PackStart(widget);

	internal void PackEnd(Gtk.Widget widget) => CommandBar.PackEnd(widget);

	internal void Remove(Gtk.Widget widget) => CommandBar.Remove(widget);
}