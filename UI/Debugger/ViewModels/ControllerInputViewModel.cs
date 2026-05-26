using Avalonia;
using Avalonia.Controls;
using Mesen.Interop;
using Mesen.Utilities;
using Mesen.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Mesen.Debugger.ViewModels
{
	public partial class ControllerInputViewModel : ViewModelBase
	{
		[ObservableProperty] public partial int ViewHeight { get; set; }

		[ObservableProperty] public partial bool ButtonA { get; set; }
		[ObservableProperty] public partial bool ButtonB { get; set; }
		[ObservableProperty] public partial bool ButtonX { get; set; }
		[ObservableProperty] public partial bool ButtonY { get; set; }
		[ObservableProperty] public partial bool ButtonL { get; set; }
		[ObservableProperty] public partial bool ButtonR { get; set; }
		[ObservableProperty] public partial bool ButtonU { get; set; }
		[ObservableProperty] public partial bool ButtonD { get; set; }
		[ObservableProperty] public partial bool ButtonUp { get; set; }
		[ObservableProperty] public partial bool ButtonDown { get; set; }
		[ObservableProperty] public partial bool ButtonLeft { get; set; }
		[ObservableProperty] public partial bool ButtonRight { get; set; }
		[ObservableProperty] public partial bool ButtonSelect { get; set; }
		[ObservableProperty] public partial bool ButtonStart { get; set; }

		public int ControllerIndex { get; }
		public bool IsSnes { get; }
		public bool IsWs { get; }
		public bool HasShoulderButtons { get; }
		public bool HasSelectButton { get; }
		public bool HasStartButton { get; }

		[Obsolete("For designer only")]
		public ControllerInputViewModel() : this(ConsoleType.Ws, 0) { }

		public ControllerInputViewModel(ConsoleType consoleType, int index)
		{
			ControllerIndex = index + 1;
			IsSnes = consoleType == ConsoleType.Snes;
			IsWs = consoleType == ConsoleType.Ws;
			HasShoulderButtons = consoleType == ConsoleType.Snes || consoleType == ConsoleType.Gba;
			HasSelectButton = consoleType != ConsoleType.Sms;
			HasStartButton = consoleType != ConsoleType.Sms || index == 0;
			ViewHeight = consoleType != ConsoleType.Ws ? (HasShoulderButtons ? 34 : 30) : 64;

			if(Design.IsDesignMode) {
				return;
			}

			PropertyChanged += ControllerInputViewModel_PropertyChanged;
		}

		private void ControllerInputViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			SetInputOverrides();
		}

		public void SetInputOverrides()
		{
			DebugApi.SetInputOverrides((uint)ControllerIndex - 1, new DebugControllerState() {
				A = ButtonA,
				B = ButtonB,
				X = ButtonX,
				Y = ButtonY,
				L = ButtonL,
				R = ButtonR,
				U = ButtonU,
				D = ButtonD,
				Up = ButtonUp,
				Down = ButtonDown,
				Left = ButtonLeft,
				Right = ButtonRight,
				Select = ButtonSelect,
				Start = ButtonStart
			});
		}

		public void OnButtonClick(object param)
		{
			if(param is string buttonName) {
				switch(buttonName) {
					case "A": ButtonA = !ButtonA; break;
					case "B": ButtonB = !ButtonB; break;
					case "X": ButtonX = !ButtonX; break;
					case "Y": ButtonY = !ButtonY; break;
					case "L": ButtonL = !ButtonL; break;
					case "R": ButtonR = !ButtonR; break;
					case "U": ButtonU = !ButtonU; break;
					case "D": ButtonD = !ButtonD; break;
					case "Up": ButtonUp = !ButtonUp; break;
					case "Down": ButtonDown = !ButtonDown; break;
					case "Left": ButtonLeft = !ButtonLeft; break;
					case "Right": ButtonRight = !ButtonRight; break;
					case "Select": ButtonSelect = !ButtonSelect; break;
					case "Start": ButtonStart = !ButtonStart; break;
				}
			}
		}
	}
}
