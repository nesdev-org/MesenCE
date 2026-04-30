using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mesen.Config;
using Mesen.Utilities;

namespace Mesen.Views
{
	public class VideoConfigView : UserControl
	{
		public VideoConfigView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void btnPreset_OnClick(object sender, RoutedEventArgs e)
		{
			((Button)sender).ContextMenu?.Open();
		}
	}
}
