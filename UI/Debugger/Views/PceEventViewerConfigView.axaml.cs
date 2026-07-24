using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Mesen.Controls;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;

namespace Mesen.Debugger.Views
{
	public class PceEventViewerConfigView : UserControl
	{
		private IDisposable? _observer = null;

		public static readonly StyledProperty<bool> IsSuperGrafxProperty = AvaloniaProperty.Register<PceEventViewerConfigView, bool>(nameof(IsSuperGrafx), true);

		public bool IsSuperGrafx
		{
			get { return GetValue(IsSuperGrafxProperty); }
			set { SetValue(IsSuperGrafxProperty, value); }
		}

		public PceEventViewerConfigView()
		{
			InitializeComponent();

			if(Design.IsDesignMode) {
				return;
			}

			string romPath = "";
			_observer = MainWindowViewModel.Instance.ObserveProp(nameof(RomInfo), () => {
				if(romPath != MainWindowViewModel.Instance.RomInfo.RomPath) {
					romPath = MainWindowViewModel.Instance.RomInfo.RomPath;
					IsSuperGrafx = DebugApi.GetConsoleState<PceState>(ConsoleType.PcEngine).IsSuperGrafx;
				}
			});
		}

		protected override void OnUnloaded(RoutedEventArgs e)
		{
			base.OnUnloaded(e);
			_observer?.Dispose();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
