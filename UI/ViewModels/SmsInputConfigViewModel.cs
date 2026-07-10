using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using System;

namespace Mesen.ViewModels
{
	public partial class SmsInputConfigViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial SmsConfig Config { get; set; }

		public Enum[] AvailableControllerTypesP12 => new Enum[] {
			ControllerType.None,
			ControllerType.SmsController,
			ControllerType.SmsLightPhaser,
		};

		[Obsolete("For designer only")]
		public SmsInputConfigViewModel() : this(new SmsConfig()) { }

		public SmsInputConfigViewModel(SmsConfig config)
		{
			Config = config;
		}
	}
}
