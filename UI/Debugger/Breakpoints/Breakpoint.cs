using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Debugger.Labels;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Text;

namespace Mesen.Debugger
{
	public partial class Breakpoint : ObservableObject
	{
		[ObservableProperty] public partial bool BreakOnRead { get; set; }
		[ObservableProperty] public partial bool BreakOnWrite { get; set; }
		[ObservableProperty] public partial bool BreakOnExec { get; set; }
		[ObservableProperty] public partial bool Forbid { get; set; }

		[ObservableProperty] public partial bool Enabled { get; set; } = true;
		[ObservableProperty] public partial bool MarkEvent { get; set; }
		[ObservableProperty] public partial bool IgnoreDummyOperations { get; set; } = true;
		[ObservableProperty] public partial MemoryType MemoryType { get; set; }
		[ObservableProperty] public partial UInt32 StartAddress { get; set; }
		[ObservableProperty] public partial UInt32 EndAddress { get; set; }
		[ObservableProperty] public partial CpuType CpuType { get; set; }
		[ObservableProperty] public partial bool AnyAddress { get; set; } = false;
		[ObservableProperty] public partial bool IsAssert { get; set; } = false;
		[ObservableProperty] public partial string Condition { get; set; } = "";

		public Breakpoint()
		{
		}

		public bool IsAbsoluteAddress { get { return !MemoryType.IsRelativeMemory(); } }
		public bool SupportsExec { get { return MemoryType.SupportsExecBreakpoints(); } }

		public bool IsSingleAddress { get { return StartAddress == EndAddress; } }
		public bool IsAddressRange { get { return StartAddress != EndAddress; } }

		public BreakpointTypeFlags Type
		{
			get
			{
				BreakpointTypeFlags type = BreakpointTypeFlags.None;
				if(BreakOnRead) {
					type |= BreakpointTypeFlags.Read;
				}
				if(BreakOnWrite) {
					type |= BreakpointTypeFlags.Write;
				}
				if(BreakOnExec && SupportsExec) {
					type |= BreakpointTypeFlags.Execute;
				}
				if(Forbid) {
					type = BreakpointTypeFlags.Forbid;
				}
				return type;
			}
		}

		public bool Matches(UInt32 address, MemoryType type, CpuType? cpuType)
		{
			if((cpuType.HasValue && cpuType.Value != CpuType)) {
				return false;
			}

			return type == MemoryType && address >= StartAddress && address <= EndAddress;
		}

		public int GetRelativeAddress()
		{
			if(SupportsExec && this.IsAbsoluteAddress) {
				return DebugApi.GetRelativeAddress(new AddressInfo() { Address = (int)StartAddress, Type = this.MemoryType }, this.CpuType).Address;
			} else {
				return (int)StartAddress;
			}
		}

		private int GetRelativeAddressEnd()
		{
			if(StartAddress != EndAddress) {
				if(SupportsExec && this.IsAbsoluteAddress) {
					return DebugApi.GetRelativeAddress(new AddressInfo() { Address = (int)this.EndAddress, Type = this.MemoryType }, this.CpuType).Address;
				} else {
					return (int)this.EndAddress;
				}
			}
			return -1;
		}

		public InteropBreakpoint ToInteropBreakpoint(int breakpointId)
		{
			InteropBreakpoint bp = new InteropBreakpoint() {
				Id = breakpointId,
				CpuType = CpuType,
				MemoryType = MemoryType,
				Type = Type,
				MarkEvent = MarkEvent,
				IgnoreDummyOperations = IgnoreDummyOperations,
				Enabled = Enabled,
				StartAddress = (Int32)StartAddress,
				EndAddress = (Int32)EndAddress
			};

			bp.Condition = new byte[1000];
			byte[] condition = Encoding.UTF8.GetBytes(Condition.Replace(Environment.NewLine, " ").Trim());
			Array.Copy(condition, bp.Condition, condition.Length);
			return bp;
		}

		public string GetAddressString(bool showLabel)
		{
			string addr = "";
			string format = MemoryType.GetFormatString();
			if(StartAddress == EndAddress) {
				addr += $"${StartAddress.ToString(format)}";
			} else {
				addr = $"${StartAddress.ToString(format)} - ${EndAddress.ToString(format)}";
			}

			if(showLabel) {
				string label = GetAddressLabel();
				if(!string.IsNullOrWhiteSpace(label)) {
					addr += " [" + label + "]";
				}
			}
			return addr;
		}

		public string GetAddressLabel()
		{
			if(MemoryType.SupportsLabels()) {
				CodeLabel? label = LabelManager.GetLabel(new AddressInfo() { Address = (int)StartAddress, Type = MemoryType });
				return label?.Label ?? string.Empty;
			}
			return string.Empty;
		}

		public string ToReadableType()
		{
			string type = MemoryType.GetShortName();
			type += ":";
			if(Forbid) {
				type += "🛇";
			} else {
				type += BreakOnRead ? "R" : "‒";
				type += BreakOnWrite ? "W" : "‒";
				if(SupportsExec) {
					type += BreakOnExec ? "X" : "‒";
				}
			}
			return type;
		}

		public Color GetColor()
		{
			DebuggerConfig config = ConfigManager.Config.Debug.Debugger;
			if(Forbid) {
				return Color.FromUInt32(config.ForbidBreakpointColor);
			}
			return Color.FromUInt32(BreakOnExec ? config.CodeExecBreakpointColor : (BreakOnWrite ? config.CodeWriteBreakpointColor : config.CodeReadBreakpointColor));
		}

		public Breakpoint Clone()
		{
			return JsonHelper.Clone(this);
		}

		public void CopyFrom(Breakpoint copy)
		{
			StartAddress = copy.StartAddress;
			EndAddress = copy.EndAddress;
			MemoryType = copy.MemoryType;
			MarkEvent = copy.MarkEvent;
			IgnoreDummyOperations = copy.IgnoreDummyOperations;
			Enabled = copy.Enabled;
			Condition = copy.Condition;
			BreakOnExec = copy.BreakOnExec;
			BreakOnRead = copy.BreakOnRead;
			BreakOnWrite = copy.BreakOnWrite;
			Forbid = copy.Forbid;
			CpuType = copy.CpuType;
		}
	}

	[Flags]
	public enum BreakpointTypeFlags
	{
		None = 0,
		Read = 1,
		Write = 2,
		Execute = 4,
		Forbid = 8,
	}
}
