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
	public partial class GameboyConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial GameboyConfig Config { get; set; }
		[ObservableProperty] public partial GameboyConfig OriginalConfig { get; set; }
		[ObservableProperty] public partial GameboyConfigTab SelectedTab { get; set; } = 0;

		public RelayCommand<Button> SetupPlayer { get; }
		public RelayCommand<Button> SetupPlayer2 { get; }

		public GameboyConfigViewModel()
		{
			Config = ConfigManager.Config.Gameboy;
			OriginalConfig = Config.Clone();

			SetupPlayer = new RelayCommand<Button>(btn => this.OpenSetup(btn!, 0));
			SetupPlayer2 = new RelayCommand<Button>(btn => this.OpenSetup(btn!, 1));

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}

		private async void OpenSetup(Button btn, int port)
		{
			PixelPoint startPosition = btn.PointToScreen(new Point(-7, btn.Bounds.Height));
			ControllerConfigWindow wnd = new ControllerConfigWindow();
			ControllerConfig originalCfg = (port == 0) ? Config.Controller : Config.LinkedController;
			ControllerConfig cfg = originalCfg.Clone();
			wnd.DataContext = new ControllerConfigViewModel(ControllerType.GameboyController, cfg, originalCfg, port);

			if(await wnd.ShowDialogAtPosition<bool>(btn.GetWindow(), startPosition)) {
				if(port == 0) {
					Config.Controller = cfg;
				} else {
					Config.LinkedController = cfg;
				}
			}
		}
	}

	public enum GameboyConfigTab
	{
		General,
		Audio,
		Emulation,
		Input,
		Video
	}
}
