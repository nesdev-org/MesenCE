using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config;

public partial class SmsConfig : BaseConfig<SmsConfig>
{
	[ObservableProperty] public partial ConsoleOverrideConfig ConfigOverrides { get; set; } = new();
	[ObservableProperty] public partial ConsoleOverrideConfig GgConfigOverrides { get; set; } = new();

	[ObservableProperty] public partial SmsControllerConfig Port1 { get; set; } = new();
	[ObservableProperty] public partial SmsControllerConfig Port2 { get; set; } = new();

	[ObservableProperty] public partial bool AllowInvalidInput { get; set; } = false;

	[ValidValues(ConsoleRegion.Auto, ConsoleRegion.Ntsc, ConsoleRegion.Pal)]
	[ObservableProperty] public partial ConsoleRegion Region { get; set; } = ConsoleRegion.Auto;

	[ValidValues(ConsoleRegion.Auto, ConsoleRegion.Ntsc, ConsoleRegion.NtscJapan, ConsoleRegion.Pal)]
	[ObservableProperty] public partial ConsoleRegion GameGearRegion { get; set; } = ConsoleRegion.Auto;

	[ObservableProperty] public partial RamState RamPowerOnState { get; set; } = RamState.Random;

	[ObservableProperty] public partial SmsRevision Revision { get; set; } = SmsRevision.Compatibility;

	[ObservableProperty] public partial bool UseSgPalette { get; set; } = true;
	[ObservableProperty] public partial bool GgBlendFrames { get; set; } = true;
	[ObservableProperty] public partial bool RemoveSpriteLimit { get; set; } = false;
	[ObservableProperty] public partial bool DisableSprites { get; set; } = false;
	[ObservableProperty] public partial bool DisableBackground { get; set; } = false;

	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Tone1Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Tone2Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Tone3Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 NoiseVol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 FmAudioVolume { get; set; } = 100;
	[ObservableProperty] public partial bool EnableFmAudio { get; set; } = true;

	[ObservableProperty] public partial OverscanConfig NtscOverscan { get; set; } = new() { Top = 24, Bottom = 24 };
	[ObservableProperty] public partial OverscanConfig PalOverscan { get; set; } = new() { Top = 24, Bottom = 24 };
	[ObservableProperty] public partial OverscanConfig GameGearOverscan { get; set; } = new() { Top = 48, Bottom = 48, Left = 48, Right = 48 };

	public void ApplyConfig()
	{
		ConfigManager.Config.Video.ApplyConfig();

		ConfigApi.SetSmsConfig(new InteropSmsConfig() {
			Port1 = Port1.ToInterop(),
			Port2 = Port2.ToInterop(),

			Region = Region,
			GameGearRegion = GameGearRegion,
			RamPowerOnState = RamPowerOnState,
			Revision = Revision,

			AllowInvalidInput = this.AllowInvalidInput,
			UseSgPalette = UseSgPalette,
			GgBlendFrames = GgBlendFrames,
			RemoveSpriteLimit = RemoveSpriteLimit,
			DisableBackground = DisableBackground,
			DisableSprites = DisableSprites,

			Tone1Vol = Tone1Vol,
			Tone2Vol = Tone2Vol,
			Tone3Vol = Tone3Vol,
			NoiseVol = NoiseVol,
			FmAudioVolume = FmAudioVolume,
			EnableFmAudio = EnableFmAudio,

			NtscOverscan = NtscOverscan.ToInterop(),
			PalOverscan = PalOverscan.ToInterop(),
			GameGearOverscan = GameGearOverscan.ToInterop(),
		});
	}

	internal void InitializeDefaults(DefaultKeyMappingType defaultMappings)
	{
		Port1.InitDefaults<SmsKeyMapping>(defaultMappings, ControllerType.SmsController);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct InteropSmsConfig
{
	public InteropControllerConfig Port1;
	public InteropControllerConfig Port2;

	public ConsoleRegion Region;
	public ConsoleRegion GameGearRegion;
	public RamState RamPowerOnState;
	public SmsRevision Revision;

	[MarshalAs(UnmanagedType.I1)] public bool AllowInvalidInput;
	[MarshalAs(UnmanagedType.I1)] public bool UseSgPalette;
	[MarshalAs(UnmanagedType.I1)] public bool GgBlendFrames;
	[MarshalAs(UnmanagedType.I1)] public bool RemoveSpriteLimit;
	[MarshalAs(UnmanagedType.I1)] public bool DisableSprites;
	[MarshalAs(UnmanagedType.I1)] public bool DisableBackground;

	public UInt32 Tone1Vol;
	public UInt32 Tone2Vol;
	public UInt32 Tone3Vol;
	public UInt32 NoiseVol;
	public UInt32 FmAudioVolume;
	[MarshalAs(UnmanagedType.I1)] public bool EnableFmAudio;

	public InteropOverscanDimensions NtscOverscan;
	public InteropOverscanDimensions PalOverscan;
	public InteropOverscanDimensions GameGearOverscan;
}

public enum SmsRevision
{
	Compatibility,
	Sms1,
	Sms2
}
