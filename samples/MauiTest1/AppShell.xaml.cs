namespace MauiTest1;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register the SecondPage route
		Routing.RegisterRoute("home", typeof(Home));
		Routing.RegisterRoute("counter", typeof(Counter));
		Routing.RegisterRoute("christmas", typeof(Christmas));
		Routing.RegisterRoute("juliafractal", typeof(JuliaFractal));


		Routing.RegisterRoute("stacklayout1", typeof(Layouts.StackLayoutTest1));
		Routing.RegisterRoute("stacklayout2", typeof(Layouts.StackLayoutTest2));

		Routing.RegisterRoute("absolutelayout1", typeof(Layouts.AbsoluteLayoutTest1));
		Routing.RegisterRoute("absolutelayout2", typeof(Layouts.AbsoluteLayoutTest2));

		Routing.RegisterRoute("grid1", typeof(Layouts.GridTest1));
		Routing.RegisterRoute("grid2", typeof(Layouts.GridTest2));
		Routing.RegisterRoute("grid3", typeof(Layouts.GridTest3));

		Routing.RegisterRoute("flexlayout1", typeof(Layouts.FlexLayoutTest1));

		Routing.RegisterRoute("photowrapping", typeof(Layouts.PhotoWrappingPage));

		Routing.RegisterRoute("pickerdemo", typeof(Layouts.PickerDemoPage));

		Routing.RegisterRoute("progressbardemo", typeof(Layouts.ProgressBarDemoPage));
	}
}
