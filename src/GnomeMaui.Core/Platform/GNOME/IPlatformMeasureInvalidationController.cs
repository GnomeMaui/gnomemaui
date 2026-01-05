namespace Microsoft.Maui.Platform;

/// <summary>
/// Interface for platform views that want to control measure invalidation propagation.
/// Views implementing this interface can stop invalidation from propagating to ancestors
/// when they have fixed constraints or want to handle their own remeasure.
/// </summary>
internal interface IPlatformMeasureInvalidationController
{
	/// <summary>
	/// Invalidates the measure for this view and determines if the invalidation should propagate to ancestors.
	/// </summary>
	/// <param name="isPropagating">True if this invalidation is propagating from a descendant view</param>
	/// <returns>True if the invalidation should continue propagating to ancestors, false to stop propagation</returns>
	bool InvalidateMeasure(bool isPropagating = false);
}
