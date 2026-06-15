using Avalonia;
using Avalonia.Data;

namespace DataBoxControl;

public abstract class DataBoxBoundColumn : DataBoxColumn
{
	public static readonly StyledProperty<BindingBase?> BindingProperty =
		 AvaloniaProperty.Register<DataBoxBoundColumn, BindingBase?>(nameof(Binding));

	public static readonly StyledProperty<BindingBase?> IsVisibleProperty =
	 AvaloniaProperty.Register<DataBoxBoundColumn, BindingBase?>(nameof(IsVisible));

	[AssignBinding]
	public BindingBase? Binding
	{
		get => GetValue(BindingProperty);
		set => SetValue(BindingProperty, value);
	}

	[AssignBinding]
	public BindingBase? IsVisible
	{
		get => GetValue(IsVisibleProperty);
		set => SetValue(IsVisibleProperty, value);
	}
}
