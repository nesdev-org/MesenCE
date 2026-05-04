using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DataBoxControl;
using Mesen.Config;
using Mesen.Debugger;
using Mesen.Debugger.Utilities;
using Mesen.Debugger.ViewModels;
using Mesen.Debugger.Windows;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Linq;

namespace Mesen.Debugger.Views
{
	public class FindResultListView : UserControl
	{
		public FindResultListView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			if(DataContext is FindResultListViewModel vm) {
				vm.InitContextMenu(this);
			}
			base.OnDataContextChanged(e);
		}

		private void OnCellDoubleClick(DataBoxCell cell)
		{
			if(DataContext is FindResultListViewModel listModel && cell.DataContext is FindResultViewModel result) {
				listModel.GoToResult(result);
			}
		}
	}
}
