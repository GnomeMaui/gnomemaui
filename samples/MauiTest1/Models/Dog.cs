using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiTest1.Models;

public partial class Dog : ObservableObject
{
	[ObservableProperty]
	public partial string Name { get; set; }
}