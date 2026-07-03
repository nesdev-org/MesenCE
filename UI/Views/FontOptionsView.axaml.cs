using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Mesen.Config;
using Mesen.Debugger.Controls;
using Mesen.Utilities;
using Mesen.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mesen.Views
{
	public class FontOptionsView : UserControl
	{
		public static readonly StyledProperty<bool> PreferMonospaceProperty = AvaloniaProperty.Register<FontOptionsView, bool>(nameof(PreferMonospace), false);
		public static readonly StyledProperty<bool> ShowWarningProperty = AvaloniaProperty.Register<FontOptionsView, bool>(nameof(ShowWarning), false);
		public static readonly StyledProperty<bool> ShowErrorProperty = AvaloniaProperty.Register<FontOptionsView, bool>(nameof(ShowError), false);
		public static readonly StyledProperty<string?> InternalFontFamilyProperty = AvaloniaProperty.Register<FontOptionsView, string?>(nameof(InternalFontFamily), null, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

		public string? InternalFontFamily
		{
			get { return GetValue(InternalFontFamilyProperty); }
			set { SetValue(InternalFontFamilyProperty, value); }
		}

		public bool PreferMonospace
		{
			get { return GetValue(PreferMonospaceProperty); }
			set { SetValue(PreferMonospaceProperty, value); }
		}

		public bool ShowWarning
		{
			get { return GetValue(ShowWarningProperty); }
			set { SetValue(ShowWarningProperty, value); }
		}

		public bool ShowError
		{
			get { return GetValue(ShowErrorProperty); }
			set { SetValue(ShowErrorProperty, value); }
		}

		static FontOptionsView()
		{
			InternalFontFamilyProperty.Changed.AddClassHandler<FontOptionsView>((s, e) => s.ValidateFont());
		}

		public FontOptionsView()
		{
			InitializeComponent();

			ComboBox cboFontFamily = this.GetControl<ComboBox>("cboFontFamily");
			cboFontFamily.ItemsSource = Configuration.GetSortedFontList();

			ComboBox cboFontSize = this.GetControl<ComboBox>("cboFontSize");
			cboFontSize.ItemsSource = new double[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);
			if(DataContext is FontConfig cfg) {
				InternalFontFamily = cfg.FontFamily;
			}
		}

		private void ValidateFont()
		{
			if(DataContext is FontConfig cfg) {
				if(InternalFontFamily == null) {
					//configured font doesn't exist - keep dropdown blank (FontFamily will crash if given a null)
					ShowWarning = true;
					return;
				}

				var typeface = new Typeface(new FontFamily(InternalFontFamily));
				var largeLetter = new FormattedText("W", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, null);
				// If the font somehow produced too small or too large of a letter, then bail out.
				if(largeLetter.Width < 2 || largeLetter.Height < 2 || !double.IsFinite(largeLetter.Width) || !double.IsFinite(largeLetter.Height)) {
					ShowWarning = false;
					ShowError = true;
					return;
				}
				var smallLetter = new FormattedText("i", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, null);
				// Check that the width of the two letters which are very often different widths in a variable width font are roughly the same.
				// If it is not roughly the same width, then its variable width.
				if(PreferMonospace && Math.Abs(largeLetter.Width - smallLetter.Width) > .01) {
					ShowWarning = true;
				} else {
					ShowWarning = false;
				}

				ShowError = false;
				cfg.FontFamily = InternalFontFamily;
			}
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
