using Maui.Controls.Sample;
using Maui.Controls.Sample.Platform;
using System.Runtime.Versioning;

MauiProgram.StartTime = Environment.TickCount64;

Adw.Module.Initialize();
Gtk.Module.Initialize();

Console.WriteLine($"After GirCore Module Initialize: {Environment.TickCount64 - MauiProgram.StartTime} ms");

var app = new AdwApplication();
Console.WriteLine($"Before app.Run: {Environment.TickCount64 - MauiProgram.StartTime} ms");

app.Run(args);
