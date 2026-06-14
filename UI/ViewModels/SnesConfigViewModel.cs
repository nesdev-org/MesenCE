using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Localization;
using Mesen.Utilities;
using System;

namespace Mesen.ViewModels
{
	public partial class SnesConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial SnesConfig Config { get; set; }
		[ObservableProperty] public partial SnesConfig OriginalConfig { get; set; }
		[ObservableProperty] public partial SnesConfigTab SelectedTab { get; set; } = 0;

		public SnesInputConfigViewModel Input { get; private set; }

		[ObservableProperty] public partial bool IsDefaultSpcClockSpeed { get; set; } = true;
		[ObservableProperty] public partial string SpcEffectiveClockSpeed { get; set; } = "";

		public Enum[] AvailableRegions => new Enum[] {
			ConsoleRegion.Auto,
			ConsoleRegion.Ntsc,
			ConsoleRegion.Pal
		};

		public SnesConfigViewModel()
		{
			Config = ConfigManager.Config.Snes;
			OriginalConfig = Config.Clone();
			Input = new SnesInputConfigViewModel(Config);

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(Input);
			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));

			AddDisposable(Config.ObserveProp(nameof(SnesConfig.SpcClockSpeedAdjustment), () => {
				SpcEffectiveClockSpeed = ResourceHelper.GetMessage("SpcClockSpeedMsg", ((32000 + Config.SpcClockSpeedAdjustment) * 32).ToString());
				IsDefaultSpcClockSpeed = Config.SpcClockSpeedAdjustment == 40;
			}));
		}
	}

	public enum SnesConfigTab
	{
		General,
		Audio,
		Emulation,
		Input,
		Overclocking,
		Video,
		Bsx
	}
}
