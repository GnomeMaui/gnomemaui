using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Maui.ApplicationModel;

class AppInfoImplementation : IAppInfo
{
	// static readonly Assembly _launchingAssembly = Assembly.GetEntryAssembly();

	public string PackageName => throw new NotImplementedException();

	public string Name => throw new NotImplementedException();

	public string VersionString => throw new NotImplementedException();

	public Version Version => throw new NotImplementedException();

	public string BuildString => throw new NotImplementedException();

	public AppTheme RequestedTheme
	{
		get
		{
			try
			{
				var styleManager = Adw.StyleManager.GetDefault();
				bool isDark = styleManager.GetDark();
				return isDark ? AppTheme.Dark : AppTheme.Light;
			}
			catch
			{
				return AppTheme.Unspecified;
			}
		}
	}

	public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;

	public LayoutDirection RequestedLayoutDirection
	{
		get
		{
			try
			{
				var direction = Gtk.Widget.GetDefaultDirection();
				return direction switch
				{
					Gtk.TextDirection.Ltr => LayoutDirection.LeftToRight,
					Gtk.TextDirection.Rtl => LayoutDirection.RightToLeft,
					_ => LayoutDirection.Unknown
				};
			}
			catch
			{
				return LayoutDirection.Unknown;
			}
		}
	}

	public void ShowSettingsUI()
	{
		throw new NotImplementedException();
	}

}
