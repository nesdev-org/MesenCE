using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Mesen.Config
{
	public partial class EventViewerConfig : BaseWindowConfig<EventViewerConfig>
	{
		[ObservableProperty] public partial bool ShowSettingsPanel { get; set; } = true;

		[ObservableProperty] public partial double ImageScale { get; set; } = 1;

		[ObservableProperty] public partial bool RefreshOnBreakPause { get; set; } = true;
		[ObservableProperty] public partial bool AutoRefresh { get; set; } = true;
		[ObservableProperty] public partial RefreshTimingConfig RefreshTiming { get; set; } = new();

		[ObservableProperty] public partial List<int> ColumnWidths { get; set; } = new();

		[ObservableProperty] public partial bool ShowToolbar { get; set; } = true;

		[ObservableProperty] public partial bool ShowListView { get; set; } = true;
		[ObservableProperty] public partial double ListViewHeight { get; set; } = 200;

		[ObservableProperty] public partial SnesEventViewerConfig SnesConfig { get; set; } = new SnesEventViewerConfig();
		[ObservableProperty] public partial NesEventViewerConfig NesConfig { get; set; } = new NesEventViewerConfig();
		[ObservableProperty] public partial GbEventViewerConfig GbConfig { get; set; } = new GbEventViewerConfig();
		[ObservableProperty] public partial GbaEventViewerConfig GbaConfig { get; set; } = new GbaEventViewerConfig();
		[ObservableProperty] public partial PceEventViewerConfig PceConfig { get; set; } = new PceEventViewerConfig();
		[ObservableProperty] public partial SmsEventViewerConfig SmsConfig { get; set; } = new SmsEventViewerConfig();
		[ObservableProperty] public partial WsEventViewerConfig WsConfig { get; set; } = new WsEventViewerConfig();
	}
}
