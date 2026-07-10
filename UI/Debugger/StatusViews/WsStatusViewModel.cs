using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Mesen.Debugger.StatusViews;

public partial class WsStatusViewModel : BaseConsoleStatusViewModel
{
	[ObservableProperty] public partial UInt16 RegAX { get; set; }
	[ObservableProperty] public partial UInt16 RegBX { get; set; }
	[ObservableProperty] public partial UInt16 RegCX { get; set; }
	[ObservableProperty] public partial UInt16 RegDX { get; set; }
	[ObservableProperty] public partial UInt16 RegFlags { get; set; }

	[ObservableProperty] public partial UInt16 RegSS { get; set; }
	[ObservableProperty] public partial UInt16 RegDS { get; set; }
	[ObservableProperty] public partial UInt16 RegES { get; set; }

	[ObservableProperty] public partial UInt16 RegCS { get; set; }
	[ObservableProperty] public partial UInt16 RegIP { get; set; }

	[ObservableProperty] public partial UInt16 RegDI { get; set; }
	[ObservableProperty] public partial UInt16 RegSI { get; set; }

	[ObservableProperty] public partial UInt16 RegSP { get; set; }
	[ObservableProperty] public partial UInt16 RegBP { get; set; }

	[ObservableProperty] public partial UInt16 Scanline { get; set; }
	[ObservableProperty] public partial UInt16 Cycle { get; set; }

	[ObservableProperty] public partial bool FlagCarry { get; set; }
	[ObservableProperty] public partial bool FlagAuxCarry { get; set; }
	[ObservableProperty] public partial bool FlagParity { get; set; }
	[ObservableProperty] public partial bool FlagSign { get; set; }
	[ObservableProperty] public partial bool FlagZero { get; set; }
	[ObservableProperty] public partial bool FlagOverflow { get; set; }
	[ObservableProperty] public partial bool FlagTrap { get; set; }
	[ObservableProperty] public partial bool FlagIrq { get; set; }
	[ObservableProperty] public partial bool FlagDirection { get; set; }
	[ObservableProperty] public partial bool FlagMode { get; set; }

	[ObservableProperty] public partial bool FlagHalted { get; set; }

	[ObservableProperty] public partial string StackPreview { get; private set; } = "";

	public WsStatusViewModel()
	{
		bool preventUpdate = false;

		this.ObserveProp([
			nameof(FlagZero), nameof(FlagCarry), nameof(FlagSign), nameof(FlagOverflow), nameof(FlagParity),
			nameof(FlagIrq), nameof(FlagTrap), nameof(FlagMode), nameof(FlagAuxCarry), nameof(FlagDirection)
		], () => {
			if(!preventUpdate) {
				UpdateFlags();
			}
		});

		this.ObserveProp(nameof(RegFlags), () => {
			preventUpdate = true;
			FlagCarry = (RegFlags & 0x01) != 0;
			FlagParity = (RegFlags & 0x04) != 0;
			FlagAuxCarry = (RegFlags & 0x10) != 0;
			FlagZero = (RegFlags & 0x40) != 0;
			FlagSign = (RegFlags & 0x80) != 0;
			FlagTrap = (RegFlags & 0x100) != 0;
			FlagIrq = (RegFlags & 0x200) != 0;
			FlagDirection = (RegFlags & 0x400) != 0;
			FlagOverflow = (RegFlags & 0x800) != 0;
			FlagMode = (RegFlags & 0x8000) != 0;
			preventUpdate = false;
		});
	}

	private void UpdateFlags()
	{
		RegFlags = (UInt16)(
			(FlagCarry ? 0x01 : 0) |
			(FlagParity ? 0x04 : 0) |
			(FlagAuxCarry ? 0x10 : 0) |
			(FlagZero ? 0x40 : 0) |
			(FlagSign ? 0x80 : 0) |
			(FlagTrap ? 0x100 : 0) |
			(FlagIrq ? 0x200 : 0) |
			(FlagDirection ? 0x400 : 0) |
			(FlagOverflow ? 0x800 : 0) |
			(FlagMode ? 0x8000 : 0) |
			0x7002
		);
	}

	protected override void InternalUpdateUiState()
	{
		WsCpuState cpu = DebugApi.GetCpuState<WsCpuState>(CpuType.Ws);
		WsPpuState ppu = DebugApi.GetPpuState<WsPpuState>(CpuType.Ws);

		UpdateCycleCount(cpu.CycleCount);

		RegAX = cpu.AX;
		RegBX = cpu.BX;
		RegCX = cpu.CX;
		RegDX = cpu.DX;

		FlagCarry = cpu.Flags.Carry;
		FlagAuxCarry = cpu.Flags.AuxCarry;
		FlagZero = cpu.Flags.Zero;
		FlagSign = cpu.Flags.Sign;
		FlagParity = cpu.Flags.Parity;
		FlagTrap = cpu.Flags.Trap;
		FlagIrq = cpu.Flags.Irq;
		FlagDirection = cpu.Flags.Direction;
		FlagOverflow = cpu.Flags.Overflow;
		FlagMode = cpu.Flags.Mode;

		RegSI = cpu.SI;
		RegDI = cpu.DI;

		RegIP = cpu.IP;
		RegCS = cpu.CS;

		RegSS = cpu.SS;
		RegDS = cpu.DS;
		RegES = cpu.ES;

		RegSP = cpu.SP;
		RegBP = cpu.BP;

		FlagHalted = cpu.Halted;

		Scanline = ppu.Scanline;
		Cycle = ppu.Cycle;

		StringBuilder sb = new StringBuilder();
		UInt32 stackAddr = (UInt32)((cpu.SS << 4) + cpu.SP);
		byte[] stackValues = DebugApi.GetMemoryValues(MemoryType.WsMemory, stackAddr, stackAddr + 30 * 4 - 1);
		for(int i = 0; i < stackValues.Length; i += 2) {
			UInt16 value = (UInt16)(stackValues[i] | (stackValues[i + 1] << 8));
			sb.Append($"${value:X4} ");
		}
		StackPreview = sb.ToString();
	}

	protected override void InternalUpdateConsoleState()
	{
		WsCpuState cpu = DebugApi.GetCpuState<WsCpuState>(CpuType.Ws);

		cpu.AX = RegAX;
		cpu.BX = RegBX;
		cpu.CX = RegCX;
		cpu.DX = RegDX;

		cpu.Flags.Carry = FlagCarry;
		cpu.Flags.AuxCarry = FlagAuxCarry;
		cpu.Flags.Zero = FlagZero;
		cpu.Flags.Sign = FlagSign;
		cpu.Flags.Parity = FlagParity;
		cpu.Flags.Trap = FlagTrap;
		cpu.Flags.Irq = FlagIrq;
		cpu.Flags.Direction = FlagDirection;
		cpu.Flags.Overflow = FlagOverflow;
		cpu.Flags.Mode = FlagMode;

		cpu.SI = RegSI;
		cpu.DI = RegDI;

		cpu.IP = RegIP;
		cpu.CS = RegCS;

		cpu.SS = RegSS;
		cpu.DS = RegDS;
		cpu.ES = RegES;

		cpu.SP = RegSP;
		cpu.BP = RegBP;

		cpu.Halted = FlagHalted;

		DebugApi.SetCpuState(cpu, CpuType.Ws);
	}
}
