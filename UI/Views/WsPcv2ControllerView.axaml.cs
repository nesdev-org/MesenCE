using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Mesen.Views;

public class WsPcv2ControllerView : UserControl
{
	public WsPcv2ControllerView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
