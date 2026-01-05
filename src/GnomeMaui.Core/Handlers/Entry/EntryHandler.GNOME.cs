namespace Microsoft.Maui.Handlers;

public partial class EntryHandler : ViewHandler<IEntry, Gtk.Entry>
{
	protected override Gtk.Entry CreatePlatformView()
		=> this.Create();

	public static void MapText(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateText(entry);

	public static void MapTextColor(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateTextColor(entry);

	public static void MapIsPassword(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateIsPassword(entry);

	public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

	public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateVerticalTextAlignment(entry);

	/// <summary>
	/// Maps the abstract <see cref="ITextInput.IsTextPredictionEnabled"/> property to the platform-specific implementations.
	/// </summary>
	/// <param name="handler"> The associated handler.</param>
	/// <param name="entry"> The associated <see cref="IEntry"/> instance.</param>
	public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateIsTextPredictionEnabled(entry);

	/// <summary>
	/// Maps the abstract <see cref="ITextInput.IsSpellCheckEnabled"/> property to the platform-specific implementations.
	/// </summary>
	/// <param name="handler"> The associated handler.</param>
	/// <param name="entry"> The associated <see cref="IEntry"/> instance.</param>
	public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateIsSpellCheckEnabled(entry);

	public static void MapMaxLength(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateMaxLength(entry);

	public static void MapPlaceholder(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdatePlaceholder(entry);

	public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdatePlaceholderColor(entry);

	public static void MapIsReadOnly(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateIsReadOnly(entry);

	public static void MapKeyboard(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateKeyboard(entry);

	public static void MapFont(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateFont(entry);

	public static void MapReturnType(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateReturnType(entry);

	public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateClearButtonVisibility(entry);

	public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateCharacterSpacing(entry);

	public static void MapCursorPosition(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateCursorPosition(entry);

	public static void MapSelectionLength(IEntryHandler handler, IEntry entry)
		=> handler.PlatformView?.UpdateSelectionLength(entry);
}