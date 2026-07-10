using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger.StatusViews
{
	public partial class NesStatusViewModel : BaseConsoleStatusViewModel
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

		[ObservableProperty] public partial bool FlagNmi { get; set; }

		[ObservableProperty] public partial bool FlagIrqExternal { get; set; }
		[ObservableProperty] public partial bool FlagIrqFrameCount { get; set; }
		[ObservableProperty] public partial bool FlagIrqDmc { get; set; }
		[ObservableProperty] public partial bool FlagIrqFdsDisk { get; set; }

		[ObservableProperty] public partial uint Cycle { get; private set; }
		[ObservableProperty] public partial int Scanline { get; private set; }
		[ObservableProperty] public partial UInt32 FrameCount { get; private set; }
		[ObservableProperty] public partial UInt16 VramAddr { get; set; }
		[ObservableProperty] public partial UInt16 TmpVramAddr { get; set; }
		[ObservableProperty] public partial UInt16 BusAddr { get; set; }
		[ObservableProperty] public partial byte ScrollX { get; set; }
		[ObservableProperty] public partial bool Sprite0Hit { get; set; }
		[ObservableProperty] public partial bool SpriteOverflow { get; set; }
		[ObservableProperty] public partial bool VerticalBlank { get; set; }
		[ObservableProperty] public partial bool WriteToggle { get; set; }

		//Mask
		[ObservableProperty] public partial bool BgEnabled { get; set; }
		[ObservableProperty] public partial bool SpritesEnabled { get; set; }
		[ObservableProperty] public partial bool BgMaskLeft { get; set; }
		[ObservableProperty] public partial bool SpriteMaskLeft { get; set; }
		[ObservableProperty] public partial bool Grayscale { get; set; }
		[ObservableProperty] public partial bool IntensifyRed { get; set; }
		[ObservableProperty] public partial bool IntensifyGreen { get; set; }
		[ObservableProperty] public partial bool IntensifyBlue { get; set; }

		//Control
		[ObservableProperty] public partial bool LargeSprites { get; set; }
		[ObservableProperty] public partial bool NmiOnVBlank { get; set; }
		[ObservableProperty] public partial bool VerticalWrite { get; set; }
		[ObservableProperty] public partial bool BgAt1000 { get; set; }
		[ObservableProperty] public partial bool SpritesAt1000 { get; set; }

		[ObservableProperty] public partial string StackPreview { get; private set; } = "";

		public NesStatusViewModel()
		{
			bool preventUpdate = false;

			this.ObserveProp([nameof(FlagC), nameof(FlagD), nameof(FlagI), nameof(FlagN), nameof(FlagV), nameof(FlagZ)], () => {
				if(!preventUpdate) {
					RegPS = (byte)(
						(FlagN ? (byte)NesCpuFlags.Negative : 0) |
						(FlagV ? (byte)NesCpuFlags.Overflow : 0) |
						(FlagD ? (byte)NesCpuFlags.Decimal : 0) |
						(FlagI ? (byte)NesCpuFlags.IrqDisable : 0) |
						(FlagZ ? (byte)NesCpuFlags.Zero : 0) |
						(FlagC ? (byte)NesCpuFlags.Carry : 0)
					);
				}
			});

			this.ObserveProp(nameof(RegPS), () => {
				preventUpdate = true;
				FlagN = (RegPS & (byte)NesCpuFlags.Negative) != 0;
				FlagV = (RegPS & (byte)NesCpuFlags.Overflow) != 0;
				FlagD = (RegPS & (byte)NesCpuFlags.Decimal) != 0;
				FlagI = (RegPS & (byte)NesCpuFlags.IrqDisable) != 0;
				FlagZ = (RegPS & (byte)NesCpuFlags.Zero) != 0;
				FlagC = (RegPS & (byte)NesCpuFlags.Carry) != 0;
				preventUpdate = false;
			});
		}

		protected override void InternalUpdateUiState()
		{
			NesCpuState cpu = DebugApi.GetCpuState<NesCpuState>(CpuType.Nes);
			NesPpuState ppu = DebugApi.GetPpuState<NesPpuState>(CpuType.Nes);

			UpdateCycleCount(cpu.CycleCount);

			RegA = cpu.A;
			RegX = cpu.X;
			RegY = cpu.Y;
			RegSP = cpu.SP;
			RegPC = cpu.PC;
			RegPS = cpu.PS;

			FlagNmi = cpu.NMIFlag;
			FlagIrqExternal = (cpu.IRQFlag & (byte)NesIrqSources.External) != 0;
			FlagIrqFrameCount = (cpu.IRQFlag & (byte)NesIrqSources.FrameCounter) != 0;
			FlagIrqDmc = (cpu.IRQFlag & (byte)NesIrqSources.DMC) != 0;
			FlagIrqFdsDisk = (cpu.IRQFlag & (byte)NesIrqSources.FdsDisk) != 0;

			StringBuilder sb = new StringBuilder();
			for(UInt32 i = (UInt32)0x100 + cpu.SP + 1; i < 0x200; i++) {
				sb.Append($"${DebugApi.GetMemoryValue(MemoryType.NesMemory, i):X2} ");
			}
			StackPreview = sb.ToString();

			Cycle = ppu.Cycle;
			Scanline = ppu.Scanline;
			FrameCount = ppu.FrameCount;
			VramAddr = ppu.VideoRamAddr;
			TmpVramAddr = ppu.TmpVideoRamAddr;
			ScrollX = ppu.ScrollX;
			BusAddr = ppu.BusAddress;

			Sprite0Hit = ppu.StatusFlags.Sprite0Hit;
			SpriteOverflow = ppu.StatusFlags.SpriteOverflow;
			VerticalBlank = ppu.StatusFlags.VerticalBlank;
			WriteToggle = ppu.WriteToggle;

			LargeSprites = ppu.Control.LargeSprites;
			NmiOnVBlank = ppu.Control.NmiOnVerticalBlank;
			VerticalWrite = ppu.Control.VerticalWrite;
			BgAt1000 = ppu.Control.BackgroundPatternAddr == 0x1000;
			SpritesAt1000 = ppu.Control.SpritePatternAddr == 0x1000;

			BgEnabled = ppu.Mask.BackgroundEnabled;
			SpritesEnabled = ppu.Mask.SpritesEnabled;
			BgMaskLeft = ppu.Mask.BackgroundMask;
			SpriteMaskLeft = ppu.Mask.SpriteMask;
			Grayscale = ppu.Mask.Grayscale;
			IntensifyRed = ppu.Mask.IntensifyRed;
			IntensifyGreen = ppu.Mask.IntensifyGreen;
			IntensifyBlue = ppu.Mask.IntensifyBlue;
		}

		protected override void InternalUpdateConsoleState()
		{
			NesCpuState cpu = DebugApi.GetCpuState<NesCpuState>(CpuType.Nes);
			NesPpuState ppu = DebugApi.GetPpuState<NesPpuState>(CpuType.Nes);

			cpu.A = RegA;
			cpu.X = RegX;
			cpu.Y = RegY;
			cpu.SP = RegSP;
			cpu.PC = RegPC;
			cpu.PS = RegPS;

			cpu.NMIFlag = FlagNmi;

			cpu.IRQFlag = (byte)(
				(FlagIrqExternal ? NesIrqSources.External : 0) |
				(FlagIrqFrameCount ? NesIrqSources.FrameCounter : 0) |
				(FlagIrqDmc ? NesIrqSources.DMC : 0) |
				(FlagIrqFdsDisk ? NesIrqSources.FdsDisk : 0)
			);

			ppu.Cycle = Cycle;
			ppu.Scanline = Scanline;
			ppu.VideoRamAddr = VramAddr;
			ppu.TmpVideoRamAddr = TmpVramAddr;
			ppu.ScrollX = ScrollX;
			ppu.BusAddress = BusAddr;

			ppu.StatusFlags.Sprite0Hit = Sprite0Hit;
			ppu.StatusFlags.SpriteOverflow = SpriteOverflow;
			ppu.StatusFlags.VerticalBlank = VerticalBlank;
			ppu.WriteToggle = WriteToggle;

			ppu.Control.LargeSprites = LargeSprites;
			ppu.Control.NmiOnVerticalBlank = NmiOnVBlank;
			ppu.Control.VerticalWrite = VerticalWrite;
			ppu.Control.BackgroundPatternAddr = (UInt16)(BgAt1000 ? 0x1000 : 0);
			ppu.Control.SpritePatternAddr = (UInt16)(SpritesAt1000 ? 0x1000 : 0);

			ppu.Mask.BackgroundEnabled = BgEnabled;
			ppu.Mask.SpritesEnabled = SpritesEnabled;
			ppu.Mask.BackgroundMask = BgMaskLeft;
			ppu.Mask.SpriteMask = SpriteMaskLeft;
			ppu.Mask.Grayscale = Grayscale;
			ppu.Mask.IntensifyRed = IntensifyRed;
			ppu.Mask.IntensifyGreen = IntensifyGreen;
			ppu.Mask.IntensifyBlue = IntensifyBlue;

			DebugApi.SetCpuState(cpu, CpuType.Nes);
			DebugApi.SetPpuState(ppu, CpuType.Nes);
		}
	}
}
