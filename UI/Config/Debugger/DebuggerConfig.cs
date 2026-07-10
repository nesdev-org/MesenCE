using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger;
using Mesen.Interop;
using System;
using System.Collections.Generic;

namespace Mesen.Config
{
	public partial class DebuggerConfig : BaseWindowConfig<DebuggerConfig>
	{
		public DockEntryDefinition? SavedDockLayout { get; set; } = null;

		[ObservableProperty] public partial bool ShowSettingsPanel { get; set; } = true;

		[ObservableProperty] public partial bool ShowByteCode { get; set; } = false;
		[ObservableProperty] public partial bool ShowMemoryValues { get; set; } = true;
		[ObservableProperty] public partial bool UseLowerCaseDisassembly { get; set; } = false;

		[ObservableProperty] public partial bool ShowJumpLabels { get; set; } = false;
		[ObservableProperty] public partial AddressDisplayType AddressDisplayType { get; set; } = AddressDisplayType.CpuAddress;

		[ObservableProperty] public partial bool DrawPartialFrame { get; set; } = false;

		[ObservableProperty] public partial SnesDebuggerConfig Snes { get; set; } = new();
		[ObservableProperty] public partial NesDebuggerConfig Nes { get; set; } = new();
		[ObservableProperty] public partial GbDebuggerConfig Gameboy { get; set; } = new();
		[ObservableProperty] public partial GbaDebuggerConfig Gba { get; set; } = new();
		[ObservableProperty] public partial PceDebuggerConfig Pce { get; set; } = new();
		[ObservableProperty] public partial SmsDebuggerConfig Sms { get; set; } = new();
		[ObservableProperty] public partial WsDebuggerConfig Ws { get; set; } = new();

		[ObservableProperty] public partial bool BreakOnUninitRead { get; set; } = false;
		[ObservableProperty] public partial bool BreakOnOpen { get; set; } = true;
		[ObservableProperty] public partial bool BreakOnPowerCycleReset { get; set; } = true;

		[ObservableProperty] public partial bool AutoResetCdl { get; set; } = true;
		[ObservableProperty] public partial bool DisableDefaultLabels { get; set; } = false;

		[ObservableProperty] public partial bool UsePredictiveBreakpoints { get; set; } = true;
		[ObservableProperty] public partial bool SingleBreakpointPerInstruction { get; set; } = true;

		[ObservableProperty] public partial bool CopyAddresses { get; set; } = true;
		[ObservableProperty] public partial bool CopyByteCode { get; set; } = true;
		[ObservableProperty] public partial bool CopyComments { get; set; } = true;
		[ObservableProperty] public partial bool CopyBlockHeaders { get; set; } = true;

		[ObservableProperty] public partial bool KeepActiveStatementInCenter { get; set; } = false;

		[ObservableProperty] public partial bool ShowMemoryMappings { get; set; } = true;

		[ObservableProperty] public partial bool RefreshWhileRunning { get; set; } = false;

		[ObservableProperty] public partial bool BringToFrontOnBreak { get; set; } = true;
		[ObservableProperty] public partial bool BringToFrontOnPause { get; set; } = false;
		[ObservableProperty] public partial bool FocusGameOnResume { get; set; } = false;

		[ObservableProperty] public partial CodeDisplayMode UnidentifiedBlockDisplay { get; set; } = CodeDisplayMode.Hide;
		[ObservableProperty] public partial CodeDisplayMode VerifiedDataDisplay { get; set; } = CodeDisplayMode.Hide;

		[ObservableProperty] public partial int BreakOnValue { get; set; } = 0;
		[ObservableProperty] public partial int BreakInCount { get; set; } = 1;
		[ObservableProperty] public partial BreakInMetric BreakInMetric { get; set; } = BreakInMetric.CpuInstructions;

		[ObservableProperty] public partial bool ShowSelectionLength { get; set; } = false;
		[ObservableProperty] public partial WatchFormatStyle WatchFormat { get; set; } = WatchFormatStyle.Hex;

		[ObservableProperty] public partial UInt32 CodeOpcodeColor { get; set; } = Color.FromRgb(22, 37, 37).ToUInt32();
		[ObservableProperty] public partial UInt32 CodeLabelDefinitionColor { get; set; } = Colors.Blue.ToUInt32();
		[ObservableProperty] public partial UInt32 CodeImmediateColor { get; set; } = Colors.Chocolate.ToUInt32();
		[ObservableProperty] public partial UInt32 CodeAddressColor { get; set; } = Colors.DarkRed.ToUInt32();
		[ObservableProperty] public partial UInt32 CodeCommentColor { get; set; } = Colors.Green.ToUInt32();
		[ObservableProperty] public partial UInt32 CodeEffectiveAddressColor { get; set; } = Colors.SteelBlue.ToUInt32();

		[ObservableProperty] public partial UInt32 CodeVerifiedDataColor { get; set; } = Color.FromRgb(255, 252, 236).ToUInt32();
		[ObservableProperty] public partial UInt32 CodeUnidentifiedDataColor { get; set; } = Color.FromRgb(255, 242, 242).ToUInt32();
		[ObservableProperty] public partial UInt32 CodeUnexecutedCodeColor { get; set; } = Color.FromRgb(225, 244, 228).ToUInt32();

		[ObservableProperty] public partial UInt32 CodeExecBreakpointColor { get; set; } = Color.FromRgb(140, 40, 40).ToUInt32();
		[ObservableProperty] public partial UInt32 CodeWriteBreakpointColor { get; set; } = Color.FromRgb(40, 120, 80).ToUInt32();
		[ObservableProperty] public partial UInt32 CodeReadBreakpointColor { get; set; } = Color.FromRgb(40, 40, 200).ToUInt32();
		[ObservableProperty] public partial UInt32 ForbidBreakpointColor { get; set; } = Color.FromRgb(115, 115, 115).ToUInt32();

		[ObservableProperty] public partial UInt32 CodeActiveStatementColor { get; set; } = Colors.Yellow.ToUInt32();
		[ObservableProperty] public partial UInt32 CodeActiveMidInstructionColor { get; set; } = Color.FromRgb(255, 220, 40).ToUInt32();

		[ObservableProperty] public partial List<int> LabelListColumnWidths { get; set; } = new();
		[ObservableProperty] public partial List<int> FunctionListColumnWidths { get; set; } = new();
		[ObservableProperty] public partial List<int> BreakpointListColumnWidths { get; set; } = new();
		[ObservableProperty] public partial List<int> WatchListColumnWidths { get; set; } = new();
		[ObservableProperty] public partial List<int> CallStackColumnWidths { get; set; } = new();
		[ObservableProperty] public partial List<int> FindResultColumnWidths { get; set; } = new();

		public DebuggerConfig()
		{
		}
	}

	public partial class CfgColor : ObservableObject
	{
		[ObservableProperty] public partial UInt32 ColorCode { get; set; }
	}

	public enum BreakInMetric
	{
		CpuInstructions,
		PpuCycles,
		Scanlines,
		Frames
	}

	public enum CodeDisplayMode
	{
		Hide,
		Show,
		Disassemble
	}

	public enum AddressDisplayType
	{
		CpuAddress,
		AbsAddress,
		Both,
		BothCompact
	}
}
