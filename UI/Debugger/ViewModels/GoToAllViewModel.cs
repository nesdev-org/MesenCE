using Avalonia.Controls;
using Avalonia.Controls.Selection;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger.Integration;
using Mesen.Debugger.Utilities;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mesen.Debugger.ViewModels;

public partial class GoToAllViewModel : DisposableViewModel
{
	[ObservableProperty] public partial string SearchString { get; set; } = "";
	[ObservableProperty] public partial List<SearchResultInfo> SearchResults { get; set; } = new();
	[ObservableProperty] public partial SearchResultInfo? SelectedItem { get; set; } = null;
	[ObservableProperty] public partial bool CanSelect { get; set; } = false;

	public SelectionModel<SearchResultInfo?> SelectionModel { get; private set; } = new();

	[Obsolete("For designer only")]
	public GoToAllViewModel() : this(CpuType.Snes, GoToAllOptions.None) { }

	public GoToAllViewModel(CpuType cpuType, GoToAllOptions options, ISymbolProvider? symbolProvider = null)
	{
		AddDisposable(this.ObserveProp(nameof(SearchString), () => {
			SearchResults = SearchHelper.GetGoToAllResults(cpuType, SearchString, options, symbolProvider);
			if(SearchResults.Count > 0) {
				SelectionModel.SelectedIndex = 0;
			}
		}));

		AddDisposable(SelectionModel.ObserveProp(nameof(SelectionModel.SelectedItem), () => {
			CanSelect = SelectionModel.SelectedItem?.Disabled == false;
		}));
	}
}
