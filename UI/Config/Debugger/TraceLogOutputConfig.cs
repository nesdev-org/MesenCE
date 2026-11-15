using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mesen.Config
{
	public enum TraceLogOutputTarget
	{
		File,
		NetworkSocket,
	};

	public enum TraceLogOutputCompressionType : byte
	{
		None = 0,
		[RestrictToPlatforms(CpuType.Snes, CpuType.Sa1)]
		DiztinguishZlib = 1,
	};

	public enum TraceLogOutputFormat : byte
	{
		Text = 0,
		[RestrictToPlatforms(CpuType.Snes, CpuType.Sa1)]
		Diztinguish = 1,
		[RestrictToPlatforms(CpuType.Snes, CpuType.Sa1)]
		DiztinguishAbridged = 2,
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	public partial class TraceLogOutputCpuDependentConfig : BaseConfig<TraceLogOutputCpuDependentConfig>
	{
		[ObservableProperty] public partial TraceLogOutputCompressionType CompressionType { get; set; } = TraceLogOutputCompressionType.None;
		[ObservableProperty] public partial TraceLogOutputFormat Format { get; set; } = TraceLogOutputFormat.Text;
	};

	public partial class TraceLogOutputSharedConfig : BaseConfig<TraceLogOutputSharedConfig>
	{
		[ObservableProperty] public partial uint ChunkSize { get; set; } = 1 * 1024 * 1024;
		[ObservableProperty] public partial uint ChunkCount { get; set; } = 10;
		[ObservableProperty] public partial bool BatchLogData { get; set; } = true;

		[RestrictToPlatforms(CpuType.Snes, CpuType.Sa1)]
		[ObservableProperty] public partial bool UniqueRowsOnly { get; set; } = false;
		[ObservableProperty] public partial bool IncludeImplicitPC { get; set; } = true;

		// I copied this from TraceLoggerConfig.cs.
		// Is that really the best way to do things?
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig SnesConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig SpcConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig NecDspConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig Sa1Config { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig GsuConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig Cx4Config { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig St018Config { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig GbConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig NesConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig PceConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig SmsConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig GbaConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputCpuDependentConfig WsConfig { get; set; } = new();

		// Feels slightly evil, but is the easiest way we can bind these settings to Avalonia...
		[ObservableProperty] public partial CpuType LastUsedCpuType { get; set; } = CpuType.Snes;
		public TraceLogOutputCpuDependentConfig CpuDependentConfig { get => GetCpuConfig(LastUsedCpuType); }

		public TraceLogOutputCpuDependentConfig GetCpuConfig(CpuType type)
		{
			return type switch {
				CpuType.Snes => SnesConfig,
				CpuType.Spc => SpcConfig,
				CpuType.NecDsp => NecDspConfig,
				CpuType.Sa1 => Sa1Config,
				CpuType.Gsu => GsuConfig,
				CpuType.Cx4 => Cx4Config,
				CpuType.St018 => St018Config,
				CpuType.Gameboy => GbConfig,
				CpuType.Nes => NesConfig,
				CpuType.Pce => PceConfig,
				CpuType.Sms => SmsConfig,
				CpuType.Gba => GbaConfig,
				CpuType.Ws => WsConfig,
				_ => throw new NotImplementedException("Unsupport cpu type")
			};
		}
	};

	public partial class TraceLogOutputFileConfig : BaseConfig<TraceLogOutputFileConfig>
	{
		[ObservableProperty] public partial string FileName { get; set; } = "";
		[ObservableProperty] public partial bool NativeNewLines { get; set; } = true;
	};

	public partial class TraceLogOutputNetworkSocketConfig : BaseConfig<TraceLogOutputNetworkSocketConfig>
	{
		[ObservableProperty] public partial ushort ConnectionPort { get; set; } = 27015;
	};

	public partial class TraceLogOutputConfig : BaseWindowConfig<TraceLogOutputConfig>
	{
		[ObservableProperty] public partial TraceLogOutputTarget OutputTarget { get; set; } = TraceLogOutputTarget.File;
		[ObservableProperty] public partial bool ShowAdvancedSettings { get; set; } = false;
		[ObservableProperty] public partial TraceLogOutputSharedConfig SharedConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputFileConfig FileConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLogOutputNetworkSocketConfig SocketConfig { get; set; } = new();
	}
}
