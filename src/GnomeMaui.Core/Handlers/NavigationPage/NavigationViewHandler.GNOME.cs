using System;

namespace Microsoft.Maui.Handlers;

public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, StackNavigationManager>
{
	protected override StackNavigationManager CreatePlatformView() => new();

	protected override void ConnectHandler(StackNavigationManager platformView)
	{
		platformView.Connect(VirtualView);
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(StackNavigationManager platformView)
	{
		platformView.Disconnect();
		base.DisconnectHandler(platformView);
	}


	public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
	{
		if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest ea)
		{
			platformHandler.PlatformView?.RequestNavigation(ea);
		}
		else
		{
			throw new InvalidOperationException("Args must be NavigationRequest");
		}
	}
}
