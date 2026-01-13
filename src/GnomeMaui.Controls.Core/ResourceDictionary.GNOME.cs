#if !NET10_0_OR_GREATER
#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui.Controls;

public partial class ResourceDictionary
{
	internal sealed partial class RDSourceTypeConverter
	{
		public static string GetResourcePath(Uri uri, string? assemblyName)
		{
			var resourcePath = uri.OriginalString;
			
			if (uri.IsAbsoluteUri)
			{
				resourcePath = uri.PathAndQuery;
			}

			if (resourcePath.StartsWith("/"))
			{
				resourcePath = resourcePath.Substring(1);
			}

			if (!string.IsNullOrEmpty(assemblyName))
			{
				resourcePath = Path.Combine(assemblyName, resourcePath);
			}

			return resourcePath;
		}
	}
}
#endif
