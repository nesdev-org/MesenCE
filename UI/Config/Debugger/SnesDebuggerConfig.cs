using Mesen.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.Config
{
	public partial class SnesDebuggerConfig : ViewModelBase
	{
		[ObservableProperty] public partial bool BreakOnBrk { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnCop { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnWdm { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnStp { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnInvalidPpuAccess { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnReadDuringAutoJoy { get; set; } = false;

		[ObservableProperty] public partial bool SpcBreakOnBrk { get; set; } = false;
		[ObservableProperty] public partial bool SpcBreakOnStpSleep { get; set; } = false;

		[ObservableProperty] public partial bool UseAltSpcOpNames { get; set; } = false;
		[ObservableProperty] public partial bool IgnoreDspReadWrites { get; set; } = true;
	}
}
