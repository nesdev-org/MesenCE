using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.ViewModels;

namespace Mesen.Config
{
	public partial class NesDebuggerConfig : ViewModelBase
	{
		[ObservableProperty] public partial bool BreakOnBrk { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnUnofficialOpCode { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnUnstableOpCode { get; set; } = true;
		[ObservableProperty] public partial bool BreakOnCpuCrash { get; set; } = true;

		[ObservableProperty] public partial bool BreakOnBusConflict { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnDecayedOamRead { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnPpuScrollGlitch { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnExtOutputMode { get; set; } = true;
		[ObservableProperty] public partial bool BreakOnInvalidVramAccess { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnInvalidOamWrite { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnDmaInputRead { get; set; } = false;
	}
}
