using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Mesen.Config
{
	public partial class SpriteViewerConfig : BaseWindowConfig<SpriteViewerConfig>
	{
		[ObservableProperty] public partial bool ShowSettingsPanel { get; set; } = true;

		[ObservableProperty] public partial bool ShowOutline { get; set; } = false;
		[ObservableProperty] public partial bool ShowOffscreenRegions { get; set; } = false;
		[ObservableProperty] public partial SpriteBackground Background { get; set; } = SpriteBackground.Gray;

		[ObservableProperty] public partial SpriteViewerSource Source { get; set; } = SpriteViewerSource.SpriteRam;
		[ObservableProperty] public partial int SourceOffset { get; set; } = 0;

		[ObservableProperty] public partial bool DimOffscreenSprites { get; set; } = true;
		[ObservableProperty] public partial bool ShowListView { get; set; } = false;
		[ObservableProperty] public partial double ListViewHeight { get; set; } = 100;
		[ObservableProperty] public partial List<int> ColumnWidths { get; set; } = new();

		[ObservableProperty] public partial double ImageScale { get; set; } = 2;
		[ObservableProperty] public partial RefreshTimingConfig RefreshTiming { get; set; } = new();

		public SpriteViewerConfig()
		{
		}
	}

	public enum SpriteViewerSource
	{
		SpriteRam,
		CpuMemory
	}
}
