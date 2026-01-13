#if !NET10_0_OR_GREATER
#nullable enable
namespace Microsoft.Maui.Controls.Xaml;

public partial class XamlParseException
{
	internal string UnformattedMessage => Message;
}
#endif
