using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Mesen.Config
{
	public partial class RefreshTimingConfig : BaseConfig<RefreshTimingConfig>
	{
		[ObservableProperty] public partial Dictionary<CpuType, RefreshTimingConsoleConfig> ConsoleConfig { get; set; } = new();
		[ObservableProperty] public partial bool RefreshOnBreakPause { get; set; } = true;
		[ObservableProperty] public partial bool AutoRefresh { get; set; } = true;

		public RefreshTimingConfig()
		{
		}

		public RefreshTimingConsoleConfig GetConsoleConfig(CpuType cpuType)
		{
			if(!ConsoleConfig.TryGetValue(cpuType, out RefreshTimingConsoleConfig? config)) {
				config = new();
				ConsoleConfig[cpuType] = config;
			}
			return config;
		}
	}

	public partial class RefreshTimingConsoleConfig : BaseConfig<RefreshTimingConsoleConfig>
	{
		[ObservableProperty] public partial int RefreshScanline { get; set; } = 240;
		[ObservableProperty] public partial int RefreshCycle { get; set; } = 0;

		public RefreshTimingConsoleConfig()
		{
		}
	}
}
