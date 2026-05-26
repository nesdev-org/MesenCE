using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config;

public partial class CvConfig : BaseConfig<CvConfig>
{
	[ObservableProperty] public partial ConsoleOverrideConfig ConfigOverrides { get; set; } = new();

	[ObservableProperty] public partial CvControllerConfig Port1 { get; set; } = new();
	[ObservableProperty] public partial CvControllerConfig Port2 { get; set; } = new();

	[ValidValues(ConsoleRegion.Auto, ConsoleRegion.Ntsc, ConsoleRegion.Pal)]
	[ObservableProperty] public partial ConsoleRegion Region { get; set; } = ConsoleRegion.Auto;

	[ObservableProperty] public partial RamState RamPowerOnState { get; set; } = RamState.AllZeros;

	[ObservableProperty] public partial bool RemoveSpriteLimit { get; set; } = false;
	[ObservableProperty] public partial bool DisableSprites { get; set; } = false;
	[ObservableProperty] public partial bool DisableBackground { get; set; } = false;

	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Tone1Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Tone2Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Tone3Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 NoiseVol { get; set; } = 100;

	public void ApplyConfig()
	{
		ConfigManager.Config.Video.ApplyConfig();

		ConfigApi.SetCvConfig(new InteropCvConfig() {
			Port1 = Port1.ToInterop(),
			Port2 = Port2.ToInterop(),

			Region = Region,
			RamPowerOnState = RamPowerOnState,

			RemoveSpriteLimit = RemoveSpriteLimit,
			DisableBackground = DisableBackground,
			DisableSprites = DisableSprites,

			Tone1Vol = Tone1Vol,
			Tone2Vol = Tone2Vol,
			Tone3Vol = Tone3Vol,
			NoiseVol = NoiseVol,
		});
	}

	internal void InitializeDefaults(DefaultKeyMappingType defaultMappings)
	{
		Port1.InitDefaults<CvKeyMapping>(defaultMappings, ControllerType.ColecoVisionController);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct InteropCvConfig
{
	public InteropControllerConfig Port1;
	public InteropControllerConfig Port2;

	public ConsoleRegion Region;
	public RamState RamPowerOnState;

	[MarshalAs(UnmanagedType.I1)] public bool RemoveSpriteLimit;
	[MarshalAs(UnmanagedType.I1)] public bool DisableSprites;
	[MarshalAs(UnmanagedType.I1)] public bool DisableBackground;

	public UInt32 Tone1Vol;
	public UInt32 Tone2Vol;
	public UInt32 Tone3Vol;
	public UInt32 NoiseVol;
}
