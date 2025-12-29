using System;

namespace Microsoft.Maui.ApplicationModel;

public static partial class MainThread
{
	static readonly SynchronizationContext? _mainContext = SynchronizationContext.Current;

	static bool PlatformIsMainThread => SynchronizationContext.Current == _mainContext;

	static void PlatformBeginInvokeOnMainThread(Action action)
	{
		if (PlatformIsMainThread)
			action();
		else
			_mainContext?.Post(_ => action(), null);
	}
}
