using Avalonia.Media;
using Mesen.Interop;
using Mesen.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Mesen.Config
{
	public partial class EventViewerCategoryCfg : ViewModelBase
	{
		[ObservableProperty] public partial bool Visible { get; set; } = true;
		[ObservableProperty] public partial UInt32 Color { get; set; }

		public EventViewerCategoryCfg() { }

		public EventViewerCategoryCfg(Color color)
		{
			Color = color.ToUInt32();
		}

		public static implicit operator InteropEventViewerCategoryCfg(EventViewerCategoryCfg cfg)
		{
			return new InteropEventViewerCategoryCfg() {
				Visible = cfg.Visible,
				Color = cfg.Color
			};
		}
	}
}
