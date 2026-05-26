using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Mesen.Config
{
	public partial class NetplayConfig : BaseConfig<NetplayConfig>
	{
		[ObservableProperty] public partial string Host { get; set; } = "localhost";
		[ObservableProperty] public partial UInt16 Port { get; set; } = 8888;
		[ObservableProperty] public partial string Password { get; set; } = "";

		[ObservableProperty] public partial UInt16 ServerPort { get; set; } = 8888;
		[ObservableProperty] public partial string ServerPassword { get; set; } = "";
	}
}
