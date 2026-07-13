using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using System;
using System.Linq;

namespace Mesen.ViewModels
{
	public partial class SnesInputConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial SnesConfig Config { get; set; }

		public Enum[] AvailableControllerTypesP1 => new Enum[] {
			ControllerType.None,
			ControllerType.SnesController,
			ControllerType.SnesMouse,
			ControllerType.SuperScope,
			ControllerType.SnesNttDataKeypad,
			ControllerType.Multitap,
			ControllerType.SnesRumbleController,
			ControllerType.SnesBlueRetroController,
		};

		public Enum[] AvailableControllerTypesP2 => new Enum[] {
			ControllerType.None,
			ControllerType.SnesController,
			ControllerType.SnesMouse,
			ControllerType.SuperScope,
			ControllerType.SnesNttDataKeypad,
			ControllerType.AsciiTurboFileTwinTf2,
			ControllerType.AsciiTurboFileTwinStf,
			ControllerType.Multitap,
		};

		public Enum[] AvailableControllerTypesMultitap => new Enum[] {
			ControllerType.None,
			ControllerType.SnesController,
			ControllerType.SnesMouse,
			ControllerType.SuperScope,
		};

		[Obsolete("For designer only")]
		public SnesInputConfigViewModel() : this(new SnesConfig()) { }

		public SnesInputConfigViewModel(SnesConfig config)
		{
			Config = config;
		}
	}
}
