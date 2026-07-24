using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.ViewModels;

namespace Mesen.Config
{
	public partial class PceEventViewerConfig : ViewModelBase
	{
		[ObservableProperty] public partial EventViewerCategoryCfg VdcStatusReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x29, 0xC9, 0xC9));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcVramWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x9F, 0x93, 0xC6));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcVramReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x29, 0xC9, 0x29));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcRegSelectWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x29, 0x29, 0xC9));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcControlWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x29, 0xC6, 0x93));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcRcrWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xC6, 0x9F, 0x93));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcHvConfigWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x93, 0xC6, 0x9F));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcMemoryWidthWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xC6, 0x29, 0xC6));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcScrollWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xC6, 0xC6, 0x29));
		[ObservableProperty] public partial EventViewerCategoryCfg VdcDmaWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x93, 0x29, 0xC9));

		[ObservableProperty] public partial EventViewerCategoryCfg CdRomWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xDA, 0xB4, 0x7A));
		[ObservableProperty] public partial EventViewerCategoryCfg CdRomReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x44, 0x53, 0xD7));
		[ObservableProperty] public partial EventViewerCategoryCfg AdpcmWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xD7, 0x7A, 0xDA));
		[ObservableProperty] public partial EventViewerCategoryCfg AdpcmReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xB4, 0x53, 0x44));
		[ObservableProperty] public partial EventViewerCategoryCfg ArcadeCardWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x7A, 0xDA, 0xD7));
		[ObservableProperty] public partial EventViewerCategoryCfg ArcadeCardReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x44, 0xB4, 0x53));

		[ObservableProperty] public partial EventViewerCategoryCfg VceWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xB4, 0x7A, 0xDA));
		[ObservableProperty] public partial EventViewerCategoryCfg VceReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x53, 0xD7, 0x44));

		[ObservableProperty] public partial EventViewerCategoryCfg PsgWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xE2, 0x51, 0xF7));
		[ObservableProperty] public partial EventViewerCategoryCfg PsgReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xF9, 0xFE, 0xAC));

		[ObservableProperty] public partial EventViewerCategoryCfg TimerWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xD1, 0xDD, 0x42));
		[ObservableProperty] public partial EventViewerCategoryCfg TimerReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x00, 0x75, 0x97));

		[ObservableProperty] public partial EventViewerCategoryCfg VpcWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x56, 0x9D, 0xB7));
		[ObservableProperty] public partial EventViewerCategoryCfg VpcReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x9D, 0x56, 0xB7));

		[ObservableProperty] public partial EventViewerCategoryCfg IoWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xFF, 0x5E, 0x5E));
		[ObservableProperty] public partial EventViewerCategoryCfg IoReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x18, 0x98, 0xE4));
		[ObservableProperty] public partial EventViewerCategoryCfg IrqControlWrites { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xFF, 0x5E, 0xDD));
		[ObservableProperty] public partial EventViewerCategoryCfg IrqControlReads { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xE2, 0x98, 0xE4));

		[ObservableProperty] public partial EventViewerCategoryCfg Irq { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0xC4, 0xF4, 0x7A));

		[ObservableProperty] public partial EventViewerCategoryCfg MarkedBreakpoints { get; set; } = new EventViewerCategoryCfg(Color.FromRgb(0x18, 0x98, 0xE4));

		[ObservableProperty] public partial PceEventViewerSgxFilter SuperGrafxFilter { get; set; } = PceEventViewerSgxFilter.Both;
		[ObservableProperty] public partial bool ShowPreviousFrameEvents { get; set; } = true;

		public InteropPceEventViewerConfig ToInterop()
		{
			return new InteropPceEventViewerConfig() {
				VdcStatusReads = this.VdcStatusReads,
				VdcVramWrites = this.VdcVramWrites,
				VdcVramReads = this.VdcVramReads,
				VdcRegSelectWrites = this.VdcRegSelectWrites,
				VdcControlWrites = this.VdcControlWrites,
				VdcRcrWrites = this.VdcRcrWrites,
				VdcHvConfigWrites = this.VdcHvConfigWrites,
				VdcMemoryWidthWrites = this.VdcMemoryWidthWrites,
				VdcScrollWrites = this.VdcScrollWrites,
				VdcDmaWrites = this.VdcDmaWrites,

				VceWrites = this.VceWrites,
				VceReads = this.VceReads,
				PsgWrites = this.PsgWrites,
				PsgReads = this.PsgReads,
				TimerWrites = this.TimerWrites,
				TimerReads = this.TimerReads,
				IoWrites = this.IoWrites,
				IoReads = this.IoReads,
				IrqControlWrites = this.IrqControlWrites,
				IrqControlReads = this.IrqControlReads,

				CdRomWrites = this.CdRomWrites,
				CdRomReads = this.CdRomReads,
				AdpcmWrites = this.AdpcmWrites,
				AdpcmReads = this.AdpcmReads,
				ArcadeCardReads = this.ArcadeCardReads,
				ArcadeCardWrites = this.ArcadeCardWrites,

				VpcWrites = this.VpcWrites,
				VpcReads = this.VpcReads,

				Irq = this.Irq,
				MarkedBreakpoints = this.MarkedBreakpoints,
				SuperGrafxFilter = this.SuperGrafxFilter,
				ShowPreviousFrameEvents = this.ShowPreviousFrameEvents
			};
		}
	}

	public enum PceEventViewerSgxFilter
	{
		Both,
		Vdc1,
		Vdc2
	}
}
