using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class NecDspStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial UInt16 RegTR { get; set; }
		[ObservableProperty] public partial UInt16 RegTRB { get; set; }
		[ObservableProperty] public partial UInt16 RegRP { get; set; }
		[ObservableProperty] public partial UInt16 RegDP { get; set; }
		[ObservableProperty] public partial UInt16 RegPC { get; set; }
		[ObservableProperty] public partial byte RegSP { get; set; }

		[ObservableProperty] public partial UInt16 RegDR { get; set; }
		[ObservableProperty] public partial UInt16 RegSR { get; set; }
		[ObservableProperty] public partial UInt16 RegK { get; set; }
		[ObservableProperty] public partial UInt16 RegL { get; set; }
		[ObservableProperty] public partial UInt16 RegM { get; set; }
		[ObservableProperty] public partial UInt16 RegN { get; set; }
		[ObservableProperty] public partial UInt16 RegA { get; set; }
		[ObservableProperty] public partial UInt16 RegB { get; set; }

		[ObservableProperty] public partial bool RegA_C { get; set; }
		[ObservableProperty] public partial bool RegA_Z { get; set; }
		[ObservableProperty] public partial bool RegA_V0 { get; set; }
		[ObservableProperty] public partial bool RegA_V1 { get; set; }
		[ObservableProperty] public partial bool RegA_S0 { get; set; }
		[ObservableProperty] public partial bool RegA_S1 { get; set; }

		[ObservableProperty] public partial bool RegB_C { get; set; }
		[ObservableProperty] public partial bool RegB_Z { get; set; }
		[ObservableProperty] public partial bool RegB_V0 { get; set; }
		[ObservableProperty] public partial bool RegB_V1 { get; set; }
		[ObservableProperty] public partial bool RegB_S0 { get; set; }
		[ObservableProperty] public partial bool RegB_S1 { get; set; }

		public NecDspStatusViewModel()
		{
		}

		protected override void InternalUpdateUiState()
		{
			NecDspState cpu = DebugApi.GetCpuState<NecDspState>(CpuType.NecDsp);

			UpdateCycleCount(cpu.CycleCount);

			RegTR = cpu.TR;
			RegTRB = cpu.TRB;
			RegPC = cpu.PC;
			RegSP = cpu.SP;

			RegRP = cpu.RP;
			RegDP = cpu.DP;
			RegDR = cpu.DR;
			RegSR = cpu.SR;

			RegK = cpu.K;
			RegL = cpu.L;
			RegM = cpu.M;
			RegN = cpu.N;

			RegA = cpu.A;
			RegA_C = cpu.FlagsA.Carry;
			RegA_Z = cpu.FlagsA.Zero;
			RegA_V0 = cpu.FlagsA.Overflow0;
			RegA_V1 = cpu.FlagsA.Overflow1;
			RegA_S0 = cpu.FlagsA.Sign0;
			RegA_S1 = cpu.FlagsA.Sign1;

			RegB = cpu.B;
			RegB_C = cpu.FlagsB.Carry;
			RegB_Z = cpu.FlagsB.Zero;
			RegB_V0 = cpu.FlagsB.Overflow0;
			RegB_V1 = cpu.FlagsB.Overflow1;
			RegB_S0 = cpu.FlagsB.Sign0;
			RegB_S1 = cpu.FlagsB.Sign1;
		}

		protected override void InternalUpdateConsoleState()
		{
			NecDspState cpu = DebugApi.GetCpuState<NecDspState>(CpuType.NecDsp);

			cpu.TR = RegTR;
			cpu.TRB = RegTRB;
			cpu.PC = RegPC;
			cpu.SP = RegSP;

			cpu.RP = RegRP;
			cpu.DP = RegDP;
			cpu.DR = RegDR;
			cpu.SR = RegSR;

			cpu.K = RegK;
			cpu.L = RegL;
			cpu.M = RegM;
			cpu.N = RegN;

			cpu.A = RegA;
			cpu.FlagsA.Carry = RegA_C;
			cpu.FlagsA.Zero = RegA_Z;
			cpu.FlagsA.Overflow0 = RegA_V0;
			cpu.FlagsA.Overflow1 = RegA_V1;
			cpu.FlagsA.Sign0 = RegA_S0;
			cpu.FlagsA.Sign1 = RegA_S1;

			cpu.B = RegB;
			cpu.FlagsB.Carry = RegB_C;
			cpu.FlagsB.Zero = RegB_Z;
			cpu.FlagsB.Overflow0 = RegB_V0;
			cpu.FlagsB.Overflow1 = RegB_V1;
			cpu.FlagsB.Sign0 = RegB_S0;
			cpu.FlagsB.Sign1 = RegB_S1;

			DebugApi.SetCpuState(cpu, CpuType.NecDsp);
		}
	}
}
