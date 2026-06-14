using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.Config
{
	public partial class TraceLoggerCpuConfig : BaseConfig<TraceLoggerCpuConfig>
	{
		[ObservableProperty] public partial bool Enabled { get; set; } = true;

		[ObservableProperty] public partial bool ShowRegisters { get; set; } = true;
		[ObservableProperty] public partial bool ShowStatusFlags { get; set; } = true;
		[ObservableProperty] public partial StatusFlagFormat StatusFormat { get; set; } = StatusFlagFormat.Text;

		[ObservableProperty] public partial bool ShowEffectiveAddresses { get; set; } = true;
		[ObservableProperty] public partial bool ShowMemoryValues { get; set; } = true;
		[ObservableProperty] public partial bool ShowByteCode { get; set; } = false;

		[ObservableProperty] public partial bool ShowClockCounter { get; set; } = false;
		[ObservableProperty] public partial bool ShowFrameCounter { get; set; } = false;
		[ObservableProperty] public partial bool ShowFramePosition { get; set; } = true;

		[ObservableProperty] public partial bool UseLabels { get; set; } = true;
		[ObservableProperty] public partial bool IndentCode { get; set; } = false;

		[ObservableProperty] public partial bool UseCustomFormat { get; set; } = false;
		[ObservableProperty] public partial string Format { get; set; } = "";
		[ObservableProperty] public partial string Condition { get; set; } = "";
	}

	public enum StatusFlagFormat
	{
		Hexadecimal = 0,
		Text = 1,
		CompactText = 2
	}
}
