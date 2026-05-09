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
			SetupPlayerHorizontal = ReactiveCommand.Create<Button>(btn => this.OpenSetup(btn, 0), button1Enabled);
			SetupPlayerVertical = ReactiveCommand.Create<Button>(btn => this.OpenSetup(btn, 1), button2Enabled);
			SetupPlayerPcv2 = ReactiveCommand.Create<Button>(btn => this.OpenSetup(btn, 2), button3Enabled);

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}

		private async void OpenSetup(Button btn, int port)
		{
			PixelPoint startPosition = btn.PointToScreen(new Point(-7, btn.Bounds.Height));
			ControllerConfigWindow wnd = new ControllerConfigWindow();
			WsControllerConfig orgCfg = port == 0 ? Config.ControllerHorizontal : (port == 1 ? Config.ControllerVertical : Config.ControllerPcv2);
			WsControllerConfig cfg = (WsControllerConfig) (port == 0 ? Config.ControllerHorizontal.Clone() : (port == 1 ? Config.ControllerVertical.Clone() : Config.ControllerPcv2.Clone()));
			wnd.DataContext = new ControllerConfigViewModel(port == 0 ? ControllerType.WsController : (port == 1 ? ControllerType.WsControllerVertical : ControllerType.Pcv2Controller), cfg, orgCfg, port);

			if(await wnd.ShowDialogAtPosition<bool>(btn.GetVisualRoot() as Visual, startPosition)) {
				if(port == 0) {
					Config.ControllerHorizontal = cfg;
				} else if(port == 1) {
					Config.ControllerVertical = cfg;
				} else {
					Config.ControllerPcv2 = cfg;
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
