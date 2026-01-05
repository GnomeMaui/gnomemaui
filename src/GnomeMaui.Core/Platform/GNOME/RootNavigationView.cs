namespace Microsoft.Maui.Platform;

public class RootNavigationView : Gtk.Box
{
	Adw.NavigationView _navigationView;
	MauiToolbar? _toolbar;
	Gtk.Widget? _content;

	public RootNavigationView()
	{
		SetOrientation(Gtk.Orientation.Vertical);
		_navigationView = Adw.NavigationView.New();
		Append(_navigationView);

		// Back button signal bekötése + stack változás figyelése
		_navigationView.OnPushed += (sender, args) =>
		{
			UpdateBackButtonVisibility();
		};
		_navigationView.OnPopped += (sender, args) =>
		{
			BackRequested?.Invoke(this, EventArgs.Empty);
			UpdateBackButtonVisibility();
		};
	}

	internal MauiToolbar? Toolbar
	{
		get => _toolbar;
		set => _toolbar = value;
	}

	// Content kezelés
	internal Gtk.Widget? Content
	{
		get => _content;
		set
		{
			_content = value;

			if (_content != null)
			{
				// Wrap content in AdwNavigationPage if needed
				if (_content is not Adw.NavigationPage)
				{
					var page = Adw.NavigationPage.New(_content, "Content");
					page.SetHexpand(true);
					page.SetVexpand(true);
					page.Show();
					_navigationView.Push(page);
				}
				else
				{
					// Already a NavigationPage - ensure it's visible/expanded
					_content.SetHexpand(true);
					_content.SetVexpand(true);
					_content.Show();
				}
			}
		}
	}

	internal void Push(Adw.NavigationPage page) => _navigationView.Push(page);
	internal bool Pop() => _navigationView.Pop();

	internal event EventHandler? BackRequested;

	internal void SetToolbar(MauiToolbar? toolbar)
	{
		_toolbar = toolbar;
		UpdateBackButtonVisibility();
	}

	void UpdateBackButtonVisibility()
	{
		if (_toolbar == null)
		{
			return;
		}

		var stack = _navigationView.GetNavigationStack();
		uint n = 0;
		if (stack != null)
		{
			n = stack.GetNItems();
		}
		_toolbar.IsBackButtonVisible = n > 1;
	}
}