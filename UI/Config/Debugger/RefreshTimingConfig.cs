using Mesen.Interop;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;

namespace Mesen.Config
{
	public class RefreshTimingConfig : BaseConfig<RefreshTimingConfig>
	{
		[Reactive] public Dictionary<CpuType, RefreshTimingConsoleConfig> ConsoleConfig { get; set; } = new();
		[Reactive] public bool RefreshOnBreakPause { get; set; } = true;
		[Reactive] public bool AutoRefresh { get; set; } = true;

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

	public class RefreshTimingConsoleConfig : BaseConfig<RefreshTimingConfig>
	{
		[Reactive] public int RefreshScanline { get; set; } = 240;
		[Reactive] public int RefreshCycle { get; set; } = 0;

		public RefreshTimingConsoleConfig()
		{
		}
	}
}
