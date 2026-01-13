using System.Text;

namespace Microsoft.Maui.Controls.Platform;

public static class LabelExtensions
{
	public static void UpdateLineBreakMode(this Gtk.Label platformLabel, Label label)
	{
		switch (label.LineBreakMode)
		{
			case LineBreakMode.NoWrap:
				platformLabel.Wrap = false;
				platformLabel.Ellipsize = Pango.EllipsizeMode.None;
				break;
			case LineBreakMode.WordWrap:
				platformLabel.Wrap = true;
				platformLabel.WrapMode = Pango.WrapMode.Word;
				platformLabel.Ellipsize = Pango.EllipsizeMode.None;
				platformLabel.NaturalWrapMode = Gtk.NaturalWrapMode.Word;
				break;
			case LineBreakMode.CharacterWrap:
				platformLabel.Wrap = true;
				platformLabel.WrapMode = Pango.WrapMode.Char;
				platformLabel.Ellipsize = Pango.EllipsizeMode.None;
				platformLabel.NaturalWrapMode = Gtk.NaturalWrapMode.Word;
				break;
			case LineBreakMode.HeadTruncation:
				platformLabel.Wrap = false;
				platformLabel.Ellipsize = Pango.EllipsizeMode.Start;
				break;
			case LineBreakMode.MiddleTruncation:
				platformLabel.Wrap = false;
				platformLabel.Ellipsize = Pango.EllipsizeMode.Middle;
				break;
			case LineBreakMode.TailTruncation:
				platformLabel.Wrap = false;
				platformLabel.Ellipsize = Pango.EllipsizeMode.End;
				break;
		}
	}

	public static void UpdateMaxLines(this Gtk.Label platformLabel, Label label)
	{
		platformLabel.Lines = label.MaxLines;
	}
}
