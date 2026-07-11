using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config
{
	public partial class GameboyConfig : BaseConfig<GameboyConfig>
	{
		[ObservableProperty] public partial ConsoleOverrideConfig ConfigOverrides { get; set; } = new();

		[ObservableProperty] public partial ControllerConfig Controller { get; set; } = new();
		[ObservableProperty] public partial ControllerConfig LinkedController { get; set; } = new();

		[ObservableProperty] public partial GameboyModel Model { get; set; } = GameboyModel.AutoFavorBest;
		[ObservableProperty] public partial bool UseSgb2 { get; set; } = true;

		[ObservableProperty] public partial bool UseLocalLinkCable { get; set; } = false;
		[ObservableProperty] public partial GbLocalLinkOutputOption LocalLinkCableVideoOutput { get; set; } = GbLocalLinkOutputOption.Both;
		[ObservableProperty] public partial GbLocalLinkOutputOption LocalLinkCableAudioOutput { get; set; } = GbLocalLinkOutputOption.Both;

		[ObservableProperty] public partial bool BlendFrames { get; set; } = true;
		[ObservableProperty] public partial bool GbcAdjustColors { get; set; } = true;

		[ObservableProperty] public partial bool DisableBackground { get; set; } = false;
		[ObservableProperty] public partial bool DisableSprites { get; set; } = false;
		[ObservableProperty] public partial bool RemoveSpriteLimit { get; set; } = false;
		[ObservableProperty] public partial bool HideSgbBorders { get; set; } = false;

		[ObservableProperty] public partial RamState RamPowerOnState { get; set; } = RamState.Random;
		[ObservableProperty] public partial bool AllowInvalidInput { get; set; } = false;

		[ObservableProperty][MinMax(0, 1000)] public partial UInt32 OverclockScanlineCount { get; set; } = 0;

		[ObservableProperty] public partial UInt32[] BgColors { get; set; } = new UInt32[] { 0xFFFFFFFF, 0xFFB0B0B0, 0xFF686868, 0xFF000000 };
		[ObservableProperty] public partial UInt32[] Obj0Colors { get; set; } = new UInt32[] { 0xFFFFFFFF, 0xFFB0B0B0, 0xFF686868, 0xFF000000 };
		[ObservableProperty] public partial UInt32[] Obj1Colors { get; set; } = new UInt32[] { 0xFFFFFFFF, 0xFFB0B0B0, 0xFF686868, 0xFF000000 };

		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Square1Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Square2Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 NoiseVol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 WaveVol { get; set; } = 100;

		public void ApplyConfig()
		{
			ConfigManager.Config.Video.ApplyConfig();

			ConfigApi.SetGameboyConfig(new InteropGameboyConfig() {
				Controller = Controller.ToInterop(),
				LinkedController = LinkedController.ToInterop(),
				Model = Model,
				UseSgb2 = UseSgb2,

				UseLocalLinkCable = UseLocalLinkCable,
				LocalLinkCableVideoOutput = LocalLinkCableVideoOutput,
				LocalLinkCableAudioOutput = LocalLinkCableAudioOutput,

				BlendFrames = BlendFrames,
				GbcAdjustColors = GbcAdjustColors,
				DisableBackground = DisableBackground,
				DisableSprites = DisableSprites,
				RemoveSpriteLimit = RemoveSpriteLimit,
				HideSgbBorders = HideSgbBorders,

				RamPowerOnState = RamPowerOnState,
				AllowInvalidInput = AllowInvalidInput,

				OverclockScanlineCount = OverclockScanlineCount,

				BgColors = BgColors,
				Obj0Colors = Obj0Colors,
				Obj1Colors = Obj1Colors,

				Square1Vol = Square1Vol,
				Square2Vol = Square2Vol,
				NoiseVol = NoiseVol,
				WaveVol = WaveVol
			});
		}

		internal void InitializeDefaults(DefaultKeyMappingType defaultMappings)
		{
			Controller.InitDefaults(defaultMappings, ControllerType.GameboyController);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InteropGameboyConfig
	{
		public InteropControllerConfig Controller;
		public InteropControllerConfig LinkedController;

		public GameboyModel Model;
		[MarshalAs(UnmanagedType.I1)] public bool UseSgb2;

		[MarshalAs(UnmanagedType.I1)] public bool UseLocalLinkCable;
		public GbLocalLinkOutputOption LocalLinkCableVideoOutput;
		public GbLocalLinkOutputOption LocalLinkCableAudioOutput;

		[MarshalAs(UnmanagedType.I1)] public bool BlendFrames;
		[MarshalAs(UnmanagedType.I1)] public bool GbcAdjustColors;

		[MarshalAs(UnmanagedType.I1)] public bool DisableBackground;
		[MarshalAs(UnmanagedType.I1)] public bool DisableSprites;
		[MarshalAs(UnmanagedType.I1)] public bool RemoveSpriteLimit;
		[MarshalAs(UnmanagedType.I1)] public bool HideSgbBorders;

		public RamState RamPowerOnState;
		[MarshalAs(UnmanagedType.I1)] public bool AllowInvalidInput;

		public UInt32 OverclockScanlineCount;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public UInt32[] BgColors;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public UInt32[] Obj0Colors;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public UInt32[] Obj1Colors;

		public UInt32 Square1Vol;
		public UInt32 Square2Vol;
		public UInt32 NoiseVol;
		public UInt32 WaveVol;
	}

	public enum GbLocalLinkOutputOption
	{
		Both = 0,
		MainSystemOnly = 1,
		SubSystemOnly = 2
	}

	public enum GameboyModel
	{
		AutoFavorBest,
		AutoFavorGbc,
		AutoFavorSgb,
		AutoFavorGb,
		Gameboy,
		GameboyColor,
		SuperGameboy
	}
}
