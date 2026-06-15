using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;

namespace Mesen.Debugger.ViewModels
{
	public partial class RefreshTimingViewModel : ViewModelBase
	{
		public RefreshTimingConfig Config { get; }
		public RefreshTimingConsoleConfig ConsoleConfig { get; }
		[ObservableProperty] public partial int RefreshCycle { get; private set; }
		[ObservableProperty] public partial int RefreshScanline { get; private set; }

		[ObservableProperty] public partial int MinScanline { get; private set; }
		[ObservableProperty] public partial int MaxScanline { get; private set; }
		[ObservableProperty] public partial int MaxCycle { get; private set; }

		public IRelayCommand ResetCommand { get; }

		private CpuType _cpuType;

		[Obsolete("For designer only")]
		public RefreshTimingViewModel() : this(new RefreshTimingConfig(), CpuType.Snes) { }

		public RefreshTimingViewModel(RefreshTimingConfig config, CpuType cpuType)
		{
			Config = config;
			ConsoleConfig = config.GetConsoleConfig(cpuType);
			_cpuType = cpuType;

			UpdateMinMaxValues(_cpuType);
			ResetCommand = new RelayCommand(Reset);
			UpdateMinMax();

			RefreshCycle = ConsoleConfig.RefreshCycle;
			RefreshScanline = ConsoleConfig.RefreshScanline;
		}

		partial void OnRefreshCycleChanged(int value)
		{
			UpdateMinMax();
		}

		partial void OnRefreshScanlineChanged(int value)
		{
			UpdateMinMax();
		}

		private void UpdateMinMax()
		{
			//Manually enforce min/max to avoid issues when switching from one console type to another where the UI
			//could end up setting the new console's scanline value to the max scanline value of the previous console
			//(presumably due to the order in which the property bindings were processed)
			RefreshScanline = Math.Max(MinScanline, Math.Min(MaxScanline, RefreshScanline));
			RefreshCycle = Math.Max(0, Math.Min(MaxCycle, RefreshCycle));

			ConsoleConfig.RefreshCycle = RefreshCycle;
			ConsoleConfig.RefreshScanline = RefreshScanline;
		}

		public void Reset()
		{
			RefreshScanline = _cpuType.GetConsoleType() switch {
				ConsoleType.Snes => 240,
				ConsoleType.Nes => 241,
				ConsoleType.Gameboy => 144,
				ConsoleType.PcEngine => 240, //TODOv2
				ConsoleType.Sms => 192,
				ConsoleType.Gba => 160,
				ConsoleType.Ws => 144,
				_ => throw new Exception("Invalid console type")
			};

			RefreshCycle = 0;
		}

		private void UpdateMinMaxValues(CpuType cpuType)
		{
			_cpuType = cpuType;
			TimingInfo timing = EmuApi.GetTimingInfo(_cpuType);
			MinScanline = timing.FirstScanline;
			MaxScanline = (int)timing.ScanlineCount + timing.FirstScanline - 1;
			MaxCycle = (int)timing.CycleCount - 1;

			if(ConsoleConfig.RefreshScanline < MinScanline || ConsoleConfig.RefreshScanline > MaxScanline || ConsoleConfig.RefreshCycle > MaxCycle) {
				Reset();
			}
		}
	}
}
