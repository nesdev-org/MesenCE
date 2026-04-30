using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mesen.Windows
{
	public class CommandLineHelpWindow : MesenWindow
	{
		public List<CommandLineTabEntry> HelpTabs { get; } = new();

		public CommandLineHelpWindow()
		{
			Dictionary<string, string> switchesPerCategory = CommandLineHelper.GetAvailableSwitches();
			foreach(var kvp in switchesPerCategory) {
				HelpTabs.Add(new() { Name = kvp.Key, Content = kvp.Value });
			}

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void btnOk_OnClick(object? sender, RoutedEventArgs e)
		{
			Close();
		}
	}

	public class CommandLineTabEntry
	{
		public string Name { get; set; } = "";
		public string Content { get; set; } = "";
	}
}
