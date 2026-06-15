using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config
{
	public partial class VideoRecordConfig : BaseConfig<VideoRecordConfig>
	{
		[ObservableProperty] public partial VideoCodec Codec { get; set; } = VideoCodec.CSCD;
		[ObservableProperty] public partial UInt32 CompressionLevel { get; set; } = 6;
		[ObservableProperty] public partial bool RecordSystemHud { get; set; } = false;
		[ObservableProperty] public partial bool RecordInputHud { get; set; } = false;
	}

	public enum VideoCodec
	{
		None = 0,
		ZMBV = 1,
		CSCD = 2,
		GIF = 3
	}
}
