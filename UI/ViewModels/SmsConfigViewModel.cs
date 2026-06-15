using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Controls;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mesen.ViewModels
{
	public partial class SmsConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial SmsConfig Config { get; set; }
		[ObservableProperty] public partial SmsConfig OriginalConfig { get; set; }
		[ObservableProperty] public partial SmsConfigTab SelectedTab { get; set; } = 0;

		public SmsInputConfigViewModel Input { get; private set; }

		public Enum[] AvailableRegionsSms => new Enum[] {
			ConsoleRegion.Auto,
			ConsoleRegion.Ntsc,
			ConsoleRegion.Pal
		};

		public Enum[] AvailableRegionsGg => new Enum[] {
			ConsoleRegion.Auto,
			ConsoleRegion.Ntsc,
			ConsoleRegion.NtscJapan,
			ConsoleRegion.Pal
		};

		public SmsConfigViewModel()
		{
			Config = ConfigManager.Config.Sms;
			OriginalConfig = Config.Clone();
			Input = new SmsInputConfigViewModel(Config);

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(Input);
			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}
	}

	public enum SmsConfigTab
	{
		General,
		Audio,
		Emulation,
		Input,
		Video
	}
}
