using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Mesen.Windows
{
	public class SaveSpcFileWindow : MesenWindow
	{
		private SaveSpcFileViewModel _model;

		public SaveSpcFileWindow()
		{
			_model = new SaveSpcFileViewModel();
			DataContext = _model;

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private async void SaveSpcFile()
		{
			string? filename = await FileDialogHelper.SaveFile(ConfigManager.DumpsFolder, EmuApi.GetRomInfo().GetRomName() + ".spc", this, FileDialogHelper.SpcExt);
			if(filename != null) {
				_model.SaveSpcFile(filename);
				Close(true);
			}
		}

		private void Ok_OnClick(object sender, RoutedEventArgs e)
		{
			SaveSpcFile();
		}

		private void Cancel_OnClick(object sender, RoutedEventArgs e)
		{
			Close(false);
		}
	}
}
