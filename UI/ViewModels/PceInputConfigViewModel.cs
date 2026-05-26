using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Utilities;
using System;

namespace Mesen.ViewModels
{
	public partial class PceInputConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial PcEngineConfig Config { get; set; }
		[ObservableProperty] public partial bool HasTurboTap { get; private set; }

		public Enum[] AvailableControllerTypesP1 => new Enum[] {
			ControllerType.None,
			ControllerType.PceController,
			ControllerType.PceAvenuePad6,
			ControllerType.PceTurboTap,
		};

		public Enum[] AvailableControllerTypesTurboTap => new Enum[] {
			ControllerType.None,
			ControllerType.PceController,
			ControllerType.PceAvenuePad6,
		};

		[Obsolete("For designer only")]
		public PceInputConfigViewModel() : this(new PcEngineConfig()) { }

		public PceInputConfigViewModel(PcEngineConfig config)
		{
			Config = config;

			AddDisposable(Config.Port1.ObserveProp(nameof(ControllerConfig.Type), () => {
				HasTurboTap = Config.Port1.Type == ControllerType.PceTurboTap;
			}));
		}
	}
}
