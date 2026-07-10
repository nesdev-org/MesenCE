using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Collections.Generic;

namespace Mesen.ViewModels
{
	public partial class AudioConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial AudioConfig Config { get; set; }
		[ObservableProperty] public partial AudioConfig OriginalConfig { get; set; }
		[ObservableProperty] public partial List<string> AudioDevices { get; set; } = new();
		[ObservableProperty] public partial bool ShowLatencyWarning { get; set; } = false;

		public AudioConfigViewModel()
		{
			Config = ConfigManager.Config.Audio;
			OriginalConfig = Config.Clone();

			if(Design.IsDesignMode) {
				return;
			}

			AudioDevices = ConfigApi.GetAudioDevices();
			if(AudioDevices.Count > 0 && !AudioDevices.Contains(Config.AudioDevice)) {
				Config.AudioDevice = AudioDevices[0];
			}

			AddDisposable(Config.ObserveProp(nameof(Config.AudioLatency), () => {
				ShowLatencyWarning = Config.AudioLatency <= 55;
			}));

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}
	}
}
