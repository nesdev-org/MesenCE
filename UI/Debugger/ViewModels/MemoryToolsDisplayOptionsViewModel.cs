using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Debugger.Utilities;
using Mesen.Interop;
using Mesen.Localization;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Debugger.ViewModels
{
	public partial class MemoryToolsDisplayOptionsViewModel : DisposableViewModel
	{
		public HexEditorConfig Config { get; }
		public MemoryToolsViewModel MemoryTools { get; }

		public int[] AvailableWidths => new int[] { 4, 8, 16, 32, 48, 64, 80, 96, 112, 128 };

		[ObservableProperty] public partial bool ShowFrozenAddressesOption { get; set; }
		[ObservableProperty] public partial bool ShowNesPcmDataOption { get; set; }
		[ObservableProperty] public partial bool ShowNesDrawnChrRomOption { get; set; }

		[Obsolete("For designer only")]
		public MemoryToolsDisplayOptionsViewModel() : this(new()) { }

		public MemoryToolsDisplayOptionsViewModel(MemoryToolsViewModel memoryTools)
		{
			Config = memoryTools.Config;
			MemoryTools = memoryTools;

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(Config.ObserveProp(nameof(Config.MemoryType), () => UpdateAvailableOptions()));
		}

		public void UpdateAvailableOptions()
		{
			ShowFrozenAddressesOption = Config.MemoryType.SupportsFreezeAddress();
			ShowNesPcmDataOption = Config.MemoryType.ToCpuType() == CpuType.Nes;
			ShowNesDrawnChrRomOption = Config.MemoryType.ToCpuType() == CpuType.Nes && DebugApi.GetMemorySize(MemoryType.NesChrRom) > 0;
		}
	}
}
