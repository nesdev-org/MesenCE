using Mesen.Config;
using Mesen.Interop;
using Mesen.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Mesen.Debugger.ViewModels
{
	public class RefreshTimingViewModel : ViewModelBase
	{
		public RefreshTimingConfig Config { get; }
		public RefreshTimingConsoleConfig ConsoleConfig { get; }
		[Reactive] public int MinScanline { get; private set; }
		[Reactive] public int MaxScanline { get; private set; }
		[Reactive] public int MaxCycle { get; private set; }

		public ReactiveCommand<Unit, Unit> ResetCommand { get; }

		private CpuType _cpuType;

		[Obsolete("For designer only")]
		public RefreshTimingViewModel() : this(new RefreshTimingConfig(), CpuType.Snes) { }

		public RefreshTimingViewModel(RefreshTimingConfig config, CpuType cpuType)
		{
			Config = config;
			ConsoleConfig = config.GetConsoleConfig(cpuType);
			_cpuType = cpuType;

			UpdateMinMaxValues(_cpuType);
			ResetCommand = ReactiveCommand.Create(Reset);

			ConsoleConfig.WhenAnyValue(x => x.RefreshScanline, x => x.RefreshCycle).Subscribe(x => UpdateMinMax());
		}

		private void UpdateMinMax()
		{
			//Manually enforce min/max to avoid issues when switching from one console type to another where the UI
			//could end up setting the new console's scanline value to the max scanline value of the previous console
			//(presumably due to the order in which the property bindings were processed)
			ConsoleConfig.RefreshScanline = Math.Max(MinScanline, Math.Min(MaxScanline, ConsoleConfig.RefreshScanline));
			ConsoleConfig.RefreshCycle = Math.Max(0, Math.Min(MaxCycle, ConsoleConfig.RefreshCycle));
		}

		public void Reset()
		{
			ConsoleConfig.RefreshScanline = _cpuType.GetConsoleType() switch {
				ConsoleType.Snes => 240,
				ConsoleType.Nes => 241,
				ConsoleType.Gameboy => 144,
				ConsoleType.PcEngine => 240, //TODOv2
				ConsoleType.Sms => 192,
				ConsoleType.Gba => 160,
				ConsoleType.Ws => 144,
				_ => throw new Exception("Invalid console type")
			};

			ConsoleConfig.RefreshCycle = 0;
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
