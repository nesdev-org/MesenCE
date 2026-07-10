using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Mesen.Views
{
	public class DefaultControllerView : UserControl
	{
		public DefaultControllerView()
		{
			//Defaults
			MaxWidth = 600;
			Margin = new Thickness(0);

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
