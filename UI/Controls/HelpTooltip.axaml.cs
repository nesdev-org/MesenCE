using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Mesen.Controls;

public class HelpTooltip : UserControl
{
	public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<HelpTooltip, string>(nameof(Text));

	public string Text
	{
		get { return GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}

	public HelpTooltip()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
