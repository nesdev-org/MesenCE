using Avalonia.Controls;
using Avalonia.Controls.Selection;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;

namespace Mesen.ViewModels
{
	public partial class CheatDatabaseViewModel : DisposableViewModel
	{
		private List<CheatDbGameEntry> _entries;

		[ObservableProperty] public partial IEnumerable<CheatDbGameEntry> FilteredEntries { get; set; }
		[ObservableProperty] public partial SelectionModel<CheatDbGameEntry?> SelectionModel { get; set; } = new();
		[ObservableProperty] public partial string SearchString { get; set; } = "";

		[Obsolete("For designer only")]
		public CheatDatabaseViewModel() : this(ConsoleType.Snes) { }

		public CheatDatabaseViewModel(ConsoleType consoleType)
		{
			CheatDatabase cheatDb = new();
			try {
				string? dbContent = DependencyHelper.GetFileContent("CheatDb." + consoleType.ToString() + ".json");
				if(dbContent != null) {
					cheatDb = (CheatDatabase?)JsonSerializer.Deserialize(dbContent, typeof(CheatDatabase), MesenCamelCaseSerializerContext.Default) ?? new CheatDatabase();
				}
			} catch { }

			_entries = cheatDb.Games;
			FilteredEntries = _entries;

			if(!Design.IsDesignMode) {
				string sha1 = EmuApi.GetRomHash(HashType.Sha1Cheat);
				SelectionModel.SelectedItem = _entries.Find(e => e.Sha1 == sha1);
			}
		}

		partial void OnSearchStringChanged(string value)
		{
			if(string.IsNullOrWhiteSpace(value)) {
				FilteredEntries = _entries;
			} else {
				FilteredEntries = _entries.Where(e => e.Name.Contains(value, StringComparison.OrdinalIgnoreCase));
			}

			SelectionModel.SelectedItem = FilteredEntries.FirstOrDefault();
		}
	}
}
