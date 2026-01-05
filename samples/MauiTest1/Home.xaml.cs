using System.Collections.ObjectModel;

namespace MauiTest1;

public partial class Home : ContentPage
{
	public ObservableCollection<Models.Dog> Dogs { get; } =
	[
		new() { Name = "Buddy" },
		new() { Name = "Max" },
		new() { Name = "Bella" },
		new() { Name = "Lucy" },
		new() { Name = "Charlie" },
	];

	public Home()
	{
		InitializeComponent();

		// Image image = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 100,
		// 	WidthRequest = 100
		// };
		// flexLayout.Children.Add(image);

		// Image image2 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 150,
		// 	WidthRequest = 150
		// };
		// flexLayout.Children.Add(image2);

		// Image image3 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 120,
		// 	WidthRequest = 120
		// };

		// flexLayout.Children.Add(image3);

		// Image image4 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 130,
		// 	WidthRequest = 130
		// };
		// flexLayout.Children.Add(image4);

		// Image image5 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 110,
		// 	WidthRequest = 110
		// };
		// flexLayout.Children.Add(image5);

		// Image image6 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 140,
		// 	WidthRequest = 140
		// };
		// flexLayout.Children.Add(image6);

		// Image image7 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 125,
		// 	WidthRequest = 125
		// };
		// flexLayout.Children.Add(image7);

		// Image image8 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 135,
		// 	WidthRequest = 135
		// };
		// flexLayout.Children.Add(image8);

		// Image image9 = new Image
		// {
		// 	Source = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/IMG_3588.JPG")),
		// 	HeightRequest = 115,
		// 	WidthRequest = 115
		// };
		// flexLayout.Children.Add(image9);
	}
}
