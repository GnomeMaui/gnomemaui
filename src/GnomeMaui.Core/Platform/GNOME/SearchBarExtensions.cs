using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class SearchBarExtensions
{
	private const string Prefix = "searchbar";

	public static Gtk.SearchEntry Create(this SearchBarHandler _)
	{
		var searchBar = new Gtk.SearchEntry();
		searchBar.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return searchBar;
	}

	public static void UpdateText(this Gtk.SearchEntry platform, ISearchBar searchBar)
	{
		//platform.Text = searchBar.Text ?? string.Empty;
	}
}
