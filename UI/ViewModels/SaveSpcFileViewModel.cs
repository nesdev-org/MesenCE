using Mesen.Debugger;
using Mesen.Debugger.Utilities;
using Mesen.Interop;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mesen.ViewModels
{
	public class SaveSpcFileViewModel : ViewModelBase
	{
		[Reactive] public string GameTitle { get; set; } = "";
		[Reactive] public string SongTitle { get; set; } = "";
		[Reactive] public string SongArtist { get; set; } = "";

		public SaveSpcFileViewModel()
		{

		}

		public void WriteFixedLengthString(BinaryWriter writer, string text, int length)
		{
			byte[] textAsBytes = Encoding.ASCII.GetBytes(text);
			byte[] buffer = new byte[length];

			for(int i = 0; i < length; i++) {
				buffer[i] = (i < textAsBytes.Length) ? textAsBytes[i] : (byte)0;
			}

			writer.Write(buffer);
		}

		public void SaveSpcFile(string filename)
		{
			bool releaseDebugger = !DebugWindowManager.HasOpenedDebugWindows();
			bool paused = EmuApi.IsPaused();

			using(FileStream stream = File.Open(filename, FileMode.Create)) {
				using(BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false)) {
					SpcState cpu = DebugApi.GetCpuState<SpcState>(CpuType.Spc);
					byte[] spcRam = DebugApi.GetMemoryState(MemoryType.SpcRam);
					byte[] spcMemory = DebugApi.GetMemoryState(MemoryType.SpcMemory);
					byte[] dspRegisters = DebugApi.GetMemoryState(MemoryType.SpcDspRegisters);

					WriteFixedLengthString(writer, "SNES-SPC700 Sound File Data v0.30", 33);
					writer.Write((short)0x1a1a);
					writer.Write((byte)26); // Has ID666 tags
					writer.Write((byte)30); // Minor version number
					writer.Write(cpu.PC);
					writer.Write(cpu.A);
					writer.Write(cpu.X);
					writer.Write(cpu.Y);
					writer.Write((byte)cpu.PS);
					writer.Write(cpu.SP);
					writer.Write((short)0); // Reserved
					WriteFixedLengthString(writer, SongTitle, 32);
					WriteFixedLengthString(writer, GameTitle, 32);
					WriteFixedLengthString(writer, "", 16); // Name of dumper
					WriteFixedLengthString(writer, "", 32); // Comments
					WriteFixedLengthString(writer, DateTime.Now.ToString("MM/dd/yyyy"), 11); // Date dumped
					WriteFixedLengthString(writer, "", 3);  // Number of seconds to play song before fading out
					WriteFixedLengthString(writer, "", 5);  // Length of fade in milliseconds
					WriteFixedLengthString(writer, SongArtist, 32);
					writer.Write((short)0); // No channels disabled, unknown emulator
					WriteFixedLengthString(writer, "", 45); // Reserved

					// Include the most recently written values for the write-only registers
					spcMemory[0xF0] = spcRam[0xF0];
					spcMemory[0xF1] = spcRam[0xF1];
					spcMemory[0xFA] = spcRam[0xFA];
					spcMemory[0xFB] = spcRam[0xFB];
					spcMemory[0xFC] = spcRam[0xFC];

					writer.Write(spcMemory, 0, 0x10000);
					writer.Write(dspRegisters, 0, 128);
					WriteFixedLengthString(writer, "", 64); // Unused
					writer.Write(spcRam, 0x10000 - 64, 64); // Last 64 bytes of RAM, in case IPL was enabled
				}
			}

			if(releaseDebugger) {
				//The debug calls to get SPC state will initialize the debugger - stop the debugger if no other debug window is opened
				DebugApi.ReleaseDebugger();
				if(paused) {
					EmuApi.Pause();
				}
			}
		}
	}
}
