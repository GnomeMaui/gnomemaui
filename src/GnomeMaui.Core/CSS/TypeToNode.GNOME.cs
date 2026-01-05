namespace GnomeMaui.CSS;

/// <summary>
/// Maps .NET types to GNOME CSS node names.
/// </summary>
public static class TypeToNode
{
	static readonly Dictionary<string, (string nodeName, string? autoStyleClass)> _map = new()
	{
		["Button"] = ("button", "text-button"),
		["Label"] = ("label", null),
		["Image"] = ("picture", null),
		["Switch"] = ("switch", null),
		["Slider"] = ("scale", null),
	};

	public static bool TryGetNodeName(string typeName, out (string nodeName, string? autoStyleClass) mapping)
	{
		if (_map.TryGetValue(typeName, out mapping))
		{
			return true;
		}

		mapping = (null!, null);
		return false;
	}
}