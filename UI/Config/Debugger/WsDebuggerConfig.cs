using Avalonia;
using Avalonia.Media;
using Mesen.Debugger;
using Mesen.Interop;
using Mesen.ViewModels;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Linq;

namespace Mesen.Config
{
	public class WsDebuggerConfig : ViewModelBase
	{
		[Reactive] public bool BreakOnUndefinedOpCode { get; set; } = false;
	}
}
