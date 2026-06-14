using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class SmsStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial byte RegA { get; set; }
		[ObservableProperty] public partial byte RegB { get; set; }
		[ObservableProperty] public partial byte RegC { get; set; }
		[ObservableProperty] public partial byte RegD { get; set; }
		[ObservableProperty] public partial byte RegE { get; set; }
		[ObservableProperty] public partial byte RegFlags { get; set; }

		[ObservableProperty] public partial byte RegH { get; set; }
		[ObservableProperty] public partial byte RegL { get; set; }

		[ObservableProperty] public partial UInt16 RegIX { get; set; }
		[ObservableProperty] public partial UInt16 RegIY { get; set; }

		[ObservableProperty] public partial byte RegR { get; set; }
		[ObservableProperty] public partial byte RegI { get; set; }

		[ObservableProperty] public partial byte RegAltA { get; set; }
		[ObservableProperty] public partial byte RegAltFlags { get; set; }
		[ObservableProperty] public partial byte RegAltB { get; set; }
		[ObservableProperty] public partial byte RegAltC { get; set; }
		[ObservableProperty] public partial byte RegAltD { get; set; }
		[ObservableProperty] public partial byte RegAltE { get; set; }
		[ObservableProperty] public partial byte RegAltH { get; set; }
		[ObservableProperty] public partial byte RegAltL { get; set; }

		[ObservableProperty] public partial UInt16 RegSP { get; set; }
		[ObservableProperty] public partial UInt16 RegPC { get; set; }

		[ObservableProperty] public partial UInt16 Scanline { get; set; }
		[ObservableProperty] public partial UInt16 Cycle { get; set; }

		[ObservableProperty] public partial bool FlagCarry { get; set; }
		[ObservableProperty] public partial bool FlagAddSub { get; set; }
		[ObservableProperty] public partial bool FlagParity { get; set; }
		[ObservableProperty] public partial bool FlagF3 { get; set; }
		[ObservableProperty] public partial bool FlagHalf { get; set; }
		[ObservableProperty] public partial bool FlagF5 { get; set; }
		[ObservableProperty] public partial bool FlagZero { get; set; }
		[ObservableProperty] public partial bool FlagSign { get; set; }

		[ObservableProperty] public partial bool FlagIFF1 { get; set; }
		[ObservableProperty] public partial bool FlagIFF2 { get; set; }
		[ObservableProperty] public partial bool FlagHalted { get; set; }
		[ObservableProperty] public partial byte IM { get; set; }

		[ObservableProperty] public partial string StackPreview { get; private set; } = "";

		public SmsStatusViewModel()
		{
			bool preventUpdate = false;

			this.ObserveProp([
				nameof(FlagCarry), nameof(FlagAddSub), nameof(FlagParity), nameof(FlagF3),
				nameof(FlagHalf), nameof(FlagF5), nameof(FlagZero), nameof(FlagSign),

			], () => {
				if(!preventUpdate) {
					UpdateFlagsValue();
				}
			});

			this.ObserveProp(nameof(RegFlags), () => {
				preventUpdate = true;
				FlagCarry = (RegFlags & (byte)SmsCpuFlags.Carry) != 0;
				FlagAddSub = (RegFlags & (byte)SmsCpuFlags.AddSub) != 0;
				FlagParity = (RegFlags & (byte)SmsCpuFlags.Parity) != 0;
				FlagF3 = (RegFlags & (byte)SmsCpuFlags.F3) != 0;
				FlagHalf = (RegFlags & (byte)SmsCpuFlags.HalfCarry) != 0;
				FlagF5 = (RegFlags & (byte)SmsCpuFlags.F5) != 0;
				FlagZero = (RegFlags & (byte)SmsCpuFlags.Zero) != 0;
				FlagSign = (RegFlags & (byte)SmsCpuFlags.Sign) != 0;
				preventUpdate = false;
			});
		}

		private void UpdateFlagsValue()
		{
			RegFlags = (byte)(
				(FlagCarry ? (byte)SmsCpuFlags.Carry : 0) |
				(FlagAddSub ? (byte)SmsCpuFlags.AddSub : 0) |
				(FlagParity ? (byte)SmsCpuFlags.Parity : 0) |
				(FlagF3 ? (byte)SmsCpuFlags.F3 : 0) |
				(FlagHalf ? (byte)SmsCpuFlags.HalfCarry : 0) |
				(FlagF5 ? (byte)SmsCpuFlags.F5 : 0) |
				(FlagZero ? (byte)SmsCpuFlags.Zero : 0) |
				(FlagSign ? (byte)SmsCpuFlags.Sign : 0)
			);
		}

		protected override void InternalUpdateUiState()
		{
			SmsState state = DebugApi.GetConsoleState<SmsState>(ConsoleType.Sms);

			SmsCpuState cpu = state.Cpu;
			SmsVdpState ppu = DebugApi.GetPpuState<SmsVdpState>(CpuType.Sms);

			UpdateCycleCount(state.Cpu.CycleCount);

			RegA = cpu.A;
			RegB = cpu.B;
			RegC = cpu.C;
			RegD = cpu.D;
			RegE = cpu.E;
			RegFlags = cpu.Flags;

			RegH = cpu.H;
			RegL = cpu.L;

			RegIX = (UInt16)(cpu.IXL | (cpu.IXH << 8));
			RegIY = (UInt16)(cpu.IYL | (cpu.IYH << 8));

			RegPC = cpu.PC;
			RegSP = cpu.SP;

			RegR = cpu.R;
			RegI = cpu.I;

			RegAltA = cpu.AltA;
			RegAltB = cpu.AltB;
			RegAltC = cpu.AltC;
			RegAltD = cpu.AltD;
			RegAltE = cpu.AltE;
			RegAltFlags = cpu.AltFlags;

			RegAltH = cpu.AltH;
			RegAltL = cpu.AltL;

			FlagIFF1 = cpu.IFF1;
			FlagIFF2 = cpu.IFF2;
			FlagHalted = cpu.Halted;
			IM = cpu.IM;

			Scanline = ppu.Scanline;
			Cycle = ppu.Cycle;

			StringBuilder sb = new StringBuilder();
			for(UInt32 i = (UInt32)cpu.SP; (i & 0xFF) != 0; i++) {
				sb.Append($"${DebugApi.GetMemoryValue(MemoryType.SmsMemory, i):X2} ");
			}
			StackPreview = sb.ToString();
		}

		protected override void InternalUpdateConsoleState()
		{
			SmsCpuState cpu = DebugApi.GetCpuState<SmsCpuState>(CpuType.Sms);

			cpu.A = RegA;
			cpu.B = RegB;
			cpu.C = RegC;
			cpu.D = RegD;
			cpu.E = RegE;
			cpu.Flags = RegFlags;

			cpu.H = RegH;
			cpu.L = RegL;

			cpu.IXL = (byte)(RegIX & 0xFF);
			cpu.IXH = (byte)((RegIX >> 8) & 0xFF);
			cpu.IYL = (byte)(RegIY & 0xFF);
			cpu.IYH = (byte)((RegIY >> 8) & 0xFF);

			cpu.PC = RegPC;
			cpu.SP = RegSP;

			cpu.R = RegR;
			cpu.I = RegI;

			cpu.AltA = RegAltA;
			cpu.AltB = RegAltB;
			cpu.AltC = RegAltC;
			cpu.AltD = RegAltD;
			cpu.AltE = RegAltE;
			cpu.AltFlags = RegAltFlags;

			cpu.AltH = RegAltH;
			cpu.AltL = RegAltL;

			cpu.IFF1 = FlagIFF1;
			cpu.IFF2 = FlagIFF2;
			cpu.Halted = FlagHalted;
			cpu.IM = IM;

			DebugApi.SetCpuState(cpu, CpuType.Sms);
		}
	}
}
