using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger;
using Mesen.Interop;
using Mesen.ViewModels;

namespace Mesen.Config
{
	public partial class WsDebuggerConfig : ViewModelBase
	{
		[ObservableProperty] public partial bool BreakOnUndefinedOpCode { get; set; } = false;
	}
}
