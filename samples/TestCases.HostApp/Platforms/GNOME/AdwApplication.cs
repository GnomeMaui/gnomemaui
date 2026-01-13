using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Platform;

class AdwApplication : MauiAdwApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public AdwApplication() : base("com.companyname.MauiTest1", typeof(AdwApplication).Assembly)
	{
	}
}
