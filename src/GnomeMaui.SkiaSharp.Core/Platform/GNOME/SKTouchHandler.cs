using Gtk;
using Microsoft.Maui;
using SkiaSharp;
using System;

namespace SkiaSharp.Views.Maui.Platform;

class SKTouchHandler
{
	Action<SKTouchEventArgs>? onTouchAction;
	Func<double, double, SKPoint>? scalePixels;

	GestureDrag drag = Gtk.GestureDrag.New();
	EventControllerScroll scroll = Gtk.EventControllerScroll.New(Gtk.EventControllerScrollFlags.Vertical);
	GestureClick click = Gtk.GestureClick.New();
	EventControllerMotion motion = Gtk.EventControllerMotion.New();
	GestureZoom zoom = Gtk.GestureZoom.New();

	public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
	{
		this.onTouchAction = onTouchAction;
		this.scalePixels = scalePixels;
	}

	public void SetEnabled(Widget view, bool enableTouchEvents)
	{
		if (view == null)
		{
			return;
		}

		// Detach first to avoid duplicate handlers
		if (click.GetWidget() == view)
			view.RemoveController(click);
		if (motion.GetWidget() == view)
			view.RemoveController(motion);
		if (zoom.GetWidget() == view)
			view.RemoveController(zoom);
		if (scroll.GetWidget() == view)
			view.RemoveController(scroll);
		if (drag.GetWidget() == view)
			view.RemoveController(drag);

		click.OnPressed -= OnPressed;
		//click.OnUpdate -= OnUpdate;
		click.OnReleased -= OnRelease;

		drag.OnDragBegin -= OnDragBegin;
		drag.OnDragUpdate -= OnDragUpdate;
		drag.OnDragEnd -= OnDragEnd;

		motion.OnMotion -= OnMotion;
		motion.OnLeave -= OnLeave;

		zoom.OnScaleChanged -= OnScaleChanged;
		scroll.OnScroll -= OnScroll;

		if (enableTouchEvents)
		{
			click.OnPressed += OnPressed;
			//click.OnUpdate += OnUpdate;
			click.OnReleased += OnRelease;
			view.AddController(click);

			drag.OnDragBegin += OnDragBegin;
			drag.OnDragUpdate += OnDragUpdate;
			drag.OnDragEnd += OnDragEnd;
			view.AddController(drag);

			motion.OnMotion += OnMotion;
			motion.OnLeave += OnLeave;
			view.AddController(motion);

			zoom.OnScaleChanged += OnScaleChanged;
			view.AddController(zoom);

			scroll.OnScroll += OnScroll;
			view.AddController(scroll);
		}
	}

	public void Detach(Widget view)
	{
		if (view == null)
		{
			return;
		}

		if (click.GetWidget() == view)
			view.RemoveController(click);
		if (motion.GetWidget() == view)
			view.RemoveController(motion);
		if (zoom.GetWidget() == view)
			view.RemoveController(zoom);
		if (scroll.GetWidget() == view)
			view.RemoveController(scroll);
		if (drag.GetWidget() == view)
			view.RemoveController(drag);

		click.OnPressed -= OnPressed;
		//click.OnUpdate -= OnUpdate;
		click.OnReleased -= OnRelease;

		drag.OnDragBegin -= OnDragBegin;
		drag.OnDragUpdate -= OnDragUpdate;
		drag.OnDragEnd -= OnDragEnd;

		motion.OnMotion -= OnMotion;
		motion.OnLeave -= OnLeave;

		zoom.OnScaleChanged -= OnScaleChanged;
		scroll.OnScroll -= OnScroll;

		click.Dispose();
		drag.Dispose();
		motion.Dispose();
		zoom.Dispose();
		scroll.Dispose();

		onTouchAction = null;
		scalePixels = null;
	}

	bool OnScroll(EventControllerScroll sender, EventControllerScroll.ScrollSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return false;
		}

		int wheel = Convert.ToInt32(args.Dy);

		var pt = new SKPoint(0, 0);
		var skpt = new SKPoint(pt.X, pt.Y);
		var ev = new SKTouchEventArgs(0, SKTouchAction.WheelChanged, SKMouseButton.Unknown, SKTouchDeviceType.Mouse, skpt, true, wheel);
		onTouchAction(ev);
		return ev.Handled;
	}

	void OnScaleChanged(GestureZoom sender, GestureZoom.ScaleChangedSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		var pt = new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.WheelChanged, SKMouseButton.Unknown, SKTouchDeviceType.Touch, pt, false);
		onTouchAction(ev);
	}

	void OnLeave(EventControllerMotion sender, EventArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		var pt = new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Exited, pt, false);
		onTouchAction(ev);
	}

	void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		var skpt = scalePixels?.Invoke(args.X, args.Y) ?? new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Moved, SKMouseButton.Unknown, SKTouchDeviceType.Touch, skpt, false);
		onTouchAction(ev);
	}

	void OnRelease(GestureClick sender, GestureClick.ReleasedSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		var skpt = scalePixels?.Invoke(args.X, args.Y) ?? new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Released, SKMouseButton.Left, SKTouchDeviceType.Mouse, skpt, false);
		onTouchAction(ev);
	}

	// void OnUpdate(Gesture sender, Gesture.UpdateSignalArgs args)
	// {
	// 	if (onTouchAction == null || scalePixels == null)
	// 	{
	// 		return;
	// 	}

	// 	var skpt = new SKPoint(0, 0);
	// 	var ev = new SKTouchEventArgs(0, SKTouchAction.Moved, skpt, true);
	// 	onTouchAction(ev);
	// }

	void OnPressed(GestureClick sender, GestureClick.PressedSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		var skpt = scalePixels?.Invoke(args.X, args.Y) ?? new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Pressed, SKMouseButton.Left, SKTouchDeviceType.Mouse, skpt, true);
		onTouchAction(ev);
	}

	void OnDragEnd(GestureDrag sender, GestureDrag.DragEndSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		sender.GetPoint(null, out double currentX, out double currentY);

		var skpt = scalePixels?.Invoke(currentX, currentY) ?? new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Released, SKMouseButton.Left, SKTouchDeviceType.Mouse, skpt, false);
		onTouchAction(ev);
	}

	void OnDragUpdate(GestureDrag sender, GestureDrag.DragUpdateSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		sender.GetPoint(null, out double currentX, out double currentY);

		var skpt = scalePixels?.Invoke(currentX, currentY) ?? new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Moved, SKMouseButton.Left, SKTouchDeviceType.Mouse, skpt, true);
		onTouchAction(ev);
	}

	void OnDragBegin(GestureDrag sender, GestureDrag.DragBeginSignalArgs args)
	{
		if (onTouchAction == null || scalePixels == null)
		{
			return;
		}

		var skpt = scalePixels?.Invoke(args.StartX, args.StartY) ?? new SKPoint(0, 0);
		var ev = new SKTouchEventArgs(0, SKTouchAction.Pressed, SKMouseButton.Left, SKTouchDeviceType.Mouse, skpt, true);
		onTouchAction(ev);
	}
}
