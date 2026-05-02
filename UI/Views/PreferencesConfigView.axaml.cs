using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using Mesen.Config;
using Mesen.Controls;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.Windows;
using System.Collections.Generic;

namespace Mesen.Views
{
	public class PreferencesConfigView : UserControl
	{
		public PreferencesConfigView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void btnResetLagCounter_OnClick(object sender, RoutedEventArgs e)
		{
			InputApi.ResetLagCounter();
		}

		private void btnChangeStorageFolder_OnClick(object sender, RoutedEventArgs e)
		{
			ShowSelectFolderWindow();
		}

		private async void ShowSelectFolderWindow()
		{
			SelectStorageFolderWindow wnd = new();
			if(await wnd.ShowCenteredDialog<bool>(this.GetVisualRoot() as Visual)) {
				(this.GetVisualRoot() as Window)?.Close();
				ApplicationHelper.GetMainWindow()?.Close();
				ConfigManager.RestartMesen();
			}
		}
	}
}
