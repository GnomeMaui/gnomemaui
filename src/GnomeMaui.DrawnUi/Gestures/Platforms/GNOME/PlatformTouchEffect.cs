
using Gtk;
using Microsoft.Maui.Controls.Platform;

namespace AppoMobi.Maui.Gestures;

public partial class PlatformTouchEffect : PlatformEffect
{
	Widget? _widget;
	GestureDrag? _drag;
	EventControllerScroll? _scroll;
	EventControllerMotion? _motion;
	bool _isButtonPressed = false;
	PointF _lastMousePosition = new(0, 0);

	public float ScaleLimitMin { get; set; } = 0.1f;
	public float ScaleLimitMax { get; set; } = 1000.0f;
	public float ScaleFactor { get; set; } = 1.0f;

	protected override void OnAttached()
	{
		_widget = Control ?? Container;

		FormsEffect = Element.Effects.FirstOrDefault(e => e is TouchEffect) as TouchEffect;

		if (FormsEffect != null && _widget != null)
		{
			_drag = GestureDrag.New();
			_scroll = EventControllerScroll.New(
				EventControllerScrollFlags.Vertical |
				EventControllerScrollFlags.Horizontal |
				EventControllerScrollFlags.Kinetic);
			_motion = EventControllerMotion.New();

			_drag.OnDragBegin += OnDragBegin;
			_drag.OnDragUpdate += OnDragUpdate;
			_drag.OnDragEnd += OnDragEnd;
			_widget.AddController(_drag);

			_motion.OnMotion += OnMotion;
			_motion.OnLeave += OnLeave;
			_motion.OnEnter += OnEnter;
			_widget.AddController(_motion);

			_scroll.OnScroll += OnScroll;
			_widget.AddController(_scroll);

			FormsEffect.Disposing += OnFormsDisposing;
			Element.HandlerChanged += OnHandlerChanged;
		}
	}

	void OnHandlerChanged(object? sender, EventArgs e)
	{
		if (sender is Element element && element.Handler == null)
		{
			element.HandlerChanged -= OnHandlerChanged;
			OnDetached();
		}
	}

	void OnFormsDisposing(object? sender, EventArgs e)
	{
		OnDetached();
	}

	protected override void OnDetached()
	{
		if (_widget != null)
		{
			if (_drag != null)
			{
				_drag.OnDragBegin -= OnDragBegin;
				_drag.OnDragUpdate -= OnDragUpdate;
				_drag.OnDragEnd -= OnDragEnd;
				if (_drag.GetWidget() == _widget)
				{
					_widget.RemoveController(_drag);
				}
				_drag.Dispose();
				_drag = null;
			}

			if (_motion != null)
			{
				_motion.OnMotion -= OnMotion;
				_motion.OnLeave -= OnLeave;
				_motion.OnEnter -= OnEnter;
				if (_motion.GetWidget() == _widget)
				{
					_widget.RemoveController(_motion);
				}
				_motion.Dispose();
				_motion = null;
			}

			if (_scroll != null)
			{
				_scroll.OnScroll -= OnScroll;
				if (_scroll.GetWidget() == _widget)
				{
					_widget.RemoveController(_scroll);
				}
				_scroll.Dispose();
				_scroll = null;
			}

			_widget = null;
		}

		if (FormsEffect != null)
		{
			FormsEffect.Disposing -= OnFormsDisposing;
			FormsEffect.Dispose();
			FormsEffect = null;
		}
	}

	void OnEnter(EventControllerMotion sender, EventControllerMotion.EnterSignalArgs args)
	{
		CountFingers = 1;
		isInsideView = true;
		_lastMousePosition = new PointF((float)args.X, (float)args.Y);
		FireEvent(0, TouchActionType.Entered, _lastMousePosition);
	}

	void OnLeave(EventControllerMotion sender, EventArgs args)
	{
		CountFingers = 0;
		isInsideView = false;
		FireEvent(0, TouchActionType.Exited, _lastMousePosition);
	}

	bool OnScroll(EventControllerScroll sender, EventControllerScroll.ScrollSignalArgs args)
	{
		var isMouse = false;
		var isTouchpad = false;
		var currentEvent = sender.GetCurrentEvent();
		if (currentEvent != null)
		{
			var device = currentEvent.GetDevice();
			if (device != null)
			{
				var source = device.GetSource();
				isTouchpad = source == Gdk.InputSource.Touchpad;
				isMouse = source == Gdk.InputSource.Mouse;
			}
		}

		CountFingers = 1;
		isInsideView = true;

		var wheelDelta = (float)args.Dy * (isMouse ? -0.0333f : -10f);

		float scaleFactorAdjustment = wheelDelta < 0 ? 1.05f : 0.95f;
		ScaleFactor = Math.Max(ScaleLimitMin, Math.Min(ScaleFactor * scaleFactorAdjustment, ScaleLimitMax));

		var location = _lastMousePosition;

		Wheel = new TouchEffect.WheelEventArgs
		{
			Delta = wheelDelta,
			Scale = ScaleFactor,
			Center = location
		};

		FireEvent(0, TouchActionType.Wheel, location);
		return false;
	}

	void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args)
	{
		_lastMousePosition = new PointF((float)args.X, (float)args.Y);

		if (!isInsideView)
			return;

		if (!_isButtonPressed)
			return;

		CountFingers = 1;
		FireEvent(0, TouchActionType.Moved, _lastMousePosition);
	}

	void OnDragEnd(GestureDrag sender, GestureDrag.DragEndSignalArgs args)
	{
		_isButtonPressed = false;
		sender.GetPoint(null, out double currentX, out double currentY);
		var location = new PointF((float)currentX, (float)currentY);
		FireEvent(0, TouchActionType.Released, location);
		CountFingers = 0;
	}

	void OnDragUpdate(GestureDrag sender, GestureDrag.DragUpdateSignalArgs args)
	{
		CountFingers = 1;
		sender.GetPoint(null, out double currentX, out double currentY);
		var location = new PointF((float)currentX, (float)currentY);
		FireEvent(0, TouchActionType.Moved, location);
	}

	void OnDragBegin(GestureDrag sender, GestureDrag.DragBeginSignalArgs args)
	{
		_isButtonPressed = true;
		CountFingers = 1;
		isInsideView = true;
		var location = new PointF((float)args.StartX, (float)args.StartY);
		FireEvent(0, TouchActionType.Pressed, location);
	}

	void FireEvent(int id, TouchActionType actionType, PointF pointerLocation)
	{
		var args = new TouchActionEventArgs(id, actionType, pointerLocation, null)
		{
			Wheel = Wheel,
			NumberOfTouches = CountFingers,
			IsInsideView = isInsideView
		};

		FormsEffect?.OnTouchAction(args);
	}
}
