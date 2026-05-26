using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config.Shortcuts
{
	public partial class ShortcutKeyInfo : ObservableObject
	{
		[ObservableProperty] public partial EmulatorShortcut Shortcut { get; set; }
		[ObservableProperty] public partial KeyCombination KeyCombination { get; set; } = new KeyCombination();
		[ObservableProperty] public partial KeyCombination KeyCombination2 { get; set; } = new KeyCombination();
	}
}
