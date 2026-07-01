using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.ComponentModel;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class GbaStatusViewModel : BaseConsoleStatusViewModel
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

		[ObservableProperty] public partial UInt32 RegCpsr { get; set; }

		[ObservableProperty] public partial GbaCpuMode Mode { get; set; }
		[ObservableProperty] public partial string ModeString { get; set; } = GbaCpuMode.User.ToString();

		[ObservableProperty] public partial bool FlagZero { get; set; }
		[ObservableProperty] public partial bool FlagCarry { get; set; }
		[ObservableProperty] public partial bool FlagNegative { get; set; }
		[ObservableProperty] public partial bool FlagOverflow { get; set; }

		[ObservableProperty] public partial bool FlagThumb { get; set; }
		[ObservableProperty] public partial bool FlagIrqDisable { get; set; }
		[ObservableProperty] public partial bool FlagFiqDisable { get; set; }

		[ObservableProperty] public partial string StackPreview { get; set; } = "";

		[ObservableProperty] public partial UInt16 Scanline { get; set; }
		[ObservableProperty] public partial UInt16 Cycle { get; set; }

		public GbaStatusViewModel()
		{
			this.ObserveProp([
				nameof(FlagZero),
				nameof(FlagCarry),
				nameof(FlagNegative),
				nameof(FlagOverflow),
				nameof(FlagThumb),
				nameof(FlagIrqDisable),
				nameof(FlagFiqDisable),
				nameof(Mode)
			], () => UpdateFlags());
		}

		private void UpdateFlags()
		{
			RegCpsr = (UInt32)(
				(FlagNegative ? (1 << 31) : 0) |
				(FlagZero ? (1 << 30) : 0) |
				(FlagCarry ? (1 << 29) : 0) |
				(FlagOverflow ? (1 << 28) : 0) |

				(FlagIrqDisable ? (1 << 7) : 0) |
				(FlagFiqDisable ? (1 << 6) : 0) |
				(FlagThumb ? (1 << 5) : 0) |
				(byte)Mode
			);
		}

		protected override void InternalUpdateUiState()
		{
			GbaCpuState cpu = DebugApi.GetCpuState<GbaCpuState>(CpuType.Gba);
			GbaPpuState ppu = DebugApi.GetPpuState<GbaPpuState>(CpuType.Gba);
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

			FlagCarry = cpu.CPSR.Carry;
			FlagZero = cpu.CPSR.Zero;
			FlagNegative = cpu.CPSR.Negative;
			FlagOverflow = cpu.CPSR.Overflow;
			FlagIrqDisable = cpu.CPSR.IrqDisable;
			FlagFiqDisable = cpu.CPSR.FiqDisable;
			FlagThumb = cpu.CPSR.Thumb;

			Mode = cpu.CPSR.Mode;
			if(cpu.CPSR.Mode == GbaCpuMode.Irq || cpu.CPSR.Mode == GbaCpuMode.Fiq) {
				ModeString = cpu.CPSR.Mode.ToString().ToUpper();
			} else {
				ModeString = cpu.CPSR.Mode.ToString();
			}

			StringBuilder sb = new StringBuilder();
			byte[] stackValues = DebugApi.GetMemoryValues(MemoryType.GbaMemory, cpu.R[13], cpu.R[13] + 30 * 4 - 1);
			for(int i = 0; i < stackValues.Length; i += 4) {
				UInt32 value = (uint)stackValues[i] | (uint)(stackValues[i + 1] << 8) | (uint)(stackValues[i + 2] << 16) | (uint)(stackValues[i + 3] << 24);
				sb.Append($"${value:X8} ");
			}
			StackPreview = sb.ToString();

			Scanline = ppu.Scanline;
			Cycle = ppu.Cycle;
		}

		protected override void InternalUpdateConsoleState()
		{
			GbaCpuState cpu = DebugApi.GetCpuState<GbaCpuState>(CpuType.Gba);

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

			cpu.CPSR.Carry = FlagCarry;
			cpu.CPSR.Zero = FlagZero;
			cpu.CPSR.Negative = FlagNegative;
			cpu.CPSR.Overflow = FlagOverflow;
			cpu.CPSR.IrqDisable = FlagIrqDisable;
			cpu.CPSR.FiqDisable = FlagFiqDisable;
			cpu.CPSR.Thumb = FlagThumb;

			DebugApi.SetCpuState(cpu, CpuType.Gba);
		}
	}
}
