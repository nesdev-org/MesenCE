using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Mesen.Views
{
	public class NesCustomControllerView : UserControl
	{
		public NesCustomControllerView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
