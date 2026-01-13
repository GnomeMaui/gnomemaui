#nullable disable

namespace Microsoft.Maui.Controls.Handlers.Items;

public partial class CollectionViewHandler : ReorderableItemsViewHandler<ReorderableItemsView>
{
	// CollectionView is the final handler that combines all functionality:
	// - ItemsViewHandler: Base functionality with ItemsSource, ItemTemplate, EmptyView
	// - StructuredItemsViewHandler: Header/Footer support and layout management
	// - SelectableItemsViewHandler: Selection support (Single, Multiple, None)
	// - GroupableItemsViewHandler: Grouping support with TreeListModel
	// - ReorderableItemsViewHandler: Drag & drop reordering support

	// No additional implementation needed here, all functionality is inherited
}
