using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices;

partial class HapticFeedbackImplementation : IHapticFeedback
{
	public bool IsSupported
		=> throw ExceptionUtils.NotSupportedOrImplementedException;

	public void Perform(HapticFeedbackType type)
		=> throw ExceptionUtils.NotSupportedOrImplementedException;
}
