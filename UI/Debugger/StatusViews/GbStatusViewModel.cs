using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class GbStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial byte RegA { get; set; }
		[ObservableProperty] public partial byte RegB { get; set; }
		[ObservableProperty] public partial byte RegC { get; set; }
		[ObservableProperty] public partial byte RegD { get; set; }
		[ObservableProperty] public partial byte RegE { get; set; }
		[ObservableProperty] public partial byte RegFlags { get; set; }

		[ObservableProperty] public partial byte RegH { get; set; }
		[ObservableProperty] public partial byte RegL { get; set; }

		[ObservableProperty] public partial UInt16 RegSP { get; set; }
		[ObservableProperty] public partial UInt16 RegPC { get; set; }

		[ObservableProperty] public partial UInt16 Scanline { get; set; }
		[ObservableProperty] public partial UInt16 Cycle { get; set; }

		[ObservableProperty] public partial bool FlagCarry { get; set; }
		[ObservableProperty] public partial bool FlagHalf { get; set; }
		[ObservableProperty] public partial bool FlagAddSub { get; set; }
		[ObservableProperty] public partial bool FlagZero { get; set; }

		[ObservableProperty] public partial bool FlagHalted { get; set; }
		[ObservableProperty] public partial bool FlagEiPending { get; set; }
		[ObservableProperty] public partial bool FlagIme { get; set; }

		[ObservableProperty] public partial string StackPreview { get; private set; } = "";

		public GbStatusViewModel()
		{
			bool preventUpdate = false;

			this.ObserveProp([nameof(FlagCarry), nameof(FlagHalf), nameof(FlagAddSub), nameof(FlagZero)], () => {
				if(!preventUpdate) {
					UpdateFlagsValue();
				}
			});

			this.ObserveProp(nameof(RegFlags), () => {
				preventUpdate = true;
				FlagCarry = (RegFlags & (byte)GameboyFlags.Carry) != 0;
				FlagHalf = (RegFlags & (byte)GameboyFlags.HalfCarry) != 0;
				FlagAddSub = (RegFlags & (byte)GameboyFlags.AddSub) != 0;
				FlagZero = (RegFlags & (byte)GameboyFlags.Zero) != 0;
				preventUpdate = false;
			});
		}

		private void UpdateFlagsValue()
		{
			RegFlags = (byte)(
				(FlagCarry ? (byte)GameboyFlags.Carry : 0) |
				(FlagHalf ? (byte)GameboyFlags.HalfCarry : 0) |
				(FlagAddSub ? (byte)GameboyFlags.AddSub : 0) |
				(FlagZero ? (byte)GameboyFlags.Zero : 0)
			);
		}

		protected override void InternalUpdateUiState()
		{
			GbState state = DebugApi.GetConsoleState<GbState>(ConsoleType.Gameboy);

			GbCpuState cpu = state.Cpu;
			GbPpuState ppu = DebugApi.GetPpuState<GbPpuState>(CpuType.Gameboy);

			UpdateCycleCount(state.Cpu.CycleCount);

			RegA = cpu.A;
			RegB = cpu.B;
			RegC = cpu.C;
			RegD = cpu.D;
			RegE = cpu.E;
			RegFlags = cpu.Flags;

			RegH = cpu.H;
			RegL = cpu.L;

			RegPC = cpu.PC;
			RegSP = cpu.SP;

			FlagEiPending = cpu.EiPending;
			FlagHalted = cpu.HaltCounter > 0;
			FlagIme = cpu.IME;

			Scanline = ppu.Scanline;
			Cycle = ppu.Cycle;

			StringBuilder sb = new StringBuilder();
			for(UInt32 i = (UInt32)cpu.SP; (i & 0xFF) != 0; i++) {
				sb.Append($"${DebugApi.GetMemoryValue(MemoryType.GameboyMemory, i):X2} ");
			}
			StackPreview = sb.ToString();
		}

		protected override void InternalUpdateConsoleState()
		{
			GbCpuState cpu = DebugApi.GetCpuState<GbCpuState>(CpuType.Gameboy);

			cpu.A = RegA;
			cpu.B = RegB;
			cpu.C = RegC;
			cpu.D = RegD;
			cpu.E = RegE;
			cpu.Flags = RegFlags;

			cpu.H = RegH;
			cpu.L = RegL;

			cpu.PC = RegPC;
			cpu.SP = RegSP;

			cpu.EiPending = FlagEiPending;
			if(cpu.HaltCounter == 0 && FlagHalted) {
				cpu.HaltCounter = 1;
			} else if(!FlagHalted) {
				cpu.HaltCounter = 0;
			}
			cpu.IME = FlagIme;

			DebugApi.SetCpuState(cpu, CpuType.Gameboy);
		}
	}
}
