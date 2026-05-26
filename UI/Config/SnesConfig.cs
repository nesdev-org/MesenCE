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
	public partial class SnesConfig : BaseConfig<SnesConfig>
	{
		[ObservableProperty] public partial ConsoleOverrideConfig ConfigOverrides { get; set; } = new();

		//Input
		[ObservableProperty] public partial SnesControllerConfig Port1 { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port2 { get; set; } = new SnesControllerConfig();

		[ObservableProperty] public partial SnesControllerConfig Port1A { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port1B { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port1C { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port1D { get; set; } = new SnesControllerConfig();

		[ObservableProperty] public partial SnesControllerConfig Port2A { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port2B { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port2C { get; set; } = new SnesControllerConfig();
		[ObservableProperty] public partial SnesControllerConfig Port2D { get; set; } = new SnesControllerConfig();

		[ObservableProperty] public partial bool AllowInvalidInput { get; set; } = false;

		[ValidValues(ConsoleRegion.Auto, ConsoleRegion.Ntsc, ConsoleRegion.Pal)]
		[ObservableProperty] public partial ConsoleRegion Region { get; set; } = ConsoleRegion.Auto;

		//Video
		[ObservableProperty] public partial SnesHighResBlendMode HighResBlendMode { get; set; } = SnesHighResBlendMode.None;
		[ObservableProperty] public partial bool HideBgLayer1 { get; set; } = false;
		[ObservableProperty] public partial bool HideBgLayer2 { get; set; } = false;
		[ObservableProperty] public partial bool HideBgLayer3 { get; set; } = false;
		[ObservableProperty] public partial bool HideBgLayer4 { get; set; } = false;
		[ObservableProperty] public partial bool HideSprites { get; set; } = false;
		[ObservableProperty] public partial bool DisableFrameSkipping { get; set; } = false;
		[ObservableProperty] public partial bool ForceFixedResolution { get; set; } = false;

		[ObservableProperty] public partial OverscanConfig Overscan { get; set; } = new() { Top = 7, Bottom = 8 };

		//Audio
		[ObservableProperty] public partial DspInterpolationType InterpolationType { get; set; } = DspInterpolationType.Gauss;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel1Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel2Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel3Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel4Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel5Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel6Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel7Vol { get; set; } = 100;
		[ObservableProperty][MinMax(0, 100)] public partial UInt32 Channel8Vol { get; set; } = 100;

		//Emulation
		[ObservableProperty] public partial bool EnableRandomPowerOnState { get; set; } = false;
		[ObservableProperty] public partial bool EnableStrictBoardMappings { get; set; } = false;
		[ObservableProperty] public partial RamState RamPowerOnState { get; set; } = RamState.Random;
		[ObservableProperty][MinMax(-999, 999)] public partial Int32 SpcClockSpeedAdjustment { get; set; } = 40;

		//Overclocking
		[ObservableProperty][MinMax(0, 1000)] public partial UInt32 PpuExtraScanlinesBeforeNmi { get; set; } = 0;
		[ObservableProperty][MinMax(0, 1000)] public partial UInt32 PpuExtraScanlinesAfterNmi { get; set; } = 0;
		[ObservableProperty][MinMax(100, 1000)] public partial UInt32 GsuClockSpeed { get; set; } = 100;

		//BSX
		[ObservableProperty] public partial bool BsxUseCustomTime { get; set; } = false;
		[ObservableProperty] public partial DateTimeOffset BsxCustomDate { get; set; } = new DateTimeOffset(1995, 1, 1, 0, 0, 0, TimeSpan.Zero);
		[ObservableProperty] public partial TimeSpan BsxCustomTime { get; set; } = TimeSpan.Zero;

		public void ApplyConfig()
		{
			ConfigManager.Config.Video.ApplyConfig();

			ConfigApi.SetSnesConfig(new InteropSnesConfig() {
				Port1 = Port1.ToInterop(),
				Port1A = Port1.ToInterop(Port1A.Type),
				Port1B = Port1B.ToInterop(),
				Port1C = Port1C.ToInterop(),
				Port1D = Port1D.ToInterop(),

				Port2 = Port2.ToInterop(),
				Port2A = Port2.ToInterop(Port2A.Type),
				Port2B = Port2B.ToInterop(),
				Port2C = Port2C.ToInterop(),
				Port2D = Port2D.ToInterop(),

				Region = this.Region,

				AllowInvalidInput = this.AllowInvalidInput,

				HighResBlendMode = this.HighResBlendMode,
				HideBgLayer1 = this.HideBgLayer1,
				HideBgLayer2 = this.HideBgLayer2,
				HideBgLayer3 = this.HideBgLayer3,
				HideBgLayer4 = this.HideBgLayer4,
				HideSprites = this.HideSprites,

				DisableFrameSkipping = DisableFrameSkipping,
				ForceFixedResolution = ForceFixedResolution,

				Overscan = Overscan.ToInterop(),

				InterpolationType = InterpolationType,

				Channel1Vol = Channel1Vol,
				Channel2Vol = Channel2Vol,
				Channel3Vol = Channel3Vol,
				Channel4Vol = Channel4Vol,
				Channel5Vol = Channel5Vol,
				Channel6Vol = Channel6Vol,
				Channel7Vol = Channel7Vol,
				Channel8Vol = Channel8Vol,

				EnableRandomPowerOnState = this.EnableRandomPowerOnState,
				EnableStrictBoardMappings = this.EnableStrictBoardMappings,
				PpuExtraScanlinesBeforeNmi = this.PpuExtraScanlinesBeforeNmi,
				PpuExtraScanlinesAfterNmi = this.PpuExtraScanlinesAfterNmi,
				GsuClockSpeed = this.GsuClockSpeed,
				RamPowerOnState = this.RamPowerOnState,
				SpcClockSpeedAdjustment = this.SpcClockSpeedAdjustment,
				BsxCustomDate = BsxUseCustomTime ? (this.BsxCustomDate.ToUnixTimeSeconds() + (long)this.BsxCustomTime.TotalSeconds) : -1
			});
		}

		public void InitializeDefaults(DefaultKeyMappingType defaultMappings)
		{
			Port1.InitDefaults<SnesKeyMapping>(defaultMappings, ControllerType.SnesController);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InteropSnesConfig
	{
		public InteropControllerConfig Port1;
		public InteropControllerConfig Port2;

		public InteropControllerConfig Port1A;
		public InteropControllerConfig Port1B;
		public InteropControllerConfig Port1C;
		public InteropControllerConfig Port1D;

		public InteropControllerConfig Port2A;
		public InteropControllerConfig Port2B;
		public InteropControllerConfig Port2C;
		public InteropControllerConfig Port2D;

		public ConsoleRegion Region;

		[MarshalAs(UnmanagedType.I1)] public bool AllowInvalidInput;

		public SnesHighResBlendMode HighResBlendMode;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer1;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer2;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer3;
		[MarshalAs(UnmanagedType.I1)] public bool HideBgLayer4;
		[MarshalAs(UnmanagedType.I1)] public bool HideSprites;
		[MarshalAs(UnmanagedType.I1)] public bool DisableFrameSkipping;
		[MarshalAs(UnmanagedType.I1)] public bool ForceFixedResolution;

		public InteropOverscanDimensions Overscan;

		public DspInterpolationType InterpolationType;
		public UInt32 Channel1Vol;
		public UInt32 Channel2Vol;
		public UInt32 Channel3Vol;
		public UInt32 Channel4Vol;
		public UInt32 Channel5Vol;
		public UInt32 Channel6Vol;
		public UInt32 Channel7Vol;
		public UInt32 Channel8Vol;

		[MarshalAs(UnmanagedType.I1)] public bool EnableRandomPowerOnState;
		[MarshalAs(UnmanagedType.I1)] public bool EnableStrictBoardMappings;
		public RamState RamPowerOnState;
		public Int32 SpcClockSpeedAdjustment;

		public UInt32 PpuExtraScanlinesBeforeNmi;
		public UInt32 PpuExtraScanlinesAfterNmi;
		public UInt32 GsuClockSpeed;

		public long BsxCustomDate;
	}

	public enum DspInterpolationType
	{
		Gauss,
		Cubic,
		Sinc,
		None
	}

	public enum SnesHighResBlendMode
	{
		None,
		BlendAll,
		BlendEvenOdd
	}
}
