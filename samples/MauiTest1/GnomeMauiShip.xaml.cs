using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiTest1;

public partial class GnomeMauiShip : ContentPage
{
	record struct Star(float X, float Y, float Z, float Size);

	const int StarCount = 2000;
	const float Depth = 1200f;

	readonly Star[] _stars = new Star[StarCount];
	readonly Random _rng = new(1);

	float _speed = 480f; // units/sec
	long _lastTicks;

	public GnomeMauiShip()
	{
		InitializeComponent();
		InitStars();
	}

	void InitStars()
	{
		for (int i = 0; i < _stars.Length; i++)
			_stars[i] = NewStar(z: 1f + (float)_rng.NextDouble() * (Depth - 1f));
	}

	Star NewStar(float z)
	{
		float x = (float)(_rng.NextDouble() * 2 - 1);
		float y = (float)(_rng.NextDouble() * 2 - 1);

		// more stars toward the edges
		x *= x < 0 ? -x : x;
		y *= y < 0 ? -y : y;

		float size = 0.6f + (float)_rng.NextDouble() * 1.8f;
		return new Star(x, y, z, size);
	}

	void OnStarsPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;

		canvas.Clear(SKColors.Transparent);

		long ticks = Environment.TickCount64;
		if (_lastTicks == 0) _lastTicks = ticks;

		float dt = (ticks - _lastTicks) / 1000f;
		_lastTicks = ticks;

		if (dt > 0.05f) dt = 0.05f;

		float cx = info.Width * 0.5f;
		float cy = info.Height * 0.5f;

		float fov = MathF.Min(info.Width, info.Height) * 0.9f;

		using var paint = new SKPaint
		{
			IsAntialias = true
		};

		for (int i = 0; i < _stars.Length; i++)
		{
			var s = _stars[i];

			// DIRECTION: stars move "backwards", towards the center point
			s.Z += _speed * dt;

			// if it went too far, respawn closer
			if (s.Z >= Depth)
			{
				s = NewStar(z: 1f + (float)_rng.NextDouble() * 40f);
			}

			// perspective projection (KEEP THIS, this is what made it good)
			float p = fov / s.Z;
			float x = cx + s.X * p * cx;
			float y = cy + s.Y * p * cy;

			// if it goes out of the image, throw it back closer
			if (x < -50 || x > info.Width + 50 || y < -50 || y > info.Height + 50)
			{
				s = NewStar(z: 1f + (float)_rng.NextDouble() * 40f);
				_stars[i] = s;
				continue;
			}

			// brightness by depth (closer is brighter, far is dimmer)
			byte a = (byte)Math.Clamp(255f * (1f - (s.Z / Depth)), 20f, 255f);

			// size by perspective
			float r = s.Size * (0.6f + (1f - s.Z / Depth) * 0.5f);

			paint.Color = new SKColor(255, 255, 255, a);
			canvas.DrawCircle(x, y, r, paint);

			_stars[i] = s;
		}
	}
}
