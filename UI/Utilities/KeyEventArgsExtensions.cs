using Avalonia.Input;
using System;

namespace Mesen.Utilities
{
	public static class KeyEventArgsExtensions
	{
		public static bool IsSpecialKey(this KeyEventArgs key)
		{
			//Some keys only trigger a KeyUp event without a matching KeyDown event
			//And some trigger both events at the same time, causing the emulator to never see the key press.
			switch(key.Key) {
				case Key.PrintScreen:
				case Key.Pause:
				case Key.Scroll:
					return true;

				default:
					return false;
			}
		}

		public static UInt16 GetKeyCode(this KeyEventArgs key)
		{
			if(key.PhysicalKey == PhysicalKey.NumPadEnter) {
				return (UInt16)0x1FF;
			} else {
				return (UInt16)key.Key;
			}
		}
	}
}
