using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.ViewModels
{
	public partial class ColorPickerViewModel : ViewModelBase
	{
		[ObservableProperty] public partial Color Color { get; set; }
	}
}
