using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Mesen.Config
{
	public partial class HistoryViewerConfig : BaseWindowConfig<HistoryViewerConfig>
	{
		[ObservableProperty] public partial int Volume { get; set; } = 100;
	}
}
