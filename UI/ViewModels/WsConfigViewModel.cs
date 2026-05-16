using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Mesen.Config;
using Mesen.Utilities;
using Mesen.Windows;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Mesen.ViewModels
{
	public class WsConfigViewModel : DisposableViewModel
	{
		[Reactive] public WsConfig Config { get; set; }
		[Reactive] public WsConfig OriginalConfig { get; set; }
		[Reactive] public WsConfigTab SelectedTab { get; set; } = 0;

		public ReactiveCommand<Button, Unit> SetupPlayerHorizontal { get; }
		public ReactiveCommand<Button, Unit> SetupPlayerVertical { get; }
		public ReactiveCommand<Button, Unit> SetupPlayerPcv2 { get; }

		public WsConfigViewModel()
		{
			Config = ConfigManager.Config.Ws;
			OriginalConfig = Config.Clone();

			IObservable<bool> button1Enabled = this.WhenAnyValue(x => x.Config.ControllerHorizontal.Type, x => x.CanConfigure());
			IObservable<bool> button2Enabled = this.WhenAnyValue(x => x.Config.ControllerVertical.Type, x => x.CanConfigure());
			IObservable<bool> button3Enabled = this.WhenAnyValue(x => x.Config.ControllerPcv2.Type, x => x.CanConfigure());
			SetupPlayerHorizontal = ReactiveCommand.Create<Button>(btn => this.OpenSetup(btn, ControllerType.WsController), button1Enabled);
			SetupPlayerVertical = ReactiveCommand.Create<Button>(btn => this.OpenSetup(btn, ControllerType.WsControllerVertical), button2Enabled);
			SetupPlayerPcv2 = ReactiveCommand.Create<Button>(btn => this.OpenSetup(btn, ControllerType.Pcv2Controller), button3Enabled);

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}

		private async void OpenSetup(Button btn, ControllerType type)
		{
			PixelPoint startPosition = btn.PointToScreen(new Point(-7, btn.Bounds.Height));
			ControllerConfigWindow wnd = new ControllerConfigWindow();
			ControllerConfig orgCfg = type switch {
				ControllerType.WsController => Config.ControllerHorizontal,
				ControllerType.WsControllerVertical => Config.ControllerVertical,
				ControllerType.Pcv2Controller => Config.ControllerPcv2,
				_ => throw new NotImplementedException()
			};

			ControllerConfig cfg = orgCfg.Clone();

			wnd.DataContext = new ControllerConfigViewModel(type, cfg, orgCfg, 0);

			if(await wnd.ShowDialogAtPosition<bool>(btn.GetVisualRoot() as Visual, startPosition)) {
				switch(type) {
					case ControllerType.WsController: Config.ControllerHorizontal = cfg; break;
					case ControllerType.WsControllerVertical: Config.ControllerVertical = cfg; break;
					case ControllerType.Pcv2Controller: Config.ControllerPcv2 = cfg; break;
				}
			}
		}
	}

	public enum WsConfigTab
	{
		General,
		Audio,
		Emulation,
		Input,
		Video
	}
}
