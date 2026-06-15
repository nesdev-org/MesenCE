using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger.Labels;
using Mesen.Debugger.Utilities;
using Mesen.Debugger.Windows;
using Mesen.Interop;
using Mesen.Localization;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mesen.Debugger.ViewModels
{
	public partial class BreakpointEditViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial Breakpoint Breakpoint { get; set; }

		public Control? HelpTooltip { get; } = null;
		public string WindowTitle { get; } = "";
		[ObservableProperty] public partial bool IsConditionValid { get; private set; }
		[ObservableProperty] public partial bool OkEnabled { get; private set; }
		[ObservableProperty] public partial string MaxAddress { get; private set; } = "";
		[ObservableProperty] public partial bool CanExec { get; private set; } = false;
		[ObservableProperty] public partial bool HasDummyOperations { get; private set; } = false;

		public Enum[] AvailableMemoryTypes { get; private set; } = Array.Empty<Enum>();

		[Obsolete("For designer only")]
		public BreakpointEditViewModel() : this(null!) { }

		public BreakpointEditViewModel(Breakpoint bp)
		{
			Breakpoint = bp;

			if(Design.IsDesignMode) {
				return;
			}

			WindowTitle = ResourceHelper.GetViewLabel(nameof(BreakpointEditWindow), bp.Forbid ? "wndTitleForbid" : "wndTitle");

			HasDummyOperations = bp.CpuType.HasDummyOperations() && !bp.Forbid;
			HelpTooltip = ExpressionTooltipHelper.GetHelpTooltip(bp.CpuType, false);
			AvailableMemoryTypes = Enum.GetValues<MemoryType>().Where(t => {
				if(bp.Forbid && !t.SupportsExecBreakpoints()) {
					return false;
				}
				return bp.CpuType.CanAccessMemoryType(t) && t.SupportsBreakpoints() && DebugApi.GetMemorySize(t) > 0;
			}).Cast<Enum>().ToArray();
			if(!AvailableMemoryTypes.Contains(Breakpoint.MemoryType)) {
				Breakpoint.MemoryType = (MemoryType)AvailableMemoryTypes[0];
			}

			UInt32 prevStart = Breakpoint.StartAddress;
			AddDisposable(Breakpoint.ObserveProp(nameof(Breakpoint.StartAddress), () => {
				if(prevStart == Breakpoint.EndAddress) {
					Breakpoint.EndAddress = Breakpoint.StartAddress;
				}
				prevStart = Breakpoint.StartAddress;
			}));

			AddDisposable(Breakpoint.ObserveProp(nameof(Breakpoint.MemoryType), () => {
				CanExec = Breakpoint.MemoryType.SupportsExecBreakpoints();

				int maxAddress = DebugApi.GetMemorySize(Breakpoint.MemoryType) - 1;
				if(maxAddress <= 0) {
					MaxAddress = "(unavailable)";
				} else {
					MaxAddress = "(Max: $" + maxAddress.ToString("X4") + ")";
				}
			}));

			AddDisposable(Breakpoint.ObserveProp(nameof(Breakpoint.Condition), () => {
				if(!string.IsNullOrWhiteSpace(Breakpoint.Condition)) {
					EvalResultType resultType;
					DebugApi.EvaluateExpression(Breakpoint.Condition.Replace(Environment.NewLine, " "), Breakpoint.CpuType, out resultType, false);
					if(resultType == EvalResultType.Invalid) {
						IsConditionValid = false;
						return;
					}
				}
				IsConditionValid = true;
				UpdateButtonState();
			}));

			AddDisposable(Breakpoint.ObserveProp([
				nameof(Breakpoint.BreakOnExec),
				nameof(Breakpoint.BreakOnRead),
				nameof(Breakpoint.BreakOnWrite),
				nameof(Breakpoint.MemoryType),
				nameof(Breakpoint.StartAddress),
				nameof(Breakpoint.EndAddress)
			], () => {
				UpdateButtonState();
			}));
		}

		private void UpdateButtonState()
		{
			bool enabled = true;
			if(Breakpoint.Type == BreakpointTypeFlags.None || !IsConditionValid) {
				enabled = false;
			} else {
				int maxAddress = DebugApi.GetMemorySize(Breakpoint.MemoryType) - 1;
				if(Breakpoint.StartAddress > maxAddress || Breakpoint.EndAddress > maxAddress || Breakpoint.StartAddress > Breakpoint.EndAddress) {
					enabled = false;
				}
			}
			OkEnabled = enabled;
		}
	}
}
