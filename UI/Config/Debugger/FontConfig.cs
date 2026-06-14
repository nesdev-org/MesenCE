using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.ViewModels;
using System;

namespace Mesen.Config
{
	public partial class FontConfig : BaseConfig<FontConfig>
	{
		[ObservableProperty] public partial string FontFamily { get; set; } = "";
		[ObservableProperty] public partial double FontSize { get; set; } = 12;
	}
}
