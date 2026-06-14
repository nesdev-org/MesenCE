using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mesen.Controls;
using Mesen.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Mesen.Debugger.Windows
{
	public class FindAllOccurrencesWindow : MesenWindow
	{
		private static string _lastSearch = "";
		private static bool _lastMatchCase = false;
		private static bool _lastMatchWholeWord = false;

		public string SearchString { get; set; }
		public bool MatchCase { get; set; }
		public bool MatchWholeWord { get; set; }

		public FindAllOccurrencesWindow()
		{
			SearchString = _lastSearch;
			MatchCase = _lastMatchCase;
			MatchWholeWord = _lastMatchWholeWord;

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);
			this.GetControl<TextBox>("txtSearch").FocusAndSelectAll();
		}

		private void Ok_OnClick(object sender, RoutedEventArgs e)
		{
			_lastSearch = SearchString;
			_lastMatchCase = MatchCase;
			_lastMatchWholeWord = MatchWholeWord;
			Close(SearchString);
		}

		private void Cancel_OnClick(object sender, RoutedEventArgs e)
		{
			Close(null!);
		}
	}
}
