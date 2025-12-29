namespace DrawnUi.Draw;

public partial class SkiaImageManager
{
	public static async Task<SKBitmap> LoadImageOnPlatformAsync(ImageSource source, CancellationToken cancel)
	{
		if (source == null)
			return null;

		try
		{
			if (source is StreamImageSource streamSource)
			{
				using (var stream = await streamSource.Stream(cancel))
				{
					return SKBitmap.Decode(stream);
				}
			}
			else if (source is UriImageSource uriSource)
			{
				return await LoadImageFromInternetAsync(uriSource, cancel);
			}
			else if (source is FileImageSource fileSource)
			{
				return await LoadFromFile(fileSource.File, cancel);
			}
			else
			{
#if DEBUG
				Console.Out.WriteLine($"[SkiaImageManager][LoadImageOnPlatformAsync] Unsupported ImageSource type: {source.GetType().Name}");
#endif
			}
		}
		catch (TaskCanceledException)
		{
			SkiaImageManager.TraceLog($"[LoadImageOnPlatformAsync] TaskCanceledException for {source}");
		}
		catch (Exception e)
		{
			Super.Log($"[LoadImageOnPlatformAsync] {e}");
		}

		SkiaImageManager.TraceLog($"[LoadImageOnPlatformAsync] loaded NULL for {source}");
		return null;
	}
}
