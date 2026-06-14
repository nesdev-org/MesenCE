using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.IO;

namespace Mesen.ViewModels
{
	public partial class MovieRecordConfigViewModel : ViewModelBase
	{
		[ObservableProperty] public partial string SavePath { get; set; }
		[ObservableProperty] public partial MovieRecordConfig Config { get; set; }

		public MovieRecordConfigViewModel()
		{
			Config = ConfigManager.Config.MovieRecord.Clone();

			SavePath = Path.Join(ConfigManager.MovieFolder, EmuApi.GetRomInfo().GetRomName() + "." + FileDialogHelper.MesenMovieExt);
		}

		public void SaveConfig()
		{
			ConfigManager.Config.MovieRecord = Config.Clone();
		}
	}
}
