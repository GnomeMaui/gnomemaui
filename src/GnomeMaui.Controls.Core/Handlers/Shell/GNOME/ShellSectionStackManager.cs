using System.Collections.Generic;
using System.Threading.Tasks;
using GColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform;

public class ShellSectionStackManager : StackNavigationManager
{
	protected ShellSection? ShellSection { get; private set; }

	ShellSectionView? _rootView;

	public void Connect(IElement navigationView, IMauiContext mauiContext)
	{
		NavigationView = (IStackNavigation)navigationView;
		MauiContext = mauiContext;
		ShellSection = (ShellSection)navigationView;
	}

	public override void Disconnect()
	{
		base.Disconnect();
		ShellSection = null;
	}

	protected override async Task InitializeStack(IReadOnlyList<IView> newStack, bool animated)
	{
		if (newStack.Count == 0)
		{
			return;
		}

		List<IView> navigationStack = new List<IView>(newStack);

		_rootView = new ShellSectionView(ShellSection!, MauiContext!);

		await PlatformNavigation.Push(_rootView, false);

		navigationStack.RemoveAt(0);
		await base.InitializeStack(navigationStack, animated);
	}
}
