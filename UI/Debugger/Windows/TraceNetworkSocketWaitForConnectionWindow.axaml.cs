using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Mesen.Config;
using Mesen.Config.Shortcuts;
using Mesen.Interop;
using Mesen.Localization;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Mesen.Windows
{
	public enum TraceNetworkSocketWaitForConnectionWindowResult
	{
		Cancelled,
		ConnectionEstablished,
	}

	public class TraceNetworkSocketWaitForConnectionWindow : MesenWindow
	{
		private DispatcherTimer _timer;

		public TraceNetworkSocketWaitForConnectionWindow()
		{
			InitializeComponent();

			_timer = new DispatcherTimer(TimeSpan.FromMilliseconds(25), DispatcherPriority.Normal, (s, e) => CheckForConnection());
			_timer.Start();
		}

		protected override void OnClosed(EventArgs e)
		{
			_timer?.Stop();
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

		private void CheckForConnection()
		{
			if(DebugApi.BeginNetworkLogConnection()) {
				this.Close(TraceNetworkSocketWaitForConnectionWindowResult.ConnectionEstablished);
			}
		}

		private void OnCancel(object sender, RoutedEventArgs e)
		{
			this.Close(TraceNetworkSocketWaitForConnectionWindowResult.Cancelled);
		}
	}
}
