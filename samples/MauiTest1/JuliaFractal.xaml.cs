using System;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiTest1;

public partial class JuliaFractal : ContentPage, IDisposable
{
	const int MinIterations = 10;
	const int MaxIterations = 500;
	const float MinZoom = 0.1f;
	const float MaxZoom = 10.0f;

	static string GenerateShaderSource(int maxIterations, float maxZoom) => $@"
uniform vec2 u_resolution;
uniform float u_time;
uniform float u_zoom;
uniform float u_maxIterations;
uniform vec2 u_julia_c;

vec3 palette(float t) {{
    vec3 a = vec3(0.5, 0.5, 0.5);
    vec3 b = vec3(0.5, 0.5, 0.5);
    vec3 c = vec3(1.0, 1.0, 1.0);
    vec3 d = vec3(0.263, 0.416, 0.557);
    return a + b * cos(6.28318 * (c * t + d));
}}

vec4 main(vec2 fragCoord) {{
    vec2 uv = (fragCoord - u_resolution * 0.5) / min(u_resolution.x, u_resolution.y);
    uv *= u_zoom;
    
    vec2 z = uv;
    vec2 c = u_julia_c;
    
    float iterations = 0.0;
    
    for (float i = 0.0; i < {maxIterations}.0; i++) {{
        if (i >= u_maxIterations) break;
        
        float x = z.x * z.x - z.y * z.y + c.x;
        float y = 2.0 * z.x * z.y + c.y;
        z = vec2(x, y);
        
        if (length(z) > 4.0) {{
            iterations = i;
            break;
        }}
    }}
    
    float t = iterations / u_maxIterations;
    vec3 color = palette(t + u_time * 0.1);
    
    return vec4(color, 1.0);
}}";

	readonly SKFont _statFont = new(SKTypeface.FromFamilyName("Open Sans"), 12);
	readonly SKPaint _statPaint = new() { IsAntialias = false };

	readonly SKRuntimeEffect _juliaEffect = default!;
	double _time = 0;
	float _zoom = 2.0f;
	int _currentIterations = 100;
	uint _animationTimerId = 0;
	long _lastFrameTick = Environment.TickCount64;
	double _fps = 0;

	public double CurrentReal => -0.7 + (0.3 * Math.Sin(_time * 0.7));
	public double CurrentImaginary => 0.27 + (0.3 * Math.Cos(_time * 0.5));

	public JuliaFractal()
	{
		InitializeComponent();

		var shaderSource = GenerateShaderSource(MaxIterations, MaxZoom);
		_juliaEffect = SKRuntimeEffect.CreateShader(shaderSource, out string errors);

		if (_juliaEffect == null)
		{
			Console.Error.WriteLine($"Shader compilation error: {errors}");
		}

		_animationTimerId = GLib.Functions.TimeoutAdd(
			priority: GLib.Constants.PRIORITY_DEFAULT_IDLE,
			interval: 16,
			function: new GLib.SourceFunc(() =>
			{
				_time += 0.02;
				skiaView.InvalidateSurface();
				return true;
			}));
	}

	void OnTouch(object sender, SKTouchEventArgs e)
	{
		if (e.ActionType == SKTouchAction.WheelChanged)
		{
			_zoom *= e.WheelDelta > 0 ? 1.1f : 0.9f;
			_zoom = Math.Clamp(_zoom, MinZoom, MaxZoom);
			skiaView.InvalidateSurface();
			e.Handled = true;
		}
	}

	void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Black);

		if (_juliaEffect == null)
		{
			return;
		}

		var width = e.Info.Width;
		var height = e.Info.Height;

		var uniforms = new SKRuntimeEffectUniforms(_juliaEffect)
		{
			["u_resolution"] = new float[] { width, height },
			["u_time"] = (float)_time,
			["u_zoom"] = _zoom,
			["u_maxIterations"] = (float)_currentIterations,
			["u_julia_c"] = new float[] { (float)CurrentReal, (float)CurrentImaginary }
		};

		using var shader = _juliaEffect.ToShader(uniforms);
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(0, 0, width, height, paint);

		var now = Environment.TickCount64;
		var delta = now - _lastFrameTick;
		_lastFrameTick = now;
		_fps = delta > 0 ? 1000.0 / delta : 0;

		_statPaint.Color = SKColors.Gray;
		canvas.DrawText($"Zoom: {_zoom:F2} [{MinZoom}-{MaxZoom}] | Iter: {_currentIterations} [{MinIterations}-{MaxIterations}] | FPS: {_fps:F1}",
			10, 20, _statFont, _statPaint);
	}

	public void Dispose()
	{
		if (_animationTimerId != 0)
		{
			GLib.Functions.SourceRemove(_animationTimerId);
			_animationTimerId = 0;
		}
		_juliaEffect?.Dispose();
		_statFont?.Dispose();
		_statPaint?.Dispose();
		GC.SuppressFinalize(this);
	}
}
