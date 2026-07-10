using Mesen.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.Config;

public partial class GbaDebuggerConfig : ViewModelBase
{
	[ObservableProperty] public partial bool BreakOnInvalidOpCode { get; set; } = false;
	[ObservableProperty] public partial bool BreakOnNopLoad { get; set; } = false;
	[ObservableProperty] public partial bool BreakOnUnalignedMemAccess { get; set; } = false;

	[ObservableProperty] public partial GbaDisassemblyMode DisassemblyMode { get; set; } = GbaDisassemblyMode.Default;
}

public enum GbaDisassemblyMode : byte
{
	Default,
	Arm,
	Thumb
}
