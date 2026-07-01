using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mesen.Config;
using Mesen.Utilities;
using Mesen.Windows;
using System;

namespace Mesen.ViewModels
{
	public partial class GbaConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial GbaConfig Config { get; set; }
		[ObservableProperty] public partial GbaConfig OriginalConfig { get; set; }
		[ObservableProperty] public partial GbaConfigTab SelectedTab { get; set; } = 0;

		public IRelayCommand SetupPlayer { get; }

		public GbaConfigViewModel()
		{
			Config = ConfigManager.Config.Gba;
			OriginalConfig = Config.Clone();

			SetupPlayer = new RelayCommand<Button>(btn => this.OpenSetup(btn!, 0));

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}

		private async void OpenSetup(Button btn, int port)
		{
			PixelPoint startPosition = btn.PointToScreen(new Point(-7, btn.Bounds.Height));
			ControllerConfigWindow wnd = new ControllerConfigWindow();
			ControllerConfig cfg = Config.Controller.Clone();
			wnd.DataContext = new ControllerConfigViewModel(ControllerType.GbaController, cfg, Config.Controller, 0);

			if(await wnd.ShowDialogAtPosition<bool>(btn.GetWindow(), startPosition)) {
				Config.Controller = cfg;
			}
		}
	}

	public enum GbaConfigTab
	{
		General,
		Audio,
		Emulation,
		Input,
		Video
	}
}
