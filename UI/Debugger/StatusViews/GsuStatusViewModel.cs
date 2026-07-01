using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class GsuStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial UInt16 Reg0 { get; set; }
		[ObservableProperty] public partial UInt16 Reg1 { get; set; }
		[ObservableProperty] public partial UInt16 Reg2 { get; set; }
		[ObservableProperty] public partial UInt16 Reg3 { get; set; }
		[ObservableProperty] public partial UInt16 Reg4 { get; set; }
		[ObservableProperty] public partial UInt16 Reg5 { get; set; }
		[ObservableProperty] public partial UInt16 Reg6 { get; set; }
		[ObservableProperty] public partial UInt16 Reg7 { get; set; }
		[ObservableProperty] public partial UInt16 Reg8 { get; set; }
		[ObservableProperty] public partial UInt16 Reg9 { get; set; }
		[ObservableProperty] public partial UInt16 Reg10 { get; set; }
		[ObservableProperty] public partial UInt16 Reg11 { get; set; }
		[ObservableProperty] public partial UInt16 Reg12 { get; set; }
		[ObservableProperty] public partial UInt16 Reg13 { get; set; }
		[ObservableProperty] public partial UInt16 Reg14 { get; set; }
		[ObservableProperty] public partial UInt16 Reg15 { get; set; }

		[ObservableProperty] public partial UInt16 RegSfr { get; set; }
		[ObservableProperty] public partial UInt16 RamAddrCache { get; set; }

		[ObservableProperty] public partial byte RegSrc { get; set; }
		[ObservableProperty] public partial byte RegDst { get; set; }
		[ObservableProperty] public partial byte RegColor { get; set; }
		[ObservableProperty] public partial byte RegPor { get; set; }

		[ObservableProperty] public partial byte RegPbr { get; set; }
		[ObservableProperty] public partial byte RomBank { get; set; }
		[ObservableProperty] public partial byte RamBank { get; set; }

		[ObservableProperty] public partial bool FlagZero { get; set; }
		[ObservableProperty] public partial bool FlagCarry { get; set; }
		[ObservableProperty] public partial bool FlagSign { get; set; }
		[ObservableProperty] public partial bool FlagOverflow { get; set; }

		[ObservableProperty] public partial bool FlagAlt1 { get; set; }
		[ObservableProperty] public partial bool FlagAlt2 { get; set; }
		[ObservableProperty] public partial bool FlagIrq { get; set; }
		[ObservableProperty] public partial bool FlagRomReadPending { get; set; }

		[ObservableProperty] public partial bool FlagRunning { get; set; }
		[ObservableProperty] public partial bool FlagImmLow { get; set; }
		[ObservableProperty] public partial bool FlagImmHigh { get; set; }
		[ObservableProperty] public partial bool FlagPrefix { get; set; }

		[ObservableProperty] public partial bool FlagPlotTransparent { get; set; }
		[ObservableProperty] public partial bool FlagPlotDither { get; set; }
		[ObservableProperty] public partial bool FlagColorHighNibble { get; set; }
		[ObservableProperty] public partial bool FlagColorFreezeHigh { get; set; }
		[ObservableProperty] public partial bool FlagObjMode { get; set; }

		public GsuStatusViewModel()
		{
			this.ObserveProp([
				nameof(FlagZero), nameof(FlagCarry), nameof(FlagSign), nameof(FlagOverflow),
				nameof(FlagAlt1), nameof(FlagAlt2), nameof(FlagIrq), nameof(FlagRomReadPending),
				nameof(FlagRunning), nameof(FlagImmLow), nameof(FlagImmHigh), nameof(FlagPrefix)
			], UpdateSfrValue);

			this.ObserveProp([
				nameof(FlagPlotTransparent), nameof(FlagPlotDither), nameof(FlagColorHighNibble),
				nameof(FlagColorFreezeHigh), nameof(FlagObjMode)
			], UpdatePorValue);
		}

		private void UpdateSfrValue()
		{
			RegSfr = (UInt16)(
				(FlagZero ? 0x02 : 0) |
				(FlagCarry ? 0x04 : 0) |
				(FlagSign ? 0x08 : 0) |
				(FlagOverflow ? 0x10 : 0) |
				(FlagRunning ? 0x20 : 0) |
				(FlagRomReadPending ? 0x40 : 0) |
				(FlagAlt1 ? 0x100 : 0) |
				(FlagAlt2 ? 0x200 : 0) |
				(FlagImmLow ? 0x400 : 0) |
				(FlagImmHigh ? 0x800 : 0) |
				(FlagPrefix ? 0x1000 : 0) |
				(FlagIrq ? 0x8000 : 0)
			);
		}

		private void UpdatePorValue()
		{
			RegPor = (byte)(
				(FlagPlotTransparent ? 0x01 : 0) |
				(FlagPlotDither ? 0x02 : 0) |
				(FlagColorHighNibble ? 0x04 : 0) |
				(FlagColorFreezeHigh ? 0x08 : 0) |
				(FlagObjMode ? 0x10 : 0)
			);
		}

		protected override void InternalUpdateUiState()
		{
			GsuState cpu = DebugApi.GetCpuState<GsuState>(CpuType.Gsu);

			UpdateCycleCount(cpu.CycleCount);

			Reg0 = cpu.R[0];
			Reg1 = cpu.R[1];
			Reg2 = cpu.R[2];
			Reg3 = cpu.R[3];
			Reg4 = cpu.R[4];
			Reg5 = cpu.R[5];
			Reg6 = cpu.R[6];
			Reg7 = cpu.R[7];
			Reg8 = cpu.R[8];
			Reg9 = cpu.R[9];
			Reg10 = cpu.R[10];
			Reg11 = cpu.R[11];
			Reg12 = cpu.R[12];
			Reg13 = cpu.R[13];
			Reg14 = cpu.R[14];
			Reg15 = cpu.R[15];

			RegSrc = cpu.SrcReg;
			RegDst = cpu.DestReg;
			RegColor = cpu.ColorReg;
			RegPbr = cpu.ProgramBank;
			RomBank = cpu.RomBank;
			RamBank = cpu.RamBank;
			RamAddrCache = cpu.RamAddress;

			FlagCarry = cpu.SFR.Carry;
			FlagZero = cpu.SFR.Zero;
			FlagSign = cpu.SFR.Sign;
			FlagOverflow = cpu.SFR.Overflow;
			FlagRunning = cpu.SFR.Running;
			FlagRomReadPending = cpu.SFR.RomReadPending;
			FlagAlt1 = cpu.SFR.Alt1;
			FlagAlt2 = cpu.SFR.Alt2;
			FlagImmLow = cpu.SFR.ImmLow;
			FlagImmHigh = cpu.SFR.ImmHigh;
			FlagPrefix = cpu.SFR.Prefix;
			FlagIrq = cpu.SFR.Irq;

			FlagPlotTransparent = cpu.PlotTransparent;
			FlagPlotDither = cpu.PlotDither;
			FlagColorHighNibble = cpu.ColorHighNibble;
			FlagColorFreezeHigh = cpu.ColorFreezeHigh;
			FlagObjMode = cpu.ObjMode;
		}

		protected override void InternalUpdateConsoleState()
		{
			GsuState cpu = DebugApi.GetCpuState<GsuState>(CpuType.Gsu);

			cpu.R[0] = Reg0;
			cpu.R[1] = Reg1;
			cpu.R[2] = Reg2;
			cpu.R[3] = Reg3;
			cpu.R[4] = Reg4;
			cpu.R[5] = Reg5;
			cpu.R[6] = Reg6;
			cpu.R[7] = Reg7;
			cpu.R[8] = Reg8;
			cpu.R[9] = Reg9;
			cpu.R[10] = Reg10;
			cpu.R[11] = Reg11;
			cpu.R[12] = Reg12;
			cpu.R[13] = Reg13;
			cpu.R[14] = Reg14;
			cpu.R[15] = Reg15;

			cpu.SrcReg = RegSrc;
			cpu.DestReg = RegDst;
			cpu.ColorReg = RegColor;
			cpu.ProgramBank = RegPbr;
			cpu.RomBank = RomBank;
			cpu.RamBank = RamBank;
			cpu.RamAddress = RamAddrCache;

			cpu.SFR.Carry = FlagCarry;
			cpu.SFR.Zero = FlagZero;
			cpu.SFR.Sign = FlagSign;
			cpu.SFR.Overflow = FlagOverflow;
			cpu.SFR.Running = FlagRunning;
			cpu.SFR.RomReadPending = FlagRomReadPending;
			cpu.SFR.Alt1 = FlagAlt1;
			cpu.SFR.Alt2 = FlagAlt2;
			cpu.SFR.ImmLow = FlagImmLow;
			cpu.SFR.ImmHigh = FlagImmHigh;
			cpu.SFR.Prefix = FlagPrefix;
			cpu.SFR.Irq = FlagIrq;

			cpu.PlotTransparent = FlagPlotTransparent;
			cpu.PlotDither = FlagPlotDither;
			cpu.ColorHighNibble = FlagColorHighNibble;
			cpu.ColorFreezeHigh = FlagColorFreezeHigh;
			cpu.ObjMode = FlagObjMode;

			DebugApi.SetCpuState(cpu, CpuType.Gsu);
		}
	}
}
