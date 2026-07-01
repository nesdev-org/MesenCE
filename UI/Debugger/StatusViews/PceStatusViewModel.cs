using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class PceStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial byte RegA { get; set; }
		[ObservableProperty] public partial byte RegX { get; set; }
		[ObservableProperty] public partial byte RegY { get; set; }
		[ObservableProperty] public partial byte RegSP { get; set; }
		[ObservableProperty] public partial UInt16 RegPC { get; set; }
		[ObservableProperty] public partial byte RegPS { get; set; }

		[ObservableProperty] public partial bool FlagN { get; set; }
		[ObservableProperty] public partial bool FlagV { get; set; }
		[ObservableProperty] public partial bool FlagD { get; set; }
		[ObservableProperty] public partial bool FlagI { get; set; }
		[ObservableProperty] public partial bool FlagZ { get; set; }
		[ObservableProperty] public partial bool FlagC { get; set; }
		[ObservableProperty] public partial bool FlagT { get; set; }

		[ObservableProperty] public partial UInt16 Cycle { get; private set; }
		[ObservableProperty] public partial UInt16 Scanline { get; private set; }
		[ObservableProperty] public partial UInt32 FrameCount { get; private set; }

		[ObservableProperty] public partial string StackPreview { get; private set; } = "";

		public PceStatusViewModel()
		{
			bool preventUpdate = false;

			this.ObserveProp([nameof(FlagC), nameof(FlagD), nameof(FlagI), nameof(FlagN), nameof(FlagV), nameof(FlagZ), nameof(FlagT)], () => {
				if(!preventUpdate) {
					RegPS = (byte)(
						(FlagN ? (byte)PceCpuFlags.Negative : 0) |
						(FlagV ? (byte)PceCpuFlags.Overflow : 0) |
						(FlagT ? (byte)PceCpuFlags.Memory : 0) |
						(FlagD ? (byte)PceCpuFlags.Decimal : 0) |
						(FlagI ? (byte)PceCpuFlags.IrqDisable : 0) |
						(FlagZ ? (byte)PceCpuFlags.Zero : 0) |
						(FlagC ? (byte)PceCpuFlags.Carry : 0)
					);
				}
			});

			this.ObserveProp(nameof(RegPS), () => {
				preventUpdate = true;
				FlagN = (RegPS & (byte)PceCpuFlags.Negative) != 0;
				FlagV = (RegPS & (byte)PceCpuFlags.Overflow) != 0;
				FlagT = (RegPS & (byte)PceCpuFlags.Memory) != 0;
				FlagD = (RegPS & (byte)PceCpuFlags.Decimal) != 0;
				FlagI = (RegPS & (byte)PceCpuFlags.IrqDisable) != 0;
				FlagZ = (RegPS & (byte)PceCpuFlags.Zero) != 0;
				FlagC = (RegPS & (byte)PceCpuFlags.Carry) != 0;
				preventUpdate = false;
			});
		}

		protected override void InternalUpdateUiState()
		{
			PceCpuState cpu = DebugApi.GetCpuState<PceCpuState>(CpuType.Pce);
			PceVideoState video = DebugApi.GetPpuState<PceVideoState>(CpuType.Pce);

			UpdateCycleCount(cpu.CycleCount);

			RegA = cpu.A;
			RegX = cpu.X;
			RegY = cpu.Y;
			RegSP = cpu.SP;
			RegPC = cpu.PC;
			RegPS = cpu.PS;

			StringBuilder sb = new StringBuilder();
			for(UInt32 i = (UInt32)0x2100 + cpu.SP + 1; i < 0x2200; i++) {
				sb.Append($"${DebugApi.GetMemoryValue(MemoryType.PceMemory, i):X2} ");
			}
			StackPreview = sb.ToString();

			Cycle = video.Vdc.HClock;
			Scanline = video.Vdc.Scanline;
			FrameCount = video.Vdc.FrameCount;
		}

		protected override void InternalUpdateConsoleState()
		{
			PceCpuState cpu = DebugApi.GetCpuState<PceCpuState>(CpuType.Pce);

			cpu.A = RegA;
			cpu.X = RegX;
			cpu.Y = RegY;
			cpu.SP = RegSP;
			cpu.PC = RegPC;
			cpu.PS = RegPS;

			DebugApi.SetCpuState(cpu, CpuType.Pce);
		}
	}
}
