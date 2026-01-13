#if !NET10_0_OR_GREATER
#nullable enable
using System;

namespace Microsoft.Maui.Controls.Xaml.Internals;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class AllowImplicitXmlnsDeclarationAttribute : Attribute
{
	public AllowImplicitXmlnsDeclarationAttribute(bool allow = true)
	{
		Allow = allow;
	}

	public bool Allow { get; }
}
#endif
