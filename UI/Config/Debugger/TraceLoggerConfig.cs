using Avalonia;
using Avalonia.Media;
using Mesen.Debugger;
using Mesen.Interop;
using Mesen.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Mesen.Config
{
	public partial class TraceLoggerConfig : BaseWindowConfig<TraceLoggerConfig>
	{
		[ObservableProperty] public partial bool AutoRefresh { get; set; } = true;
		[ObservableProperty] public partial bool RefreshOnBreakPause { get; set; } = true;
		[ObservableProperty] public partial bool ShowToolbar { get; set; } = true;

		[ObservableProperty] public partial TraceLoggerCpuConfig SnesConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig SpcConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig NecDspConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig Sa1Config { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig GsuConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig Cx4Config { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig St018Config { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig GbConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig NesConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig PceConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig SmsConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig GbaConfig { get; set; } = new();
		[ObservableProperty] public partial TraceLoggerCpuConfig WsConfig { get; set; } = new();

		public TraceLoggerConfig()
		{
		}

		public TraceLoggerCpuConfig GetCpuConfig(CpuType type)
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
	}
}
