using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.IO;

namespace Mesen.ViewModels
{
	public partial class VideoRecordConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial string SavePath { get; set; }
		[ObservableProperty] public partial VideoRecordConfig Config { get; set; }
		[ObservableProperty] public partial bool CompressionAvailable { get; private set; }

		public VideoRecordConfigViewModel()
		{
			Config = ConfigManager.Config.VideoRecord.Clone();

			SavePath = Path.Join(ConfigManager.AviFolder, EmuApi.GetRomInfo().GetRomName() + (Config.Codec == VideoCodec.GIF ? ".gif" : ".avi"));

			AddDisposable(Config.ObserveProp(nameof(VideoRecordConfig.Codec), () => {
				CompressionAvailable = Config.Codec == VideoCodec.ZMBV || Config.Codec == VideoCodec.CSCD;

				if(Config.Codec == VideoCodec.GIF && Path.GetExtension(SavePath).ToLowerInvariant() != ".gif") {
					SavePath = Path.ChangeExtension(SavePath, ".gif");
				} else if(Config.Codec != VideoCodec.GIF && Path.GetExtension(SavePath).ToLowerInvariant() == ".gif") {
					SavePath = Path.ChangeExtension(SavePath, ".avi");
				}
			}));
		}

		public void SaveConfig()
		{
			ConfigManager.Config.VideoRecord = Config.Clone();
		}
	}
}
