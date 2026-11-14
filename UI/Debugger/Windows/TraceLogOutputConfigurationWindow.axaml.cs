using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mesen.Config;
using Mesen.Debugger.ViewModels;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using Mesen.ViewModels;

namespace Mesen.Windows
{
	public enum TraceLogOutputConfigurationWindowResult
	{
		Cancelled,
		ConnectionEstablished,
	}

	public class TraceLogOutputConfigurationWindow : MesenWindow
	{
		private TraceLogOutputConfigurationViewModel _model;

		public TraceLogOutputConfig Config
		{
			get => _model.Config;
		}

		[Obsolete("For designer only")]
		public TraceLogOutputConfigurationWindow() : this(new()) { }

		public TraceLogOutputConfigurationWindow(TraceLogOutputConfigurationViewModel model)
		{
			InitializeComponent();
			_model = model;

			DataContext = model;

			if(Design.IsDesignMode) {
				return;
			}

			_model.Config.LoadWindowSettings(this);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);
		}

		protected override void OnClosing(WindowClosingEventArgs e)
		{
			base.OnClosing(e);
		}

		private async void OnStartLogging(object sender, RoutedEventArgs e)
		{
			TraceLogOutputSharedConfig inputSharedConfig = Config.SharedConfig;

			if(!Config.ShowAdvancedSettings) {
				inputSharedConfig = new();
			}

			InteropTraceLogOutputSharedConfig sharedConfig = new(inputSharedConfig, MainWindowViewModel.Instance.RomInfo.ConsoleType.GetMainCpuType());

			switch(Config.OutputTarget) {
				case TraceLogOutputTarget.File:
					InteropTraceLogOutputFileConfig fileConfig = new(Config.FileConfig);
					DebugApi.StartLogTraceToFile(fileConfig, sharedConfig);
					this.Close(TraceLogOutputConfigurationWindowResult.ConnectionEstablished);
					break;

				case TraceLogOutputTarget.NetworkSocket:
					InteropTraceLogOutputNetworkSocketConfig socketConfig = new(Config.SocketConfig);

					if(DebugApi.OpenNetworkLogSocket(socketConfig, sharedConfig)) {
						TraceNetworkSocketWaitForConnectionWindow wnd = new TraceNetworkSocketWaitForConnectionWindow();
						TraceNetworkSocketWaitForConnectionWindowResult result = await wnd.ShowCenteredDialog<TraceNetworkSocketWaitForConnectionWindowResult>(this.GetWindow());

						if(result == TraceNetworkSocketWaitForConnectionWindowResult.Cancelled) {
							DebugApi.CloseNetworkLogSocket();
							return;
						}

						Debug.Assert(result == TraceNetworkSocketWaitForConnectionWindowResult.ConnectionEstablished);

						this.Close(TraceLogOutputConfigurationWindowResult.ConnectionEstablished);
					} else {
						await MesenMsgBox.Show(null, "CreatingTraceLogNetworkSocketFailed", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					break;
			}
		}

		private void OnCancel(object sender, RoutedEventArgs e)
		{
			this.Close(TraceLogOutputConfigurationWindowResult.Cancelled);
		}

		private async void OnFileSelect(object sender, RoutedEventArgs e)
		{
			string inputDirectory = ConfigManager.DebuggerFolder;
			string inputFilename = EmuApi.GetRomInfo().GetRomName() + ".txt";

			if(Config.FileConfig.FileName != "") {
				FileInfo fileInfo = new(Config.FileConfig.FileName);
				inputDirectory = fileInfo.DirectoryName ?? inputDirectory;
				inputFilename = fileInfo.Name ?? inputFilename;
			}

			string? filename = await FileDialogHelper.SaveFile(inputDirectory, inputFilename, this, FileDialogHelper.TraceExt);

			if(filename != null) {
				Config.FileConfig.FileName = filename;
			}
		}
	}
}
