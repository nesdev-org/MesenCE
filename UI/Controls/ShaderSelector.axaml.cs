using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mesen.Controls;

public class ShaderSelector : UserControl
{
	public static readonly StyledProperty<string> ShaderFileProperty = AvaloniaProperty.Register<ShaderSelector, string>(nameof(ShaderFile), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
	public static readonly StyledProperty<string> DisplayValueProperty = AvaloniaProperty.Register<ShaderSelector, string>(nameof(DisplayValue));
	public static readonly StyledProperty<ConsoleOverrideConfig> ConsoleOverrideConfigProperty = AvaloniaProperty.Register<ShaderSelector, ConsoleOverrideConfig>(nameof(ConsoleOverrideConfig));

	public string ShaderFile
	{
		get { return GetValue(ShaderFileProperty); }
		set { SetValue(ShaderFileProperty, value); }
	}

	public string DisplayValue
	{
		get { return GetValue(DisplayValueProperty); }
		set { SetValue(DisplayValueProperty, value); }
	}

	public ConsoleOverrideConfig ConsoleOverrideConfig
	{
		get { return GetValue(ConsoleOverrideConfigProperty); }
		set { SetValue(ConsoleOverrideConfigProperty, value); }
	}

	static ShaderSelector()
	{
		ShaderFileProperty.Changed.AddClassHandler<ShaderSelector>((x, e) => {
			x.DisplayValue = Path.GetFileNameWithoutExtension(x.ShaderFile);
		});
	}

	public ShaderSelector()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private async void BtnSettings_OnClick(object sender, RoutedEventArgs e)
	{
		Window? wnd = this.GetWindow();
		if(wnd == null) {
			return;
		}

		Button btn = (Button)sender;
		btn.ContextMenu = new ContextMenu() {
			Name = "ActionMenu",
			Placement = PlacementMode.BottomEdgeAlignedLeft,
			ItemsSource = ShaderMenuHelper.GetShaderMenu(wnd, () => ShaderFile, v => ShaderFile = v, ConsoleOverrideConfig == null || ConsoleOverrideConfig.GetActiveOverride() == ConsoleOverrideConfig).SubActions
		};
		btn.ContextMenu.Open();
	}
}
