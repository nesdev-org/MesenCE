using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Mesen.Config
{
	public partial class RegisterViewerConfig : BaseWindowConfig<RegisterViewerConfig>
	{
		[ObservableProperty] public partial RefreshTimingConfig RefreshTiming { get; set; } = new();
		[ObservableProperty] public partial List<int> ColumnWidths { get; set; } = new();
	}
}
