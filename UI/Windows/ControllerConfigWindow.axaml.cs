using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mesen.Config;
using Mesen.Utilities;
using Mesen.ViewModels;
using System.ComponentModel;

namespace Mesen.Windows
{
	public class ControllerConfigWindow : MesenWindow
	{
		private ControllerConfigViewModel Model => (ControllerConfigViewModel)DataContext!;
		private bool _promptToSave = true;

		public ControllerConfigWindow()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if(e.Key == Key.Escape) {
				Close();
			}
		}

		private async void DisplaySaveChangesPrompt()
		{
			DialogResult result = await MesenMsgBox.Show(this, "PromptKeepChanges", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			switch(result) {
				case DialogResult.Yes: _promptToSave = false; Close(true); break;
				case DialogResult.No: _promptToSave = false; Close(false); break;
				default: break;
			}
		}

		protected override void OnClosing(WindowClosingEventArgs e)
		{
			base.OnClosing(e);
			if(Design.IsDesignMode) {
				return;
			}

			if(_promptToSave && !Model.Config.IsIdentical(Model.OriginalConfig)) {
				e.Cancel = true;
				DisplaySaveChangesPrompt();
				return;
			}
		}

		private void BtnOk_OnClick(object sender, RoutedEventArgs e)
		{
			_promptToSave = false;
			Close(true);
		}

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
		{
			_promptToSave = false;
			Close(false);
		}

		private void BtnPreset_OnClick(object sender, RoutedEventArgs e)
		{
			((Button)sender).ContextMenu?.Open();
		}

		private void SetDefaultMappings(KeyPresetType? preset)
		{
			int index = this.GetControl<TabControl>("tabMain").SelectedIndex;
			ControllerConfig cfg = Model.Config;
			switch(index) {
				case 0: cfg.Mapping1.SetDefaultKeys(Model.Type, preset); Model.KeyMapping1.RefreshCustomKeys(); break;
				case 1: cfg.Mapping2.SetDefaultKeys(Model.Type, preset); Model.KeyMapping2.RefreshCustomKeys(); break;
				case 2: cfg.Mapping3.SetDefaultKeys(Model.Type, preset); Model.KeyMapping3.RefreshCustomKeys(); break;
				case 3: cfg.Mapping4.SetDefaultKeys(Model.Type, preset); Model.KeyMapping4.RefreshCustomKeys(); break;
			}
		}

		private void BtnClearBindings_OnClick(object sender, RoutedEventArgs e)
		{
			int index = this.GetControl<TabControl>("tabMain").SelectedIndex;
			ControllerConfig cfg = Model.Config;
			switch(index) {
				case 0: cfg.Mapping1.ClearKeys(Model.Type); Model.KeyMapping1.RefreshCustomKeys(); break;
				case 1: cfg.Mapping2.ClearKeys(Model.Type); Model.KeyMapping2.RefreshCustomKeys(); break;
				case 2: cfg.Mapping3.ClearKeys(Model.Type); Model.KeyMapping3.RefreshCustomKeys(); break;
				case 3: cfg.Mapping4.ClearKeys(Model.Type); Model.KeyMapping4.RefreshCustomKeys(); break;
			}
		}

		private void MnuWasdLayout_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.WasdKeys);
		}

		private void MnuArrowLayout_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.ArrowKeys);
		}

		private void MnuXboxLayout1_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.XboxP1);
		}

		private void MnuXboxLayout1Alt_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.XboxP1Alt);
		}

		private void MnuXboxLayout2_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.XboxP2);
		}

		private void MnuXboxLayout2Alt_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.XboxP2Alt);
		}

		private void MnuPs4Layout1_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.Ps4P1);
		}

		private void MnuPs4Layout1Alt_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.Ps4P1Alt);
		}

		private void MnuPs4Layout2_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.Ps4P2);
		}

		private void MnuPs4Layout2Alt_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(KeyPresetType.Ps4P2Alt);
		}

		private void BtnSetDefaultBindings_OnClick(object sender, RoutedEventArgs e)
		{
			SetDefaultMappings(null);
		}
	}
}
