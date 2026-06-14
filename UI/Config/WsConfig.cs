using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config;

public partial class WsConfig : BaseConfig<WsConfig>
{
	[ObservableProperty] public partial ConsoleOverrideConfig ConfigOverrides { get; set; } = new();

	[ObservableProperty] public partial ControllerConfig ControllerHorizontal { get; set; } = new();
	[ObservableProperty] public partial ControllerConfig ControllerVertical { get; set; } = new();
	[ObservableProperty] public partial ControllerConfig ControllerPcv2 { get; set; } = new();

	[ObservableProperty] public partial WsModel Model { get; set; } = WsModel.Auto;
	[ObservableProperty] public partial bool UseBootRom { get; set; } = false;

	[ObservableProperty] public partial bool AutoRotate { get; set; } = true;

	[ObservableProperty] public partial bool BlendFrames { get; set; } = true;
	[ObservableProperty] public partial bool LcdAdjustColors { get; set; } = true;
	[ObservableProperty] public partial bool LcdShowIcons { get; set; } = true;

	[ObservableProperty] public partial bool HideBgLayer1 { get; set; } = false;
	[ObservableProperty] public partial bool HideBgLayer2 { get; set; } = false;
	[ObservableProperty] public partial bool DisableSprites { get; set; } = false;

	[ObservableProperty] public partial WsAudioMode AudioMode { get; set; } = WsAudioMode.Headphones;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel1Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel2Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel3Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel4Vol { get; set; } = 100;
	[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel5Vol { get; set; } = 100;

	public void ApplyConfig()
	{
		ControllerHorizontal.Type = ControllerType.WsController;
		ControllerVertical.Type = ControllerType.WsControllerVertical;
		ControllerPcv2.Type = ControllerType.Pcv2Controller;

		ConfigManager.Config.Video.ApplyConfig();

		ConfigApi.SetWsConfig(new InteropWsConfig() {
			ControllerHorizontal = ControllerHorizontal.ToInterop(),
			ControllerVertical = ControllerVertical.ToInterop(),
			ControllerPcv2 = ControllerPcv2.ToInterop(),

			Model = Model,
			UseBootRom = UseBootRom,

			AutoRotate = AutoRotate,

			BlendFrames = BlendFrames,
			LcdAdjustColors = LcdAdjustColors,
			LcdShowIcons = LcdShowIcons,

			HideBgLayer1 = HideBgLayer1,
			HideBgLayer2 = HideBgLayer2,
			DisableSprites = DisableSprites,

			AudioMode = AudioMode,
			Channel1Vol = Channel1Vol,
			Channel2Vol = Channel2Vol,
			Channel3Vol = Channel3Vol,
			Channel4Vol = Channel4Vol,
			Channel5Vol = Channel5Vol,
		});
	}

	internal void InitializeDefaults(DefaultKeyMappingType defaultMappings)
	{
		ControllerHorizontal.InitDefaults(defaultMappings, ControllerType.WsController);
		ControllerVertical.InitDefaults(defaultMappings, ControllerType.WsControllerVertical);
		ControllerPcv2.InitDefaults(defaultMappings, ControllerType.Pcv2Controller);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct InteropWsConfig
{
	public InteropControllerConfig ControllerHorizontal;
	public InteropControllerConfig ControllerVertical;
	public InteropControllerConfig ControllerPcv2;

	public WsModel Model;
	[MarshalAs(UnmanagedType.I1)] public bool UseBootRom;

	[MarshalAs(UnmanagedType.I1)] public bool AutoRotate;

	[MarshalAs(UnmanagedType.I1)] public bool BlendFrames;
	[MarshalAs(UnmanagedType.I1)] public bool LcdAdjustColors;
	[MarshalAs(UnmanagedType.I1)] public bool LcdShowIcons;

	[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer1;
	[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer2;
	[MarshalAs(UnmanagedType.I1)] public bool DisableSprites;

	public WsAudioMode AudioMode;
	public UInt32 Channel1Vol;
	public UInt32 Channel2Vol;
	public UInt32 Channel3Vol;
	public UInt32 Channel4Vol;
	public UInt32 Channel5Vol;
}

public enum WsModel : byte
{
	Auto,
	Monochrome,
	Color,
	SwanCrystal,
	PocketChallenge
}

public enum WsAudioMode : byte
{
	Headphones,
	Speaker
}
