namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeThickness(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashPattern(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashOffset(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeMiterLimit(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineCap(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineJoin(this ContentWidget platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		internal static void UpdateMauiDrawable(this WrapperView wrapperView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (!hasBorder)
				return;

			wrapperView.UpdateBorder(border);
		}
	}
}
