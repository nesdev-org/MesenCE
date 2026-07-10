using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.Config
{
	public partial class TileViewerConfig : BaseWindowConfig<TileViewerConfig>
	{
		[ObservableProperty] public partial bool ShowSettingsPanel { get; set; } = true;

		[ObservableProperty] public partial double ImageScale { get; set; } = 3;
		[ObservableProperty] public partial bool ShowTileGrid { get; set; } = false;

		[ObservableProperty] public partial string SelectedPreset { get; set; } = "PPU";

		[ObservableProperty] public partial MemoryType Source { get; set; }
		[ObservableProperty] public partial TileFormat Format { get; set; } = TileFormat.Bpp4;
		[ObservableProperty] public partial TileLayout Layout { get; set; } = TileLayout.Normal;
		[ObservableProperty] public partial TileFilter Filter { get; set; } = TileFilter.None;
		[ObservableProperty] public partial TileBackground Background { get; set; } = TileBackground.Default;
		[ObservableProperty] public partial int RowCount { get; set; } = 64;
		[ObservableProperty] public partial int ColumnCount { get; set; } = 32;
		[ObservableProperty] public partial int StartAddress { get; set; } = 0;
		[ObservableProperty] public partial bool UseGrayscalePalette { get; set; } = false;

		[ObservableProperty] public partial RefreshTimingConfig RefreshTiming { get; set; } = new();

		public TileViewerConfig()
		{
		}
	}
}
