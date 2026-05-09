using Mesen.Interop;
using Mesen.Localization;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mesen.Config
{
	public class WsControllerConfig : ControllerConfig
	{
		public new WsKeyMapping Mapping1 { get => (WsKeyMapping)_mapping1; set => _mapping1 = value; }
		public new WsKeyMapping Mapping2 { get => (WsKeyMapping)_mapping2; set => _mapping2 = value; }
		public new WsKeyMapping Mapping3 { get => (WsKeyMapping)_mapping3; set => _mapping3 = value; }
		public new WsKeyMapping Mapping4 { get => (WsKeyMapping)_mapping4; set => _mapping4 = value; }

		public WsControllerConfig()
		{
			_mapping1 = new WsKeyMapping();
			_mapping2 = new WsKeyMapping();
			_mapping3 = new WsKeyMapping();
			_mapping4 = new WsKeyMapping();
		}
	}

	public class WsKeyMapping : KeyMapping
	{
		public UInt16[]? Pcv2Buttons { get; set; } = null;

		protected override UInt16[]? GetCustomButtons(ControllerType type)
		{
			return type switch {
				ControllerType.Pcv2Controller => Pcv2Buttons,
				_ => null
			};
		}

		public override List<CustomKeyMapping> ToCustomKeys(ControllerType type, int mappingIndex)
		{
			UInt16[]? buttonMappings = GetCustomButtons(type);
			if(buttonMappings == null) {
				if(GetDefaultCustomKeys(type, null) != null) {
					if(mappingIndex == 0) {
						SetDefaultKeys(type, null);
					} else {
						ClearKeys(type);
					}
				}

				buttonMappings = GetCustomButtons(type);
				if(buttonMappings == null) {
					return new List<CustomKeyMapping>();
				}
			}

			List<CustomKeyMapping> keys = type switch {
				ControllerType.Pcv2Controller => Enum.GetValues<Pcv2Buttons>().Select(val => new CustomKeyMapping(ResourceHelper.GetEnumText(val), buttonMappings, (int)val)).ToList(),
				_ => new()
			};

			keys.Sort((a, b) => a.Name.CompareTo(b.Name));

			return keys;
		}

		public override void ClearKeys(ControllerType type)
		{
			switch(type) {
				case ControllerType.Pcv2Controller:
					Pcv2Buttons = new UInt16[9];
					break;

				case ControllerType.WsController:
				case ControllerType.WsControllerVertical:
					base.ClearKeys(type);
					break;
			}
		}

		public override UInt16[]? GetDefaultCustomKeys(ControllerType type, KeyPresetType? preset)
		{
			switch(type) {
				case ControllerType.Pcv2Controller:
					return new UInt16[9] {
						InputApi.GetKeyCode("Up Arrow"),
						InputApi.GetKeyCode("Down Arrow"),
						InputApi.GetKeyCode("Left Arrow"),
						InputApi.GetKeyCode("Right Arrow"),
						InputApi.GetKeyCode("S"),
						InputApi.GetKeyCode("Z"),
						InputApi.GetKeyCode("X"),
						InputApi.GetKeyCode("C"),
						InputApi.GetKeyCode("A")
					};

				default:
					return null;
			}
		}

		public override void SetDefaultKeys(ControllerType type, KeyPresetType? preset)
		{
			switch(type) {
				case ControllerType.Pcv2Controller: Pcv2Buttons = GetDefaultCustomKeys(type, preset); break;

				default:
					base.SetDefaultKeys(type, preset);
					break;
			}
		}
	}

	public enum Pcv2Buttons
	{
		Up,
		Down,
		Left,
		Right,
		Esc,
		Pass,
		Circle,
		Clear,
		View
	};
}
