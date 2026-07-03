using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Utilities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;

namespace Mesen.ViewModels
{
	public class CheatDatabaseViewModel : DisposableViewModel
	{
		private List<CheatDbGameEntry> _entries;

		[Reactive] public IEnumerable<CheatDbGameEntry> FilteredEntries { get; set; }
		[Reactive] public SelectionModel<CheatDbGameEntry?> SelectionModel { get; set; } = new();
		[Reactive] public string SearchString { get; set; } = "";

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
			AddDisposable(this.WhenAnyValue(x => x.SearchString).Subscribe(x => {
				if(string.IsNullOrWhiteSpace(x)) {
					FilteredEntries = _entries;
				} else {
					FilteredEntries = _entries.Where(e => e.Name.Contains(x, StringComparison.OrdinalIgnoreCase));
				}

				SelectionModel.SelectedItem = FilteredEntries.FirstOrDefault();
			}));

			if(!Design.IsDesignMode) {
				string sha1 = EmuApi.GetRomHash(HashType.Sha1Cheat);
				SelectionModel.SelectedItem = _entries.Find(e => e.Sha1 == sha1);
			}
		}
	}
}
