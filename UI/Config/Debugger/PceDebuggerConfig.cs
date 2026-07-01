using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger;
using Mesen.Interop;
using Mesen.ViewModels;

namespace Mesen.Config
{
	public partial class PceDebuggerConfig : ViewModelBase
	{
		[ObservableProperty] public partial bool BreakOnBrk { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnUnofficialOpCode { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnInvalidVramAddress { get; set; } = false;
	}
}
