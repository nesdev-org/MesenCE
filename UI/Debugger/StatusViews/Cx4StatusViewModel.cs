using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class Cx4StatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial UInt32 Reg0 { get; set; }
		[ObservableProperty] public partial UInt32 Reg1 { get; set; }
		[ObservableProperty] public partial UInt32 Reg2 { get; set; }
		[ObservableProperty] public partial UInt32 Reg3 { get; set; }
		[ObservableProperty] public partial UInt32 Reg4 { get; set; }
		[ObservableProperty] public partial UInt32 Reg5 { get; set; }
		[ObservableProperty] public partial UInt32 Reg6 { get; set; }
		[ObservableProperty] public partial UInt32 Reg7 { get; set; }
		[ObservableProperty] public partial UInt32 Reg8 { get; set; }
		[ObservableProperty] public partial UInt32 Reg9 { get; set; }
		[ObservableProperty] public partial UInt32 Reg10 { get; set; }
		[ObservableProperty] public partial UInt32 Reg11 { get; set; }
		[ObservableProperty] public partial UInt32 Reg12 { get; set; }
		[ObservableProperty] public partial UInt32 Reg13 { get; set; }
		[ObservableProperty] public partial UInt32 Reg14 { get; set; }
		[ObservableProperty] public partial UInt32 Reg15 { get; set; }

		[ObservableProperty] public partial UInt16 RegPb { get; set; }
		[ObservableProperty] public partial UInt16 RegP { get; set; }
		[ObservableProperty] public partial byte RegPc { get; set; }
		[ObservableProperty] public partial byte RegSp { get; set; }

		[ObservableProperty] public partial UInt32 RegMdr { get; set; }
		[ObservableProperty] public partial UInt32 RegMar { get; set; }
		[ObservableProperty] public partial UInt32 RegDpr { get; set; }

		[ObservableProperty] public partial UInt32 RegA { get; set; }
		[ObservableProperty] public partial UInt64 RegMult { get; set; }

		[ObservableProperty] public partial UInt32 RomBuffer { get; set; }
		[ObservableProperty] public partial UInt32 RamBuffer { get; set; }

		[ObservableProperty] public partial bool FlagZero { get; set; }
		[ObservableProperty] public partial bool FlagCarry { get; set; }
		[ObservableProperty] public partial bool FlagNegative { get; set; }
		[ObservableProperty] public partial bool FlagOverflow { get; set; }
		[ObservableProperty] public partial bool FlagIrq { get; set; }

		public Cx4StatusViewModel()
		{
		}

		protected override void InternalUpdateUiState()
		{
			Cx4State cpu = DebugApi.GetCpuState<Cx4State>(CpuType.Cx4);

			UpdateCycleCount(cpu.CycleCount);

			Reg0 = cpu.Regs[0];
			Reg1 = cpu.Regs[1];
			Reg2 = cpu.Regs[2];
			Reg3 = cpu.Regs[3];
			Reg4 = cpu.Regs[4];
			Reg5 = cpu.Regs[5];
			Reg6 = cpu.Regs[6];
			Reg7 = cpu.Regs[7];
			Reg8 = cpu.Regs[8];
			Reg9 = cpu.Regs[9];
			Reg10 = cpu.Regs[10];
			Reg11 = cpu.Regs[11];
			Reg12 = cpu.Regs[12];
			Reg13 = cpu.Regs[13];
			Reg14 = cpu.Regs[14];
			Reg15 = cpu.Regs[15];

			RegPb = cpu.PB;
			RegP = cpu.P;
			RegPc = cpu.PC;

			RegMdr = cpu.MemoryDataReg;
			RegMar = cpu.MemoryAddressReg;
			RegDpr = cpu.DataPointerReg;

			RegA = cpu.A;
			RegMult = cpu.Mult;

			RomBuffer = cpu.RomBuffer;
			RamBuffer = (UInt32)(cpu.RamBuffer[0] | (cpu.RamBuffer[1] << 8) | (cpu.RamBuffer[2] << 16));

			FlagCarry = cpu.Carry;
			FlagZero = cpu.Zero;
			FlagNegative = cpu.Negative;
			FlagOverflow = cpu.Overflow;
			FlagIrq = cpu.IrqFlag;
		}

		protected override void InternalUpdateConsoleState()
		{
			Cx4State cpu = DebugApi.GetCpuState<Cx4State>(CpuType.Cx4);

			cpu.Regs[0] = Reg0;
			cpu.Regs[1] = Reg1;
			cpu.Regs[2] = Reg2;
			cpu.Regs[3] = Reg3;
			cpu.Regs[4] = Reg4;
			cpu.Regs[5] = Reg5;
			cpu.Regs[6] = Reg6;
			cpu.Regs[7] = Reg7;
			cpu.Regs[8] = Reg8;
			cpu.Regs[9] = Reg9;
			cpu.Regs[10] = Reg10;
			cpu.Regs[11] = Reg11;
			cpu.Regs[12] = Reg12;
			cpu.Regs[13] = Reg13;
			cpu.Regs[14] = Reg14;
			cpu.Regs[15] = Reg15;

			cpu.PB = RegPb;
			cpu.P = RegP;
			cpu.PC = RegPc;

			cpu.MemoryDataReg = RegMdr;
			cpu.MemoryAddressReg = RegMar;
			cpu.DataPointerReg = RegDpr;

			cpu.A = RegA;
			cpu.Mult = RegMult;

			cpu.RomBuffer = RomBuffer;
			cpu.RamBuffer[0] = (byte)(RamBuffer & 0xFF);
			cpu.RamBuffer[1] = (byte)((RamBuffer >> 8) & 0xFF);
			cpu.RamBuffer[2] = (byte)((RamBuffer >> 16) & 0xFF);

			cpu.Carry = FlagCarry;
			cpu.Zero = FlagZero;
			cpu.Negative = FlagNegative;
			cpu.Overflow = FlagOverflow;
			cpu.IrqFlag = FlagIrq;

			DebugApi.SetCpuState(cpu, CpuType.Cx4);
		}
	}
}
