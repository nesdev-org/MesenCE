using CommunityToolkit.Mvvm.ComponentModel;
using HarfBuzzSharp;
using Mesen.Config;
using Mesen.Utilities;
using System;

namespace Mesen.ViewModels
{
	public partial class ConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial AudioConfigViewModel? Audio { get; set; }
		[ObservableProperty] public partial InputConfigViewModel? Input { get; set; }
		[ObservableProperty] public partial VideoConfigViewModel? Video { get; set; }
		[ObservableProperty] public partial PreferencesConfigViewModel? Preferences { get; set; }
		[ObservableProperty] public partial EmulationConfigViewModel? Emulation { get; set; }

		[ObservableProperty] public partial SnesConfigViewModel? Snes { get; set; }
		[ObservableProperty] public partial NesConfigViewModel? Nes { get; set; }
		[ObservableProperty] public partial GameboyConfigViewModel? Gameboy { get; set; }
		[ObservableProperty] public partial GbaConfigViewModel? Gba { get; set; }
		[ObservableProperty] public partial PceConfigViewModel? PcEngine { get; set; }
		[ObservableProperty] public partial SmsConfigViewModel? Sms { get; set; }
		[ObservableProperty] public partial WsConfigViewModel? Ws { get; set; }
		[ObservableProperty] public partial OtherConsolesConfigViewModel? OtherConsoles { get; set; }

		[ObservableProperty] public partial ConfigWindowTab SelectedIndex { get; set; }
		public bool AlwaysOnTop { get; }

		[Obsolete("For designer only")]
		public ConfigViewModel() : this(ConfigWindowTab.Audio) { }

		public ConfigViewModel(ConfigWindowTab selectedTab)
		{
			AlwaysOnTop = ConfigManager.Config.Preferences.AlwaysOnTop;
			SelectTab(selectedTab);
		}

		partial void OnSelectedIndexChanged(ConfigWindowTab value)
		{
			SelectTab(value);
		}

		public void SelectTab(ConfigWindowTab tab)
		{
			//Create each view model when the corresponding tab is clicked, for performance
			switch(tab) {
				case ConfigWindowTab.Audio: Audio ??= AddDisposable(new AudioConfigViewModel()); break;
				case ConfigWindowTab.Emulation: Emulation ??= AddDisposable(new EmulationConfigViewModel()); break;
				case ConfigWindowTab.Input: Input ??= AddDisposable(new InputConfigViewModel()); break;
				case ConfigWindowTab.Video: Video ??= AddDisposable(new VideoConfigViewModel()); break;

				case ConfigWindowTab.Nes:
					//TODOv2 fix this patch
					Preferences ??= AddDisposable(new PreferencesConfigViewModel());
					Nes ??= AddDisposable(new NesConfigViewModel(Preferences.Config));
					break;

				case ConfigWindowTab.Snes: Snes ??= AddDisposable(new SnesConfigViewModel()); break;
				case ConfigWindowTab.Gameboy: Gameboy ??= AddDisposable(new GameboyConfigViewModel()); break;
				case ConfigWindowTab.Gba: Gba ??= AddDisposable(new GbaConfigViewModel()); break;
				case ConfigWindowTab.PcEngine: PcEngine ??= AddDisposable(new PceConfigViewModel()); break;
				case ConfigWindowTab.Sms: Sms ??= AddDisposable(new SmsConfigViewModel()); break;
				case ConfigWindowTab.Ws: Ws ??= AddDisposable(new WsConfigViewModel()); break;
				case ConfigWindowTab.OtherConsoles: OtherConsoles ??= AddDisposable(new OtherConsolesConfigViewModel()); break;

				case ConfigWindowTab.Preferences: Preferences ??= AddDisposable(new PreferencesConfigViewModel()); break;
			}

			SelectedIndex = tab;
		}

		public void SaveConfig()
		{
			ConfigManager.Config.ApplyConfig();
			ConfigManager.Config.Save();
			ConfigManager.Config.Preferences.UpdateFileAssociations();
		}

		public void RevertConfig()
		{
			ConfigManager.Config.Audio = Audio?.OriginalConfig ?? ConfigManager.Config.Audio;
			ConfigManager.Config.Input = Input?.OriginalConfig ?? ConfigManager.Config.Input;
			ConfigManager.Config.Video = Video?.OriginalConfig ?? ConfigManager.Config.Video;
			ConfigManager.Config.Preferences = Preferences?.OriginalConfig ?? ConfigManager.Config.Preferences;
			ConfigManager.Config.Emulation = Emulation?.OriginalConfig ?? ConfigManager.Config.Emulation;
			ConfigManager.Config.Nes = Nes?.OriginalConfig ?? ConfigManager.Config.Nes;
			ConfigManager.Config.Snes = Snes?.OriginalConfig ?? ConfigManager.Config.Snes;
			ConfigManager.Config.Gameboy = Gameboy?.OriginalConfig ?? ConfigManager.Config.Gameboy;
			ConfigManager.Config.Gba = Gba?.OriginalConfig ?? ConfigManager.Config.Gba;
			ConfigManager.Config.PcEngine = PcEngine?.OriginalConfig ?? ConfigManager.Config.PcEngine;
			ConfigManager.Config.Sms = Sms?.OriginalConfig ?? ConfigManager.Config.Sms;
			ConfigManager.Config.Cv = OtherConsoles?.CvOriginalConfig ?? ConfigManager.Config.Cv;
			ConfigManager.Config.ApplyConfig();
			ConfigManager.Config.Save();
		}

		public bool IsDirty()
		{
			return (
				Audio?.OriginalConfig.IsIdentical(ConfigManager.Config.Audio) == false ||
				Input?.OriginalConfig.IsIdentical(ConfigManager.Config.Input) == false ||
				Video?.OriginalConfig.IsIdentical(ConfigManager.Config.Video) == false ||
				Preferences?.OriginalConfig.IsIdentical(ConfigManager.Config.Preferences) == false ||
				Emulation?.OriginalConfig.IsIdentical(ConfigManager.Config.Emulation) == false ||
				Nes?.OriginalConfig.IsIdentical(ConfigManager.Config.Nes) == false ||
				Snes?.OriginalConfig.IsIdentical(ConfigManager.Config.Snes) == false ||
				Gameboy?.OriginalConfig.IsIdentical(ConfigManager.Config.Gameboy) == false ||
				Gba?.OriginalConfig.IsIdentical(ConfigManager.Config.Gba) == false ||
				PcEngine?.OriginalConfig.IsIdentical(ConfigManager.Config.PcEngine) == false ||
				Sms?.OriginalConfig.IsIdentical(ConfigManager.Config.Sms) == false ||
				Ws?.OriginalConfig.IsIdentical(ConfigManager.Config.Ws) == false ||
				OtherConsoles?.CvOriginalConfig.IsIdentical(ConfigManager.Config.Cv) == false
			);
		}
	}

	public enum ConfigWindowTab
	{
		Audio = 0,
		Emulation = 1,
		Input = 2,
		Video = 3,
		//separator
		Nes = 5,
		Snes = 6,
		Gameboy = 7,
		Gba = 8,
		PcEngine = 9,
		Sms = 10,
		Ws = 11,
		OtherConsoles = 12,
		//separator
		Preferences = 14
	}
}
