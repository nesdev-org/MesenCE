using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Mesen.Config
{
	public partial class HexEditorConfig : BaseWindowConfig<HexEditorConfig>
	{
		[ObservableProperty] public partial bool ShowOptionPanel { get; set; } = true;
		[ObservableProperty] public partial bool AutoRefresh { get; set; } = true;
		[ObservableProperty] public partial bool IgnoreRedundantWrites { get; set; } = false;

		[ObservableProperty] public partial int BytesPerRow { get; set; } = 16;

		[ObservableProperty] public partial bool HighDensityTextMode { get; set; } = false;

		[ObservableProperty] public partial bool ShowCharacters { get; set; } = true;
		[ObservableProperty] public partial bool ShowTooltips { get; set; } = true;

		[ObservableProperty] public partial bool HideUnusedBytes { get; set; } = false;
		[ObservableProperty] public partial bool HideReadBytes { get; set; } = false;
		[ObservableProperty] public partial bool HideWrittenBytes { get; set; } = false;
		[ObservableProperty] public partial bool HideExecutedBytes { get; set; } = false;

		[ObservableProperty] public partial HighlightFadeSpeed FadeSpeed { get; set; } = HighlightFadeSpeed.Normal;
		[ObservableProperty] public partial HighlightConfig ReadHighlight { get; set; } = new() { Highlight = true, ColorCode = Colors.Blue.ToUInt32() };
		[ObservableProperty] public partial HighlightConfig WriteHighlight { get; set; } = new() { Highlight = true, ColorCode = Colors.Red.ToUInt32() };
		[ObservableProperty] public partial HighlightConfig ExecHighlight { get; set; } = new() { Highlight = true, ColorCode = Colors.Green.ToUInt32() };

		[ObservableProperty] public partial HighlightConfig LabelHighlight { get; set; } = new() { Highlight = false, ColorCode = Colors.LightPink.ToUInt32() };
		[ObservableProperty] public partial HighlightConfig CodeHighlight { get; set; } = new() { Highlight = false, ColorCode = Colors.DarkSeaGreen.ToUInt32() };
		[ObservableProperty] public partial HighlightConfig DataHighlight { get; set; } = new() { Highlight = false, ColorCode = Colors.LightSteelBlue.ToUInt32() };

		[ObservableProperty] public partial HighlightConfig FrozenHighlight { get; set; } = new() { Highlight = true, ColorCode = Colors.Magenta.ToUInt32() };

		[ObservableProperty] public partial HighlightConfig NesPcmDataHighlight { get; set; } = new() { Highlight = false, ColorCode = Colors.Khaki.ToUInt32() };
		[ObservableProperty] public partial HighlightConfig NesDrawnChrRomHighlight { get; set; } = new() { Highlight = false, ColorCode = Colors.Thistle.ToUInt32() };

		[ObservableProperty] public partial bool HighlightBreakpoints { get; set; } = false;

		[ObservableProperty] public partial MemoryType MemoryType { get; set; } = MemoryType.SnesMemory;

		public HexEditorConfig()
		{
		}
	}

	public enum HighlightFadeSpeed
	{
		NoFade,
		Slow,
		Normal,
		Fast
	}

	public static class HighlightFadeSpeedExtensions
	{
		public static int ToFrameCount(this HighlightFadeSpeed speed)
		{
			return speed switch {
				HighlightFadeSpeed.NoFade => 0,
				HighlightFadeSpeed.Slow => 600,
				HighlightFadeSpeed.Normal => 300,
				HighlightFadeSpeed.Fast => 120,
				_ => 0
			};
		}
	}

	public partial class HighlightConfig : ObservableObject
	{
		[ObservableProperty] public partial bool Highlight { get; set; }
		[ObservableProperty] public partial UInt32 ColorCode { get; set; }

		[JsonIgnore]
		public Color Color
		{
			get { return Color.FromUInt32(ColorCode); }
			set { ColorCode = value.ToUInt32(); }
		}
	}
}
