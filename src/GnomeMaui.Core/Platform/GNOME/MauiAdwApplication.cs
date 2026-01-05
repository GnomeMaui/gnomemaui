using Gio;
using GObject;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui;

/// <summary>
/// Defines the core behavior of a .NET MAUI application running on GNOME.
/// </summary>
public abstract class MauiAdwApplication : Adw.Application, IPlatformApplication
{
	/// <summary>
	/// The application assembly containing embedded resources (e.g., reset.css).
	/// </summary>
	public static System.Reflection.Assembly? ApplicationAssembly { get; private set; }

	/// <summary>
	/// When overridden in a derived class, creates the <see cref="MauiApp"/> to be used in this application.
	/// Typically a <see cref="MauiApp"/> is created by calling <see cref="MauiApp.CreateBuilder(bool)"/>, configuring
	/// the returned <see cref="MauiAppBuilder"/>, and returning the built app by calling <see cref="MauiAppBuilder.Build"/>.
	/// </summary>
	/// <returns>The built <see cref="MauiApp"/>.</returns>
	protected abstract MauiApp CreateMauiApp();

	protected MauiAdwApplication(string applicationId, System.Reflection.Assembly applicationAssembly)
	{
		ApplicationId = applicationId;
		ApplicationAssembly = applicationAssembly;
		Current = this;
		IPlatformApplication.Current = this;

		// Subscribe to Gtk.Application events
		OnQueryEnd += OnQueryEndHandler;
		OnWindowAdded += OnWindowAddedHandler;
		OnWindowRemoved += OnWindowRemovedHandler;

		// Subscribe to Gio.Application events
		OnActivate += OnActivateHandler;
		// OnCommandLine += OnCommandLineHandler;
		// OnHandleLocalOptions += OnHandleLocalOptionsHandler;
		// OnNameLost += OnNameLostHandler;
		OnOpen += OnOpenHandler;
		OnStartup += OnStartupHandler;
		OnShutdown += OnShutdownHandler;
	}

	// bool OnNameLostHandler(Application sender, EventArgs args)
	// {
	// 	_services?.InvokeLifecycleEvents<GNOMELifecycle.OnNameLost>(del => del(this, true));
	// 	return false;
	// }

	// int OnHandleLocalOptionsHandler(Application sender, HandleLocalOptionsSignalArgs args)
	// {
	// 	_services?.InvokeLifecycleEvents<GNOMELifecycle.OnHandleLocalOptions>(del => del(this, args));
	// 	return 0;
	// }

	// int OnCommandLineHandler(Application sender, CommandLineSignalArgs args)
	// {
	// 	_services?.InvokeLifecycleEvents<GNOMELifecycle.OnCommandLine>(del => del(this, args));
	// 	return 0;
	// }

	void OnQueryEndHandler(Gtk.Application sender, EventArgs args)
		=> _services?.InvokeLifecycleEvents<GNOMELifecycle.OnQueryEnd>(del => del(this, args));

	void OnStartupHandler(Application sender, EventArgs args)
	{
		var mauiApp = CreateMauiApp();

		var rootContext = new MauiContext(mauiApp.Services);

		_applicationContext = rootContext.MakeApplicationScope(this);

		_services = _applicationContext.Services;

		_services?.InvokeLifecycleEvents<GNOMELifecycle.OnStartup>(del => del(this, args));
	}

	void OnActivateHandler(Application sender, EventArgs args)
	{
		if (_application == null && _services != null && _applicationContext != null)
		{
			_application = _services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(_application, _applicationContext);

			this.CreatePlatformWindow(_application);
		}

		_services?.InvokeLifecycleEvents<GNOMELifecycle.OnActivate>(del => del(this, args));
	}

	void OnOpenHandler(Application sender, Gio.Application.OpenSignalArgs args)
	{
		_services?.InvokeLifecycleEvents<GNOMELifecycle.OnOpen>(del => del(this, args));
	}

	void OnShutdownHandler(Application sender, EventArgs args)
	{
		_services?.InvokeLifecycleEvents<GNOMELifecycle.OnShutdown>(del => del(this, args));
	}

	void OnWindowAddedHandler(Application sender, Gtk.Application.WindowAddedSignalArgs args)
	{
		_services?.InvokeLifecycleEvents<GNOMELifecycle.OnWindowAdded>(del => del(this, args));
	}

	void OnWindowRemovedHandler(Application sender, Gtk.Application.WindowRemovedSignalArgs args)
	{
		_services?.InvokeLifecycleEvents<GNOMELifecycle.OnWindowRemoved>(del => del(this, args));
	}


	public static MauiAdwApplication Current { get; private set; } = default!;

	IServiceProvider? _services;

	IApplication? _application;

	IMauiContext? _applicationContext;

	// TODO: we should investigate throwing an exception or changing the public API
	IServiceProvider IPlatformApplication.Services => _services!;

	IApplication IPlatformApplication.Application => _application!;

	// TODO NET9 MARK THESE AS OBSOLETE. We didn't mark them obsolete in NET8 because that
	// was causing warnings to generate for our WinUI projects, so we need to workaround that
	// before we mark this as obsolete.
	/// <summary>
	/// Use the IPlatformApplication.Current.Services instead.
	/// </summary>
	public IServiceProvider Services
	{
		get => _services!;
		protected set => _services = value;
	}

	// TODO NET9 MARK THESE AS OBSOLETE. We didn't mark them obsolete in NET8 because that
	// was causing warnings to generate for our WinUI projects, so we need to workaround that
	// before we mark this as obsolete.
	/// <summary>
	/// Use the IPlatformApplication.Current.Application instead.
	/// </summary>
	public IApplication Application
	{
		get => _application!;
		protected set => _application = value;
	}

	public override void Dispose()
	{
		OnActivate -= OnActivateHandler;
		OnStartup -= OnStartupHandler;
		OnOpen -= OnOpenHandler;
		OnShutdown -= OnShutdownHandler;
		OnWindowAdded -= OnWindowAddedHandler;
		OnWindowRemoved -= OnWindowRemovedHandler;
		base.Dispose();
		if (Current == this)
		{
			Current = null!;
			IPlatformApplication.Current = null!;
		}
		GC.SuppressFinalize(this);
	}
}
