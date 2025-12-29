using Microsoft.Maui.ApplicationModel;
using System;

namespace Microsoft.Maui.Devices;

class DeviceInfoImplementation : IDeviceInfo
{

	public string Model => throw ExceptionUtils.NotSupportedOrImplementedException;

	public string Manufacturer => throw ExceptionUtils.NotSupportedOrImplementedException;

	public string Name => throw ExceptionUtils.NotSupportedOrImplementedException;

	public string VersionString => throw ExceptionUtils.NotSupportedOrImplementedException;

	public Version Version => throw ExceptionUtils.NotSupportedOrImplementedException;

	static DevicePlatform GNOME { get; } = DevicePlatform.Create(nameof(GNOME));

	public DevicePlatform Platform => GNOME;

	public DeviceIdiom Idiom => DeviceIdiom.Desktop;

	public DeviceType DeviceType => DeviceType.Physical;

}