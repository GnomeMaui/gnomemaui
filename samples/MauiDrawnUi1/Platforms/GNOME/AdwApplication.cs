using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MauiDrawnUi1;

class AdwApplication : MauiAdwApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public AdwApplication() : base("com.companyname.MauiDrawnUi1") { }
}
