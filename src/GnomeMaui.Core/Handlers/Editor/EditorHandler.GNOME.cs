namespace Microsoft.Maui.Handlers;

public partial class EditorHandler : ViewHandler<IEditor, Gtk.TextView>
{
	protected override Gtk.TextView CreatePlatformView()
		=> this.Create();

	public static void MapText(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdateText(editor);

	public static void MapTextColor(IEditorHandler handler, IEditor editor)
		=> handler.PlatformView?.UpdateTextColor(editor);

	public static void MapPlaceholder(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdatePlaceholder(editor);

	public static void MapPlaceholderColor(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdatePlaceholderColor(editor);

	public static void MapCharacterSpacing(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdateCharacterSpacing(editor);

	public static void MapMaxLength(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdateMaxLength(editor);

	/// <summary>
	/// Maps the abstract <see cref="ITextInput.IsTextPredictionEnabled"/> property to the platform-specific implementations.
	/// </summary>
	/// <param name="handler"> The associated handler.</param>
	/// <param name="editor"> The associated <see cref="IEditor"/> instance.</param>
	public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor)
		=> handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

	/// <summary>
	/// Maps the abstract <see cref="ITextInput.IsSpellCheckEnabled"/> property to the platform-specific implementations.
	/// </summary>
	/// <param name="handler"> The associated handler.</param>
	/// <param name="editor"> The associated <see cref="IEditor"/> instance.</param>
	public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor)
		=> handler.PlatformView?.UpdateIsSpellCheckEnabled(editor);

	public static void MapFont(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdateFont(editor);

	public static void MapIsReadOnly(IViewHandler handler, IEditor editor)
		=> (handler as IEditorHandler)?.PlatformView?.UpdateIsReadOnly(editor);

	public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor)
		=> handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

	public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor)
		=> handler.PlatformView?.UpdateVerticalTextAlignment(editor);

	public static void MapKeyboard(IEditorHandler handler, IEditor editor)
		=> handler.PlatformView?.UpdateKeyboard(editor);

	public static void MapCursorPosition(IEditorHandler handler, ITextInput editor)
		=> handler.PlatformView?.UpdateCursorPosition(editor);

	public static void MapSelectionLength(IEditorHandler handler, ITextInput editor)
		=> handler.PlatformView?.UpdateSelectionLength(editor);
}
