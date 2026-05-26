using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Mesen.Config
{
	public partial class ProfilerConfig : BaseWindowConfig<ProfilerConfig>
	{
		[ObservableProperty] public partial List<int> ColumnWidths { get; set; } = new();
		[ObservableProperty] public partial bool AutoRefresh { get; set; } = true;
		[ObservableProperty] public partial bool RefreshOnBreakPause { get; set; } = true;
	}
}
