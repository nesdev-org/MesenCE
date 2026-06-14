using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger;
using Mesen.Interop;
using Mesen.ViewModels;

namespace Mesen.Config
{
	public partial class GbDebuggerConfig : ViewModelBase
	{
		[ObservableProperty] public partial bool GbBreakOnInvalidOamAccess { get; set; } = false;
		[ObservableProperty] public partial bool GbBreakOnInvalidVramAccess { get; set; } = false;
		[ObservableProperty] public partial bool GbBreakOnDisableLcdOutsideVblank { get; set; } = false;
		[ObservableProperty] public partial bool GbBreakOnInvalidOpCode { get; set; } = false;
		[ObservableProperty] public partial bool GbBreakOnNopLoad { get; set; } = false;
		[ObservableProperty] public partial bool GbBreakOnOamCorruption { get; set; } = false;
	}
}
