using DrawnUi.Controls;
using DrawnUi.Views;
using SkiaSharp.Views.Maui.Handlers;

namespace DrawnUi.Draw
{
	public static partial class DrawnExtensions
	{
		public static void ConfigureHandlers(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(SkiaViewAccelerated), typeof(SKCanvasViewHandler));
			handlers.AddHandler(typeof(SkiaView), typeof(SKCanvasViewHandler));
			handlers.AddHandler(typeof(MauiEntry), typeof(MauiEntryHandler));
			handlers.AddHandler(typeof(MauiEditor), typeof(MauiEditorHandler));
		}
	}
}
