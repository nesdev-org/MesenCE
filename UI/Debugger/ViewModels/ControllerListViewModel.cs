using Avalonia.Controls;
using Mesen.Interop;
using Mesen.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace Mesen.Debugger.ViewModels
{
	public partial class ControllerListViewModel : ViewModelBase
	{
		[ObservableProperty] public partial List<ControllerInputViewModel> Controllers { get; set; } = new();

		[Obsolete("For designer only")]
		public ControllerListViewModel() : this(ConsoleType.Snes) { }

		public ControllerListViewModel(ConsoleType consoleType)
		{
			if(Design.IsDesignMode) {
				return;
			}

			foreach(int index in DebugApi.GetAvailableInputOverrides()) {
				Controllers.Add(new ControllerInputViewModel(consoleType, index));
			}
		}

		public void SetInputOverrides()
		{
			foreach(ControllerInputViewModel controller in Controllers) {
				controller.SetInputOverrides();
			}
		}
	}
}
