using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class SpcStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial byte RegA { get; set; }
		[ObservableProperty] public partial byte RegX { get; set; }
		[ObservableProperty] public partial byte RegY { get; set; }
		[ObservableProperty] public partial byte RegSP { get; set; }
		[ObservableProperty] public partial UInt16 RegPC { get; set; }
		[ObservableProperty] public partial byte RegPS { get; set; }

		[ObservableProperty] public partial bool FlagN { get; set; }
		[ObservableProperty] public partial bool FlagV { get; set; }
		[ObservableProperty] public partial bool FlagP { get; set; }
		[ObservableProperty] public partial bool FlagB { get; set; }
		[ObservableProperty] public partial bool FlagH { get; set; }
		[ObservableProperty] public partial bool FlagI { get; set; }
		[ObservableProperty] public partial bool FlagZ { get; set; }
		[ObservableProperty] public partial bool FlagC { get; set; }

		[ObservableProperty] public partial string StackPreview { get; private set; } = "";

		public SpcStatusViewModel()
		{
			bool preventUpdate = false;

			this.ObserveProp([nameof(FlagC), nameof(FlagP), nameof(FlagB), nameof(FlagH), nameof(FlagI), nameof(FlagN), nameof(FlagV), nameof(FlagZ)], () => {
				if(!preventUpdate) {
					UpdatePsValue();
				}
			});

			this.ObserveProp(nameof(RegPS), () => {
				preventUpdate = true;
				FlagN = (RegPS & (byte)SpcFlags.Negative) != 0;
				FlagV = (RegPS & (byte)SpcFlags.Overflow) != 0;
				FlagP = (RegPS & (byte)SpcFlags.DirectPage) != 0;
				FlagB = (RegPS & (byte)SpcFlags.Break) != 0;
				FlagH = (RegPS & (byte)SpcFlags.HalfCarry) != 0;
				FlagI = (RegPS & (byte)SpcFlags.IrqEnable) != 0;
				FlagZ = (RegPS & (byte)SpcFlags.Zero) != 0;
				FlagC = (RegPS & (byte)SpcFlags.Carry) != 0;
				preventUpdate = false;
			});
		}

		private void UpdatePsValue()
		{
			RegPS = (byte)(
				(FlagN ? (byte)SpcFlags.Negative : 0) |
				(FlagV ? (byte)SpcFlags.Overflow : 0) |
				(FlagP ? (byte)SpcFlags.DirectPage : 0) |
				(FlagB ? (byte)SpcFlags.Break : 0) |
				(FlagH ? (byte)SpcFlags.HalfCarry : 0) |
				(FlagI ? (byte)SpcFlags.IrqEnable : 0) |
				(FlagZ ? (byte)SpcFlags.Zero : 0) |
				(FlagC ? (byte)SpcFlags.Carry : 0)
			);
		}

		protected override void InternalUpdateUiState()
		{
			SpcState cpu = DebugApi.GetCpuState<SpcState>(CpuType.Spc);

			UpdateCycleCount(cpu.Cycle);

			RegA = cpu.A;
			RegX = cpu.X;
			RegY = cpu.Y;
			RegSP = cpu.SP;
			RegPC = cpu.PC;
			RegPS = (byte)cpu.PS;

			StringBuilder sb = new StringBuilder();
			for(UInt32 i = (UInt32)0x100 + cpu.SP + 1; i < 0x200; i++) {
				sb.Append($"${DebugApi.GetMemoryValue(MemoryType.SpcMemory, i):X2} ");
			}
			StackPreview = sb.ToString();
		}

		protected override void InternalUpdateConsoleState()
		{
			SpcState cpu = DebugApi.GetCpuState<SpcState>(CpuType.Spc);

			cpu.A = RegA;
			cpu.X = RegX;
			cpu.Y = RegY;
			cpu.SP = RegSP;
			cpu.PC = RegPC;
			cpu.PS = (SpcFlags)RegPS;

			DebugApi.SetCpuState(cpu, CpuType.Spc);
		}
	}
}
