using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config
{
	public partial class GbaConfig : BaseConfig<GbaConfig>
	{
		[ObservableProperty] public partial ConsoleOverrideConfig ConfigOverrides { get; set; } = new();

		[ObservableProperty] public partial ControllerConfig Controller { get; set; } = new();

		[ObservableProperty] public partial bool SkipBootScreen { get; set; } = false;
		[ObservableProperty] public partial bool DisableFrameSkipping { get; set; } = false;
		[ObservableProperty] public partial bool BlendFrames { get; set; } = true;
		[ObservableProperty] public partial bool GbaAdjustColors { get; set; } = true;

		[ObservableProperty] public partial bool HideBgLayer1 { get; set; } = false;
		[ObservableProperty] public partial bool HideBgLayer2 { get; set; } = false;
		[ObservableProperty] public partial bool HideBgLayer3 { get; set; } = false;
		[ObservableProperty] public partial bool HideBgLayer4 { get; set; } = false;
		[ObservableProperty] public partial bool DisableSprites { get; set; } = false;

		[ObservableProperty][MinMax(0, 1000)] public partial UInt32 OverclockScanlineCount { get; set; } = 0;
		[ObservableProperty] public partial RamState RamPowerOnState { get; set; } = RamState.AllZeros;
		[ObservableProperty] public partial GbaSaveType SaveType { get; set; } = GbaSaveType.AutoDetect;
		[ObservableProperty] public partial GbaRtcType RtcType { get; set; } = GbaRtcType.AutoDetect;
		[ObservableProperty] public partial bool AllowInvalidInput { get; set; } = false;
		[ObservableProperty] public partial bool EnableMgbaLogApi { get; set; } = false;

		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Square1Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Square2Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 NoiseVol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 WaveVol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 ChannelAVol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 ChannelBVol { get; set; } = 100;

		public void ApplyConfig()
		{
			ConfigManager.Config.Video.ApplyConfig();

			ConfigApi.SetGbaConfig(new InteropGbaConfig() {
				Controller = Controller.ToInterop(),

				SkipBootScreen = SkipBootScreen,
				DisableFrameSkipping = DisableFrameSkipping,
				BlendFrames = BlendFrames,
				GbaAdjustColors = GbaAdjustColors,
				HideBgLayer1 = HideBgLayer1,
				HideBgLayer2 = HideBgLayer2,
				HideBgLayer3 = HideBgLayer3,
				HideBgLayer4 = HideBgLayer4,
				DisableSprites = DisableSprites,

				OverclockScanlineCount = OverclockScanlineCount,
				RamPowerOnState = RamPowerOnState,
				SaveType = SaveType,
				RtcType = RtcType,
				AllowInvalidInput = AllowInvalidInput,
				EnableMgbaLogApi = EnableMgbaLogApi,

				ChannelAVol = ChannelAVol,
				ChannelBVol = ChannelBVol,
				Square1Vol = Square1Vol,
				Square2Vol = Square2Vol,
				NoiseVol = NoiseVol,
				WaveVol = WaveVol
			});
		}

		internal void InitializeDefaults(DefaultKeyMappingType defaultMappings)
		{
			Controller.InitDefaults(defaultMappings, ControllerType.GbaController);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InteropGbaConfig
	{
		public InteropControllerConfig Controller;

		[MarshalAs(UnmanagedType.I1)] public bool SkipBootScreen;
		[MarshalAs(UnmanagedType.I1)] public bool DisableFrameSkipping;
		[MarshalAs(UnmanagedType.I1)] public bool BlendFrames;
		[MarshalAs(UnmanagedType.I1)] public bool GbaAdjustColors;

		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer1;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer2;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer3;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer4;
		[MarshalAs(UnmanagedType.I1)] public bool DisableSprites;

		public UInt32 OverclockScanlineCount;
		public RamState RamPowerOnState;
		public GbaSaveType SaveType;
		public GbaRtcType RtcType;
		[MarshalAs(UnmanagedType.I1)] public bool AllowInvalidInput;
		[MarshalAs(UnmanagedType.I1)] public bool EnableMgbaLogApi;

		public UInt32 ChannelAVol;
		public UInt32 ChannelBVol;
		public UInt32 Square1Vol;
		public UInt32 Square2Vol;
		public UInt32 NoiseVol;
		public UInt32 WaveVol;
	}

	public enum GbaSaveType
	{
		AutoDetect = 0,
		None = 1,
		Sram = 2,
		//EepromUnknown = 3, //Hidden in UI
		Eeprom512 = 4,
		Eeprom8192 = 5,
		Flash64 = 6,
		Flash128 = 7
	}

	public enum GbaRtcType
	{
		AutoDetect = 0,
		Enabled = 1,
		Disabled = 2,
	}
}
