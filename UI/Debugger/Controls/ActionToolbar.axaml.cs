using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Mesen.Config;
using Mesen.Debugger.Utilities;
using Mesen.Debugger.ViewModels;
using Mesen.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mesen.Debugger.Controls
{
	public class ActionToolbar : UserControl
	{
		public static readonly StyledProperty<List<ContextMenuAction>> ItemsProperty = AvaloniaProperty.Register<ActionToolbar, List<ContextMenuAction>>(nameof(Items));
		private DispatcherTimer _timer;

		public List<ContextMenuAction> Items
		{
			get { return GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		public ActionToolbar()
		{
			InitializeComponent();
			_timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, (s, e) => UpdateToolbar());
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
		{
			if(Design.IsDesignMode) {
				return;
			}

			_timer.Start();
		}

		protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
		{
			_timer.Stop();
		}

		private void Button_OnClick(object sender, RoutedEventArgs e)
		{
			if(sender is Button btn && btn.DataContext is ContextMenuAction action && action.SubActions?.Count > 0) {
				((Button)sender).ContextMenu?.Open();
			}
		}

		private void Button_ContextRequested(object? sender, ContextRequestedEventArgs e)
		{
			e.Handled = true;
		}

		private void UpdateToolbar()
		{
			if(Items != null) {
				foreach(object item in Items) {
					if(item is ContextMenuAction act) {
						act.Update();
					}
				}
			}
		}
	}
}
