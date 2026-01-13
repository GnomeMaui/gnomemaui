using System;

namespace Microsoft.Maui.Graphics.Platform;

public class PlatformImage : IImage
{
	public float Width => throw new NotImplementedException();

	public float Height => throw new NotImplementedException();

	public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
	{
		throw new NotImplementedException();
	}

	public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
	{
		throw new NotImplementedException();
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		throw new NotImplementedException();
	}

	public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
	{
		throw new NotImplementedException();
	}

	public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
	{
		throw new NotImplementedException();
	}

	public Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
	{
		throw new NotImplementedException();
	}

	public IImage ToPlatformImage()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		throw new NotImplementedException();
	}
	public static IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png)
	{
		throw new NotImplementedException();
	}
}
