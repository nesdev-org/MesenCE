using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger;
using Mesen.Interop;
using Mesen.ViewModels;

namespace Mesen.Config
{
	public partial class SmsDebuggerConfig : ViewModelBase
	{
		[ObservableProperty] public partial bool BreakOnNopLoad { get; set; } = false;
	}
}
