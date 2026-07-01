using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Utilities;
using Mesen.Windows;
using System;
using System.Diagnostics;
using System.IO;

namespace Mesen.ViewModels
{
	public partial class SetupWizardViewModel : ViewModelBase
	{
		[ObservableProperty] public partial bool StoreInUserProfile { get; set; } = true;

		[ObservableProperty] public partial bool EnableXboxMappings { get; set; } = true;
		[ObservableProperty] public partial bool EnablePsMappings { get; set; }
		[ObservableProperty] public partial bool EnableWasdMappings { get; set; }
		[ObservableProperty] public partial bool EnableArrowMappings { get; set; } = true;

		[ObservableProperty] public partial string InstallLocation { get; set; }

		[ObservableProperty] public partial bool CreateShortcut { get; set; } = true;
		[ObservableProperty] public partial bool CheckForUpdates { get; set; } = true;
		[ObservableProperty] public partial bool IsOsx { get; set; } = OperatingSystem.IsMacOS();

		public SetupWizardViewModel()
		{
			InstallLocation = ConfigManager.DefaultDocumentsFolder;
		}

		partial void OnEnableWasdMappingsChanged(bool value)
		{
			if(value) {
				EnableArrowMappings = false;
			}
		}

		partial void OnEnableArrowMappingsChanged(bool value)
		{
			if(value) {
				EnableWasdMappings = false;
			}
		}

		partial void OnStoreInUserProfileChanged(bool value)
		{
			InstallLocation = StoreInUserProfile ? ConfigManager.DefaultDocumentsFolder : ConfigManager.DefaultPortableFolder;
		}

		public bool Confirm(Window parent)
		{
			string targetFolder = StoreInUserProfile ? ConfigManager.DefaultDocumentsFolder : ConfigManager.DefaultPortableFolder;
			string testFile = Path.Combine(targetFolder, "test.txt");
			try {
				if(!Directory.Exists(targetFolder)) {
					Directory.CreateDirectory(targetFolder);
				}
				File.WriteAllText(testFile, "test");
				File.Delete(testFile);
				InitializeConfig();
				if(CreateShortcut) {
					CreateShortcutFile();
				}
				return true;
			} catch(Exception ex) {
				MesenMsgBox.Show(parent, "CannotWriteToFolder", MessageBoxButtons.OK, MessageBoxIcon.Error, ex.ToString());
			}

			return false;
		}

		private void InitializeConfig()
		{
			ConfigManager.CreateConfig(!StoreInUserProfile);
			DefaultKeyMappingType mappingType = DefaultKeyMappingType.None;
			if(EnableXboxMappings) {
				mappingType |= DefaultKeyMappingType.Xbox;
			}
			if(EnablePsMappings) {
				mappingType |= DefaultKeyMappingType.Ps4;
			}
			if(EnableWasdMappings) {
				mappingType |= DefaultKeyMappingType.WasdKeys;
			}
			if(EnableArrowMappings) {
				mappingType |= DefaultKeyMappingType.ArrowKeys;
			}

			ConfigManager.Config.DefaultKeyMappings = mappingType;
			ConfigManager.Config.Preferences.AutomaticallyCheckForUpdates = CheckForUpdates;
			ConfigManager.Config.Save();
		}

		private void CreateShortcutFile()
		{
			if(OperatingSystem.IsMacOS()) {
				//TODO OSX
				return;
			}

			if(OperatingSystem.IsWindows()) {
				string linkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Mesen.url");
				FileHelper.WriteAllText(linkPath,
					"[InternetShortcut]" + Environment.NewLine +
					"URL=file:///" + Program.ExePath + Environment.NewLine +
					"IconIndex=0" + Environment.NewLine +
					"IconFile=" + Program.ExePath.Replace('\\', '/') + Environment.NewLine
				);
			} else {
				string shortcutFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "mesen.desktop");
				FileAssociationHelper.CreateLinuxShortcutFile(shortcutFile);
				Process.Start("chmod", "744 " + shortcutFile);
			}
		}
	}
}
