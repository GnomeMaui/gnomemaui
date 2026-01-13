using Microsoft.Maui.ApplicationModel;
using System.Text;

namespace Microsoft.Maui.Networking;

partial class ConnectivityImplementation : IConnectivity
{
	Gio.NetworkMonitor? _networkMonitor;

	void StartListeners()
	{
		_networkMonitor = Gio.NetworkMonitorHelper.GetDefault();
		if (_networkMonitor is not null)
		{
			_networkMonitor.OnNetworkChanged += OnNetworkMonitorChanged;
		}
	}

	void StopListeners()
	{
		if (_networkMonitor is not null)
		{
			_networkMonitor.OnNetworkChanged -= OnNetworkMonitorChanged;
			_networkMonitor = null;
		}
	}

	void OnNetworkMonitorChanged(Gio.NetworkMonitor sender, Gio.NetworkMonitor.NetworkChangedSignalArgs args)
	{
		OnConnectivityChanged();
	}

	public NetworkAccess NetworkAccess
	{
		get
		{
			var monitor = _networkMonitor ?? Gio.NetworkMonitorHelper.GetDefault();
			if (monitor is null)
				return NetworkAccess.Unknown;

			if (!monitor.GetNetworkAvailable())
				return NetworkAccess.None;

			var connectivity = monitor.GetConnectivity();
			return connectivity switch
			{
				Gio.NetworkConnectivity.Local => NetworkAccess.Local,
				Gio.NetworkConnectivity.Limited => NetworkAccess.ConstrainedInternet,
				Gio.NetworkConnectivity.Portal => NetworkAccess.ConstrainedInternet,
				Gio.NetworkConnectivity.Full => NetworkAccess.Internet,
				_ => NetworkAccess.Unknown
			};
		}
	}

	public IEnumerable<ConnectionProfile> ConnectionProfiles
	{
		get
		{
			var profiles = new List<ConnectionProfile>();
			var monitor = _networkMonitor ?? Gio.NetworkMonitorHelper.GetDefault();

			if (monitor is null || !monitor.GetNetworkAvailable())
			{
				profiles.Add(ConnectionProfile.Unknown);
				return profiles;
			}

			// GNOME/GIO nem biztosít részletes információt a kapcsolat típusáról (Wi-Fi, Ethernet, stb.)
			// Az NetworkMonitor csak azt mutatja, hogy van-e hálózati kapcsolat
			// A metered jelző alapján próbálhatjuk megkülönböztetni a mobilhálózatot
			if (monitor.GetNetworkMetered())
			{
				profiles.Add(ConnectionProfile.Cellular);
			}
			else
			{
				// Ha nem metered, akkor feltételezhetően vezetékes vagy Wi-Fi kapcsolat
				// De sajnos a GNetworkMonitor nem teszi lehetővé a pontos megkülönböztetést
				profiles.Add(ConnectionProfile.Unknown);
			}

			return profiles;
		}
	}
}