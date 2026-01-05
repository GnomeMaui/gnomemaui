namespace MauiTest1.Layouts;

public partial class GridTest3 : ContentPage
{
	public GridTest3()
	{
		InitializeComponent();
	}

	private void OnSliderValueChanged(object? sender, ValueChangedEventArgs e)
	{
		if (boxView != null)
		{
			var red = redSlider.Value;
			var green = greenSlider.Value;
			var blue = blueSlider.Value;
			boxView.Color = Color.FromRgb(red, green, blue);
		}
	}
}
