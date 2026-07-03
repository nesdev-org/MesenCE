using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using System;

namespace DataBoxControl;

public class DataBoxCell : ContentControl
{
	internal DataBox? DataBox { get; set; }
	internal DataBoxColumn? Column { get; set; }

	protected override Type StyleKeyOverride => typeof(DataBoxCell);
}
