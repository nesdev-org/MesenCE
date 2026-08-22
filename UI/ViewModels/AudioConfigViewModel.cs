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
		[ObservableProperty] public partial bool IsLatencyWarning { get; set; } = false;

		public bool IsWindows { get; }

		public AudioConfigViewModel()
		{
			Config = ConfigManager.Config.Audio;
			OriginalConfig = Config.Clone();

			if(Design.IsDesignMode) {
				return;
			}

			IsWindows = OperatingSystem.IsWindows();

			UpdateAudioDevices();

			AddDisposable(Config.ObserveProp(nameof(Config.AudioLatency), () => {
				UpdateLatencyWarning();
			}));

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => {
				Config.ApplyConfig();
				if(e.PropertyName == nameof(Config.WindowsAudio)) {
					UpdateAudioDevices();
					UpdateLatencyWarning();
				}
			}));
		}

		private void UpdateLatencyWarning()
		{
			int warningLimit = IsWindows && Config.WindowsAudio == WindowsAudioBackend.Wasapi ? 25 : 55;
			IsLatencyWarning = Config.AudioLatency < warningLimit;
		}

		private void UpdateAudioDevices()
		{
			AudioDevices = ConfigApi.GetAudioDevices();
			if(AudioDevices.Count > 0 && !AudioDevices.Contains(Config.AudioDevice)) {
				Config.AudioDevice = AudioDevices[0];
			}
		}
	}
}
