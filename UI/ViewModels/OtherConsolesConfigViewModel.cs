using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Controls;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mesen.ViewModels
{
	public partial class OtherConsolesConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial CvConfig CvConfig { get; set; }
		[ObservableProperty] public partial CvConfig CvOriginalConfig { get; set; }
		[ObservableProperty] public partial OtherConsolesConfigTab SelectedTab { get; set; } = 0;

		public CvInputConfigViewModel CvInput { get; private set; }

		public Enum[] AvailableRegionsCv => new Enum[] {
			ConsoleRegion.Auto,
			ConsoleRegion.Ntsc,
			ConsoleRegion.Pal
		};

		public OtherConsolesConfigViewModel()
		{
			CvConfig = ConfigManager.Config.Cv;
			CvOriginalConfig = CvConfig.Clone();
			CvInput = new CvInputConfigViewModel(CvConfig);

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(CvInput);
			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(CvConfig, (s, e) => { CvConfig.ApplyConfig(); }));
		}
	}

	public enum OtherConsolesConfigTab
	{
		ColecoVision
	}
}
