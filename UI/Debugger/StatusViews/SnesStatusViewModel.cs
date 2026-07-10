using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class SnesStatusViewModel : BaseConsoleStatusViewModel
	{
		[ObservableProperty] public partial UInt16 RegA { get; set; }
		[ObservableProperty] public partial UInt16 RegX { get; set; }
		[ObservableProperty] public partial UInt16 RegY { get; set; }
		[ObservableProperty] public partial UInt16 RegSP { get; set; }
		[ObservableProperty] public partial UInt16 RegD { get; set; }
		[ObservableProperty] public partial UInt32 RegPC { get; set; }
		[ObservableProperty] public partial byte RegDBR { get; set; }
		[ObservableProperty] public partial byte RegPS { get; set; }

		[ObservableProperty] public partial bool FlagN { get; set; }
		[ObservableProperty] public partial bool FlagV { get; set; }
		[ObservableProperty] public partial bool FlagM { get; set; }
		[ObservableProperty] public partial bool FlagX { get; set; }
		[ObservableProperty] public partial bool FlagD { get; set; }
		[ObservableProperty] public partial bool FlagI { get; set; }
		[ObservableProperty] public partial bool FlagZ { get; set; }
		[ObservableProperty] public partial bool FlagC { get; set; }

		[ObservableProperty] public partial bool FlagE { get; set; }

		[ObservableProperty] public partial bool FlagNmi { get; set; }
		[ObservableProperty] public partial bool FlagIrqHvCounters { get; set; }
		[ObservableProperty] public partial bool FlagIrqCoprocessor { get; set; }

		[ObservableProperty] public partial int Cycle { get; private set; }
		[ObservableProperty] public partial int Scanline { get; private set; }
		[ObservableProperty] public partial int HClock { get; private set; }

		[ObservableProperty] public partial int VramAddress { get; private set; }
		[ObservableProperty] public partial int OamAddress { get; private set; }
		[ObservableProperty] public partial int CgRamAddress { get; private set; }

		[ObservableProperty] public partial string StackPreview { get; set; } = "";

		private CpuType _cpuType;

		[Obsolete("For designer only")]
		public SnesStatusViewModel() : this(CpuType.Snes) { }

		public SnesStatusViewModel(CpuType cpuType)
		{
			_cpuType = cpuType;

			bool preventUpdate = false;

			this.ObserveProp([nameof(FlagC), nameof(FlagD), nameof(FlagI), nameof(FlagN), nameof(FlagV), nameof(FlagZ), nameof(FlagM), nameof(FlagX)], () => {
				if(!preventUpdate) {
					UpdatePsValue();
				}
			});

			this.ObserveProp(nameof(RegPS), () => {
				preventUpdate = true;
				FlagN = (RegPS & (byte)SnesCpuFlags.Negative) != 0;
				FlagV = (RegPS & (byte)SnesCpuFlags.Overflow) != 0;
				FlagD = (RegPS & (byte)SnesCpuFlags.Decimal) != 0;
				FlagI = (RegPS & (byte)SnesCpuFlags.IrqDisable) != 0;
				FlagZ = (RegPS & (byte)SnesCpuFlags.Zero) != 0;
				FlagC = (RegPS & (byte)SnesCpuFlags.Carry) != 0;
				FlagX = (RegPS & (byte)SnesCpuFlags.IndexMode8) != 0;
				FlagM = (RegPS & (byte)SnesCpuFlags.MemoryMode8) != 0;
				preventUpdate = false;
			});
		}

		private void UpdatePsValue()
		{
			RegPS = (byte)(
				(FlagN ? (byte)SnesCpuFlags.Negative : 0) |
				(FlagV ? (byte)SnesCpuFlags.Overflow : 0) |
				(FlagD ? (byte)SnesCpuFlags.Decimal : 0) |
				(FlagI ? (byte)SnesCpuFlags.IrqDisable : 0) |
				(FlagZ ? (byte)SnesCpuFlags.Zero : 0) |
				(FlagC ? (byte)SnesCpuFlags.Carry : 0) |
				(FlagX ? (byte)SnesCpuFlags.IndexMode8 : 0) |
				(FlagM ? (byte)SnesCpuFlags.MemoryMode8 : 0)
			);
		}

		protected override void InternalUpdateUiState()
		{
			SnesCpuState cpu = DebugApi.GetCpuState<SnesCpuState>(_cpuType);
			SnesPpuState ppu = DebugApi.GetPpuState<SnesPpuState>(CpuType.Snes);

			UpdateCycleCount(cpu.CycleCount);

			RegA = cpu.A;
			RegX = cpu.X;
			RegY = cpu.Y;
			RegSP = cpu.SP;
			RegPC = (UInt32)((cpu.K << 16) | cpu.PC);

			RegD = cpu.D;
			RegDBR = cpu.DBR;

			RegPS = (byte)cpu.PS;

			FlagE = cpu.EmulationMode;
			FlagNmi = cpu.NmiFlagCounter > 0 || cpu.NeedNmi;
			FlagIrqHvCounters = (cpu.IrqSource & (byte)SnesIrqSource.Ppu) != 0;
			FlagIrqCoprocessor = (cpu.IrqSource & (byte)SnesIrqSource.Coprocessor) != 0;

			StringBuilder sb = new StringBuilder();
			MemoryType memType = _cpuType.ToMemoryType();
			for(UInt32 i = (uint)cpu.SP + 1; (i & 0xFF) != 0; i++) {
				sb.Append($"${DebugApi.GetMemoryValue(memType, i):X2} ");
			}
			StackPreview = sb.ToString();

			Cycle = ppu.Cycle;
			HClock = ppu.HClock;
			Scanline = ppu.Scanline;

			VramAddress = ppu.VramAddress;
			OamAddress = ppu.InternalOamRamAddress;
			CgRamAddress = ppu.CgramAddress;
		}

		protected override void InternalUpdateConsoleState()
		{
			SnesCpuState cpu = DebugApi.GetCpuState<SnesCpuState>(_cpuType);

			cpu.A = RegA;
			cpu.X = RegX;
			cpu.Y = RegY;
			cpu.SP = RegSP;
			cpu.K = (byte)((RegPC >> 16) & 0xFF);
			cpu.PC = (UInt16)(RegPC & 0xFFFF);

			cpu.D = RegD;
			cpu.DBR = RegDBR;

			cpu.PS = (SnesCpuFlags)RegPS;

			cpu.EmulationMode = FlagE;
			if(FlagNmi && cpu.NmiFlagCounter == 0 && !cpu.NeedNmi) {
				cpu.NmiFlagCounter = (byte)(FlagNmi ? 1 : 0);
			}
			cpu.IrqSource = (byte)(
				(FlagIrqHvCounters ? (byte)SnesIrqSource.Ppu : 0) |
				(FlagIrqCoprocessor ? (byte)SnesIrqSource.Coprocessor : 0)
			);

			DebugApi.SetCpuState(cpu, _cpuType);
		}
	}
}
