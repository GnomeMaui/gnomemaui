using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using System;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : SkiaGraphicsView
	{
		IGraphicsView? _graphicsView;
		RectF _bounds;
		bool _dragStarted;
		PointF[] _lastMovedViewPoints = Array.Empty<PointF>();
		bool _pressedContained;

		GestureDrag? _drag;
		EventControllerScroll? _scroll;
		GestureClick? _click;
		EventControllerMotion? _motion;

		public PlatformTouchGraphicsView()
		{
			_drag = GestureDrag.New();
			_scroll = EventControllerScroll.New(EventControllerScrollFlags.Vertical);
			_click = GestureClick.New();
			_motion = EventControllerMotion.New();
		}

		protected override void Resize(int width, int height)
		{
			_bounds = new RectF(0, 0, width, height);
		}

		public void Connect(IGraphicsView graphicsView)
		{
			_graphicsView = graphicsView;
			AttachEventHandlers();
		}

		public void Disconnect()
		{
			DetachEventHandlers();
			_graphicsView = null;
		}

		void AttachEventHandlers()
		{
			if (_click == null || _motion == null || _drag == null || _scroll == null)
			{
				return;
			}

			DetachEventHandlers();

			_click.OnPressed += OnPressed;
			_click.OnReleased += OnRelease;
			AddController(_click);

			_drag.OnDragBegin += OnDragBegin;
			_drag.OnDragUpdate += OnDragUpdate;
			_drag.OnDragEnd += OnDragEnd;
			AddController(_drag);

			_motion.OnMotion += OnMotion;
			_motion.OnLeave += OnLeave;
			_motion.OnEnter += OnEnter;
			AddController(_motion);

			_scroll.OnScroll += OnScroll;
			AddController(_scroll);
		}

		void DetachEventHandlers()
		{
			if (_click != null)
			{
				_click.OnPressed -= OnPressed;
				_click.OnReleased -= OnRelease;
				if (_click.GetWidget() == this)
					RemoveController(_click);
			}

			if (_drag != null)
			{
				_drag.OnDragBegin -= OnDragBegin;
				_drag.OnDragUpdate -= OnDragUpdate;
				_drag.OnDragEnd -= OnDragEnd;
				if (_drag.GetWidget() == this)
					RemoveController(_drag);
			}

			if (_motion != null)
			{
				_motion.OnMotion -= OnMotion;
				_motion.OnLeave -= OnLeave;
				_motion.OnEnter -= OnEnter;
				if (_motion.GetWidget() == this)
					RemoveController(_motion);
			}

			if (_scroll != null)
			{
				_scroll.OnScroll -= OnScroll;
				if (_scroll.GetWidget() == this)
					RemoveController(_scroll);
			}
		}

		void OnEnter(EventControllerMotion sender, EventControllerMotion.EnterSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			var point = new PointF((float)args.X, (float)args.Y);
			_graphicsView.StartHoverInteraction(new[] { point });
		}

		void OnLeave(EventControllerMotion sender, EventArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			_graphicsView.EndHoverInteraction();
		}

		void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			var point = new PointF((float)args.X, (float)args.Y);
			_graphicsView.MoveHoverInteraction(new[] { point });
		}

		bool OnScroll(EventControllerScroll sender, EventControllerScroll.ScrollSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return false;
			}

			return false;
		}

		void OnPressed(GestureClick sender, GestureClick.PressedSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			var point = new PointF((float)args.X, (float)args.Y);
			TouchesBegan(new[] { point });
		}

		void OnRelease(GestureClick sender, GestureClick.ReleasedSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			var point = new PointF((float)args.X, (float)args.Y);
			TouchesEnded(new[] { point });
		}

		void OnDragBegin(GestureDrag sender, GestureDrag.DragBeginSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			var point = new PointF((float)args.StartX, (float)args.StartY);
			TouchesBegan(new[] { point });
		}

		void OnDragUpdate(GestureDrag sender, GestureDrag.DragUpdateSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			sender.GetPoint(null, out double currentX, out double currentY);
			var point = new PointF((float)currentX, (float)currentY);
			TouchesMoved(new[] { point });
		}

		void OnDragEnd(GestureDrag sender, GestureDrag.DragEndSignalArgs args)
		{
			if (_graphicsView == null || !_graphicsView.IsEnabled)
			{
				return;
			}

			sender.GetPoint(null, out double currentX, out double currentY);
			var point = new PointF((float)currentX, (float)currentY);
			TouchesEnded(new[] { point });
		}

		void TouchesBegan(PointF[] points)
		{
			_dragStarted = false;
			_lastMovedViewPoints = points;
			_graphicsView?.StartInteraction(points);
			_pressedContained = true;
		}

		void TouchesMoved(PointF[] points)
		{
			if (!_dragStarted)
			{
				if (points.Length == 1)
				{
					float deltaX = _lastMovedViewPoints[0].X - points[0].X;
					float deltaY = _lastMovedViewPoints[0].Y - points[0].Y;

					if (MathF.Abs(deltaX) <= 3 && MathF.Abs(deltaY) <= 3)
						return;
				}
			}

			_lastMovedViewPoints = points;
			_dragStarted = true;
			_pressedContained = _bounds.ContainsAny(points);
			_graphicsView?.DragInteraction(points);
		}

		void TouchesEnded(PointF[] points)
		{
			_graphicsView?.EndInteraction(points, _pressedContained);
		}

		void TouchesCanceled()
		{
			_pressedContained = false;
			_graphicsView?.CancelInteraction();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DetachEventHandlers();

				_click?.Dispose();
				_click = null;

				_drag?.Dispose();
				_drag = null;

				_motion?.Dispose();
				_motion = null;

				_scroll?.Dispose();
				_scroll = null;
			}

			base.Dispose(disposing);
		}
	}
}