using Mesen.Interop;
using Mesen.Localization;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mesen.Config
{
	public class SnesControllerConfig : ControllerConfig
	{
		public new SnesKeyMapping Mapping1 { get => (SnesKeyMapping)_mapping1; set => _mapping1 = value; }
		public new SnesKeyMapping Mapping2 { get => (SnesKeyMapping)_mapping2; set => _mapping2 = value; }
		public new SnesKeyMapping Mapping3 { get => (SnesKeyMapping)_mapping3; set => _mapping3 = value; }
		public new SnesKeyMapping Mapping4 { get => (SnesKeyMapping)_mapping4; set => _mapping4 = value; }

		public SnesControllerConfig()
		{
			_mapping1 = new SnesKeyMapping();
			_mapping2 = new SnesKeyMapping();
			_mapping3 = new SnesKeyMapping();
			_mapping4 = new SnesKeyMapping();
		}
	}

	public class SnesKeyMapping : KeyMapping
	{
		public UInt16[]? MouseButtons { get; set; } = null;
		public UInt16[]? SuperScopeButtons { get; set; } = null;
		public UInt16[]? NttDataKeypadButtons { get; set; } = null;

		protected override UInt16[]? GetCustomButtons(ControllerType type)
		{
			return type switch {
				ControllerType.SnesMouse => MouseButtons,
				ControllerType.SuperScope => SuperScopeButtons,
				ControllerType.SnesNttDataKeypad => NttDataKeypadButtons,
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
				ControllerType.SnesMouse => Enum.GetValues<GenericMouseButtons>().Select(val => new CustomKeyMapping(ResourceHelper.GetEnumText(val), buttonMappings, (int)val)).ToList(),
				ControllerType.SuperScope => Enum.GetValues<SnesSuperScopeButtons>().Select(val => new CustomKeyMapping(ResourceHelper.GetEnumText(val), buttonMappings, (int)val)).ToList(),
				ControllerType.SnesNttDataKeypad => Enum.GetValues<SnesNttDataKeypadButtons>().Select(val => new CustomKeyMapping(ResourceHelper.GetEnumText(val), buttonMappings, (int)val)).ToList(),
				_ => new()
			};

			keys.Sort((a, b) => a.Name.CompareTo(b.Name));

			return keys;
		}

		public override void ClearKeys(ControllerType type)
		{
			switch(type) {
				case ControllerType.SnesMouse:
					MouseButtons = new UInt16[2];
					break;

				case ControllerType.SuperScope:
					SuperScopeButtons = new UInt16[4];
					break;

				case ControllerType.SnesNttDataKeypad:
					NttDataKeypadButtons = new UInt16[15];
					base.ClearKeys(type);
					break;

				case ControllerType.SnesController:
				case ControllerType.SnesRumbleController:
					base.ClearKeys(type);
					break;
			}
		}

		public override UInt16[]? GetDefaultCustomKeys(ControllerType type, KeyPresetType? preset)
		{
			switch(type) {
				case ControllerType.SnesMouse:
					return new UInt16[2] {
						InputApi.GetKeyCode("Mouse Left"),
						InputApi.GetKeyCode("Mouse Right")
					};

				case ControllerType.SuperScope:
					return new UInt16[4] {
						InputApi.GetKeyCode("Mouse Left"),
						InputApi.GetKeyCode("Mouse Right"),
						InputApi.GetKeyCode("Mouse Middle"),
						InputApi.GetKeyCode("Enter"),
					};

				case ControllerType.SnesNttDataKeypad:
					return new UInt16[15] {
						InputApi.GetKeyCode("Numpad 0"),
						InputApi.GetKeyCode("Numpad 1"),
						InputApi.GetKeyCode("Numpad 2"),
						InputApi.GetKeyCode("Numpad 3"),
						InputApi.GetKeyCode("Numpad 4"),
						InputApi.GetKeyCode("Numpad 5"),
						InputApi.GetKeyCode("Numpad 6"),
						InputApi.GetKeyCode("Numpad 7"),
						InputApi.GetKeyCode("Numpad 8"),
						InputApi.GetKeyCode("Numpad 9"),
						InputApi.GetKeyCode("Numpad *"),
						InputApi.GetKeyCode("Numpad +"),
						InputApi.GetKeyCode("Numpad ."),
						InputApi.GetKeyCode("Numpad -"),
						InputApi.GetKeyCode("Numpad /"),
					};

				default:
					return null;
			}
		}

		public override void SetDefaultKeys(ControllerType type, KeyPresetType? preset)
		{
			switch(type) {
				case ControllerType.SnesMouse: MouseButtons = GetDefaultCustomKeys(type, preset); break;
				case ControllerType.SuperScope: SuperScopeButtons = GetDefaultCustomKeys(type, preset); break;

				case ControllerType.SnesNttDataKeypad:
					base.SetDefaultKeys(type, preset);
					NttDataKeypadButtons = GetDefaultCustomKeys(type, preset);
					break;

				default:
					base.SetDefaultKeys(type, preset);
					break;
			}
		}
	}

	public enum SnesSuperScopeButtons { Fire, Cursor, Turbo, Pause };
	public enum SnesNttDataKeypadButtons { Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9, Star, Pound, Period, C, EndComunication };
}
