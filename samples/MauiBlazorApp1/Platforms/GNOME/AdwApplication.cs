namespace MauiBlazorApp1;

class AdwApplication : MauiAdwApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public AdwApplication()
		: base("com.companyname.MauiBlazorApp1", typeof(AdwApplication).Assembly) { }
}
