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
	public class SmsDebuggerConfig : ViewModelBase
	{
		[Reactive] public bool BreakOnNopLoad { get; set; } = false;
	}
}
