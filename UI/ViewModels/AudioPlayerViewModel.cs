using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.ViewModels
{
	public partial class AudioPlayerViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial AudioPlayerConfig Config { get; set; }
		[ObservableProperty] public partial bool IsPaused { get; set; }

		public AudioPlayerViewModel()
		{
			Config = ConfigManager.Config.AudioPlayer;

			AddDisposable(Config.ObserveProp(nameof(Config.Volume), () => {
				Config.ApplyConfig();
			}));
		}

		public void UpdatePauseFlag()
		{
			IsPaused = EmuApi.IsPaused();
		}
	}
}
