using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config
{
	public partial class AudioPlayerConfig : BaseConfig<AudioPlayerConfig>
	{
		[ObservableProperty] public partial UInt32 Volume { get; set; } = 100;
		[ObservableProperty] public partial bool Repeat { get; set; } = false;
		[ObservableProperty] public partial bool Shuffle { get; set; } = false;

		public void ApplyConfig()
		{
			ConfigApi.SetAudioPlayerConfig(new InteropAudioPlayerConfig() {
				Volume = Volume,
				Repeat = Repeat,
				Shuffle = Shuffle,
			});
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InteropAudioPlayerConfig
	{
		public UInt32 Volume;
		[MarshalAs(UnmanagedType.I1)] public bool Repeat;
		[MarshalAs(UnmanagedType.I1)] public bool Shuffle;
	}
}
