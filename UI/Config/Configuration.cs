using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config.Shortcuts;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Mesen.Config
{
	public partial class Configuration : ObservableObject
	{
		private string _fileData = "";

		public string Version { get; set; } = "2.2.1";
		public int ConfigUpgrade { get; set; } = 0;
		public bool EnableTestMode { get; set; } = false;

		[ObservableProperty] public partial VideoConfig Video { get; set; } = new();
		[ObservableProperty] public partial AudioConfig Audio { get; set; } = new();
		[ObservableProperty] public partial InputConfig Input { get; set; } = new();
		[ObservableProperty] public partial EmulationConfig Emulation { get; set; } = new();
		[ObservableProperty] public partial SnesConfig Snes { get; set; } = new();
		[ObservableProperty] public partial NesConfig Nes { get; set; } = new();
		[ObservableProperty] public partial GameboyConfig Gameboy { get; set; } = new();
		[ObservableProperty] public partial PcEngineConfig PcEngine { get; set; } = new();
		[ObservableProperty] public partial SmsConfig Sms { get; set; } = new();
		[ObservableProperty] public partial CvConfig Cv { get; set; } = new();
		[ObservableProperty] public partial GbaConfig Gba { get; set; } = new();
		[ObservableProperty] public partial WsConfig Ws { get; set; } = new();
		[ObservableProperty] public partial PreferencesConfig Preferences { get; set; } = new();
		[ObservableProperty] public partial AudioPlayerConfig AudioPlayer { get; set; } = new();
		[ObservableProperty] public partial DebugConfig Debug { get; set; } = new();
		[ObservableProperty] public partial RecentItems RecentFiles { get; set; } = new();
		[ObservableProperty] public partial VideoRecordConfig VideoRecord { get; set; } = new();
		[ObservableProperty] public partial MovieRecordConfig MovieRecord { get; set; } = new();
		[ObservableProperty] public partial HdPackBuilderConfig HdPackBuilder { get; set; } = new();
		[ObservableProperty] public partial CheatWindowConfig Cheats { get; set; } = new();
		[ObservableProperty] public partial NetplayConfig Netplay { get; set; } = new();
		[ObservableProperty] public partial HistoryViewerConfig HistoryViewer { get; set; } = new();
		[ObservableProperty] public partial MainWindowConfig MainWindow { get; set; } = new();

		public DefaultKeyMappingType DefaultKeyMappings { get; set; } = DefaultKeyMappingType.Xbox | DefaultKeyMappingType.ArrowKeys;

		public Configuration()
		{
			//Used by JSON deserializer, don't call directly - use CreateConfig
		}

		public static Configuration CreateConfig()
		{
			Configuration cfg = new();
			cfg.ConfigUpgrade = (int)ConfigUpgradeHint.FirstRun;
			return cfg;
		}

		~Configuration()
		{
			//Try to save before destruction if we were unable to save at a previous point in time
			Save();
		}

		public void Save()
		{
			if(ConfigManager.DisableSaveSettings) {
				//Don't save to disk if command line option to disable setting updates was set
				return;
			}

			Serialize(ConfigManager.ConfigFile);
		}

		public void ApplyConfig()
		{
			Video.ApplyConfig();
			Audio.ApplyConfig();
			Input.ApplyConfig();
			Emulation.ApplyConfig();
			Gameboy.ApplyConfig();
			Gba.ApplyConfig();
			PcEngine.ApplyConfig();
			Nes.ApplyConfig();
			Snes.ApplyConfig();
			Sms.ApplyConfig();
			Cv.ApplyConfig();
			Ws.ApplyConfig();
			Preferences.ApplyConfig();
			AudioPlayer.ApplyConfig();
			Debug.ApplyConfig();
		}

		public void InitializeFontDefaults()
		{
			if(ConfigUpgrade == (int)ConfigUpgradeHint.FirstRun) {
				Preferences.InitializeFontDefaults();

				Debug.Fonts.DisassemblyFont = GetDefaultMonospaceFont();
				Debug.Fonts.MemoryViewerFont = GetDefaultMonospaceFont();
				Debug.Fonts.AssemblerFont = GetDefaultMonospaceFont();
				Debug.Fonts.ScriptWindowFont = GetDefaultMonospaceFont();
				Debug.Fonts.OtherMonoFont = GetDefaultMonospaceFont(true);
				Debug.Fonts.ApplyConfig();
			}
		}

		public void UpgradeConfig()
		{
			if(ConfigUpgrade < (int)ConfigUpgradeHint.SmsInput) {
				Sms.InitializeDefaults(DefaultKeyMappings);
			}

			if(ConfigUpgrade < (int)ConfigUpgradeHint.GbaInput) {
				Gba.InitializeDefaults(DefaultKeyMappings);
			}

			if(ConfigUpgrade < (int)ConfigUpgradeHint.CvInput) {
				Cv.InitializeDefaults(DefaultKeyMappings);
			}

			if(ConfigUpgrade < (int)ConfigUpgradeHint.WsInput) {
				Ws.InitializeDefaults(DefaultKeyMappings);
			}

			ConfigUpgrade = (int)ConfigUpgradeHint.NextValue - 1;
			Version = EmuApi.GetMesenVersion().ToString(3);
		}

		public void InitializeDefaults()
		{
			if(ConfigUpgrade == (int)ConfigUpgradeHint.FirstRun) {
				Snes.InitializeDefaults(DefaultKeyMappings);
				Nes.InitializeDefaults(DefaultKeyMappings);
				Gameboy.InitializeDefaults(DefaultKeyMappings);
				Gba.InitializeDefaults(DefaultKeyMappings);
				PcEngine.InitializeDefaults(DefaultKeyMappings);
				Sms.InitializeDefaults(DefaultKeyMappings);
				Cv.InitializeDefaults(DefaultKeyMappings);
				Ws.InitializeDefaults(DefaultKeyMappings);
				ConfigUpgrade = (int)ConfigUpgradeHint.NextValue - 1;
			}
			Preferences.InitializeDefaultShortcuts();
		}

		private static HashSet<string>? _installedFonts = null;
		private static List<string>? _sortedFonts = null;

		[MemberNotNull(nameof(_installedFonts), nameof(_sortedFonts))]
		private static void InitInstalledFonts()
		{
			_installedFonts = new();
			_sortedFonts = new();
			try {
				int count = FontManager.Current.SystemFonts.Count;
				for(int i = 0; i < count; i++) {
					try {
						string? fontName = FontManager.Current.SystemFonts[i]?.Name;
						if(!string.IsNullOrWhiteSpace(fontName)) {
							_installedFonts.Add(fontName);
						}
					} catch { }
				}

				_sortedFonts.AddRange(_installedFonts);
				_sortedFonts.Sort();
			} catch {
			}
		}

		public static List<string> GetSortedFontList()
		{
			if(_sortedFonts == null) {
				InitInstalledFonts();
			}
			return new List<string>(_sortedFonts);
		}

		private static string FindMatchingFont(string defaultFont, params string[] fontNames)
		{
			if(_installedFonts == null) {
				InitInstalledFonts();
			}

			foreach(string name in fontNames) {
				if(_installedFonts.Contains(name)) {
					return name;
				}
			}

			return defaultFont;
		}

		public static string GetValidFontFamily(string requestedFont, bool preferMonoFont)
		{
			if(_installedFonts == null) {
				InitInstalledFonts();
			}

			if(_installedFonts.Contains(requestedFont)) {
				return requestedFont;
			}

			foreach(string name in _installedFonts) {
				if(preferMonoFont && name.Contains("Mono", StringComparison.InvariantCultureIgnoreCase)) {
					return name;
				} else if(!preferMonoFont && name.Contains("Sans", StringComparison.InvariantCultureIgnoreCase)) {
					return name;
				}
			}

			return _installedFonts.First();
		}

		public static FontConfig GetDefaultFont()
		{
			if(OperatingSystem.IsWindows()) {
				return new FontConfig() { FontFamily = "Microsoft Sans Serif", FontSize = 11 };
			} else if(OperatingSystem.IsMacOS()) {
				return new FontConfig() { FontFamily = FindMatchingFont("Microsoft Sans Serif"), FontSize = 11 };
			} else {
				return new FontConfig() { FontFamily = FindMatchingFont("FreeSans", "DejaVu Sans", "Noto Sans"), FontSize = 11 };
			}
		}

		public static FontConfig GetDefaultMenuFont()
		{
			if(OperatingSystem.IsWindows()) {
				return new FontConfig() { FontFamily = "Segoe UI", FontSize = 12 };
			} else if(OperatingSystem.IsMacOS()) {
				return new FontConfig() { FontFamily = FindMatchingFont("Microsoft Sans Serif"), FontSize = 12 };
			} else {
				return new FontConfig() { FontFamily = FindMatchingFont("FreeSans", "DejaVu Sans", "Noto Sans"), FontSize = 12 };
			}
		}

		public static FontConfig GetDefaultMonospaceFont(bool useSmallFont = false)
		{
			if(OperatingSystem.IsWindows()) {
				return new FontConfig() { FontFamily = "Consolas", FontSize = useSmallFont ? 12 : 14 };
			} else if(OperatingSystem.IsMacOS()) {
				return new FontConfig() { FontFamily = FindMatchingFont("PT Mono"), FontSize = useSmallFont ? 11 : 12 };
			} else {
				return new FontConfig() { FontFamily = FindMatchingFont("FreeMono", "DejaVu Sans Mono", "Noto Sans Mono"), FontSize = 12 };
			}
		}

		public static Configuration Deserialize(string configFile)
		{
			Configuration config;

			try {
				string fileData = File.ReadAllText(configFile);
				config = (Configuration?)JsonSerializer.Deserialize(fileData, typeof(Configuration), MesenSerializerContext.Default) ?? Configuration.CreateConfig();
				config._fileData = fileData;
			} catch {
				try {
					//File exists but couldn't be loaded, make a backup of the old settings before we overwrite them
					BackupSettings(configFile);
				} catch { }

				config = Configuration.CreateConfig();
			}

			return config;
		}

		public static void BackupSettings(string configFile)
		{
			//File exists but couldn't be loaded, make a backup of the old settings before we overwrite them
			string? folder = Path.GetDirectoryName(configFile);
			if(folder != null) {
				File.Copy(configFile, Path.Combine(folder, "settings." + DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss") + ".bak"), true);
			}
		}

		public void Serialize(string configFile)
		{
			try {
				string cfgData = JsonSerializer.Serialize(this, typeof(Configuration), MesenSerializerContext.Default);
				if(_fileData != cfgData && !Design.IsDesignMode) {
					FileHelper.WriteAllText(configFile, cfgData);
					_fileData = cfgData;
				}
			} catch {
				//This can sometime fail due to the file being used by another Mesen instance, etc.
			}
		}

		public void RemoveObsoleteConfig()
		{
			//Clean up configuration to remove any obsolete values that existed in older versions
			for(int i = Preferences.ShortcutKeys.Count - 1; i >= 0; i--) {
				if(Preferences.ShortcutKeys[i].Shortcut >= EmulatorShortcut.LastValidValue) {
					Preferences.ShortcutKeys.RemoveAt(i);
				}
			}
		}
	}

	[Flags]
	public enum DefaultKeyMappingType
	{
		None = 0,
		Xbox = 1,
		Ps4 = 2,
		WasdKeys = 4,
		ArrowKeys = 8
	}

	public enum ConfigUpgradeHint
	{
		Uninitialized = 0,
		FirstRun,
		SmsInput,
		GbaInput,
		CvInput,
		WsInput,
		NextValue,
	}
}
