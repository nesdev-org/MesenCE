using Avalonia.Controls;
using Mesen.Config;
using Mesen.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.ViewModels
{
	public partial class EmulationConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial EmulationConfig Config { get; set; }
		[ObservableProperty] public partial EmulationConfig OriginalConfig { get; set; }

		public EmulationConfigViewModel()
		{
			Config = ConfigManager.Config.Emulation;
			OriginalConfig = Config.Clone();

			if(Design.IsDesignMode) {
				return;
			}

			AddDisposable(ReactiveHelper.RegisterRecursiveObserver(Config, (s, e) => { Config.ApplyConfig(); }));
		}
	}
}
