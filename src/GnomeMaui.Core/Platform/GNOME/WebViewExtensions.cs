using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class WebViewExtensions
{
	private const string Prefix = "webview";

	public static WebKit.WebView Create(this WebViewHandler _)
	{
		var webView = new WebKit.WebView();
		webView.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return webView;
	}

	public static void UpdateText(this WebKit.WebView platform, IWebView webView)
	{
		//platform.Text = searchBar.Text ?? string.Empty;
	}
}
