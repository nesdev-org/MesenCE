using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.Config
{
	public partial class PaletteViewerConfig : BaseWindowConfig<PaletteViewerConfig>
	{
		[ObservableProperty] public partial bool ShowSettingsPanel { get; set; } = true;
		[ObservableProperty] public partial bool ShowPaletteIndexes { get; set; } = false;
		[ObservableProperty] public partial int Zoom { get; set; } = 3;

		[ObservableProperty] public partial RefreshTimingConfig RefreshTiming { get; set; } = new();

		public PaletteViewerConfig()
		{
		}
	}
}
