using DrawnUi.Views;
using MauiDrawnUi1.Tutorials.NewsFeed.ViewModels;

namespace MauiDrawnUi1.Tutorials.NewsFeed;

public partial class NewsFeedPage : DrawnUiBasePage
{

	public NewsFeedPage()
	{
		try
		{
			InitializeComponent();

			BindingContext = new NewsViewModel();
		}
		catch (Exception e)
		{
			Super.DisplayException(this, e);
		}
	}

}
