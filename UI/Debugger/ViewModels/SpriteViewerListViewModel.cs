using Avalonia.Controls.Selection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBoxControl;
using Mesen.Config;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Mesen.Debugger.ViewModels
{
	public partial class SpriteViewerListViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial bool ShowListView { get; set; }
		[ObservableProperty] public partial double MinListViewHeight { get; set; }
		[ObservableProperty] public partial double ListViewHeight { get; set; }
		[ObservableProperty] public partial List<SpritePreviewModel>? SpritePreviews { get; set; } = null;
		public SelectionModel<SpritePreviewModel?> Selection { get; private set; } = new();
		public SortState SortState { get; private set; } = new();
		public List<int> ColumnWidths => SpriteViewer.Config.ColumnWidths;

		public ICommand SortCommand { get; }
		public SpriteViewerViewModel SpriteViewer { get; }
		public SpriteViewerConfig Config { get; }

		private DateTime _lastRefresh = DateTime.MinValue;

		public SpriteViewerListViewModel(SpriteViewerViewModel viewer)
		{
			SpriteViewer = viewer;
			Config = viewer.Config;
			ShowListView = Config.ShowListView;
			ListViewHeight = Config.ShowListView ? Config.ListViewHeight : 0;

			SortState.SetColumnSort("SpriteIndex", ListSortDirection.Ascending, false);

			SortCommand = new RelayCommand(() => {
				RefreshList(true);
			});

			AddDisposable(Selection.ObserveProp(nameof(Selection.SelectedItem), () => {
				if(Selection.SelectedItem != null) {
					SpriteViewer.SelectSprite(Selection.SelectedItem.SpriteIndex);
				}
			}));
		}

		public void SelectSprite(int spriteIndex)
		{
			Selection.SelectedItem = SpritePreviews?.Find(x => x.SpriteIndex == spriteIndex);
		}

		public void ForceRefresh()
		{
			_lastRefresh = DateTime.MinValue;
		}

		private Dictionary<string, Func<SpritePreviewModel, SpritePreviewModel, int>> _comparers = new() {
			{ "SpriteIndex", (a, b) => a.SpriteIndex.CompareTo(b.SpriteIndex) },
			{ "X", (a, b) => a.X.CompareTo(b.X) },
			{ "Y", (a, b) => a.Y.CompareTo(b.Y) },
			{ "TileIndex", (a, b) => a.TileIndex.CompareTo(b.TileIndex) },
			{ "Size", (a, b) => {
				int result = a.Width.CompareTo(b.Width);
				if(result == 0) {
					return a.Height.CompareTo(b.Height);
				}
				return result;
			} },
			{ "Palette", (a, b) => a.Palette.CompareTo(b.Palette) },
			{ "Priority", (a, b) => a.Priority.CompareTo(b.Priority) },
			{ "Flags", (a, b) => string.Compare(a.Flags, b.Flags, StringComparison.OrdinalIgnoreCase) },
			{ "Visible", (a, b) => a.FadePreview.CompareTo(b.FadePreview) }
		};

		public void RefreshList(bool force = false)
		{
			if(!force && (DateTime.Now - _lastRefresh).TotalMilliseconds < 70) {
				return;
			}

			_lastRefresh = DateTime.Now;

			if(!ShowListView) {
				SpritePreviews = null;
				return;
			}

			if(SpritePreviews == null || SpritePreviews.Count != SpriteViewer.SpritePreviews.Count) {
				SpritePreviews = SpriteViewer.SpritePreviews.Select(x => x.Clone()).ToList();
			}

			int? selectedIndex = Selection.SelectedItem?.SpriteIndex;

			List<SpritePreviewModel> newList = new(SpriteViewer.SpritePreviews.Select(x => x.Clone()).ToList());
			SortHelper.SortList(newList, SortState.SortOrder, _comparers, "SpriteIndex");

			for(int i = 0; i < newList.Count; i++) {
				newList[i].CopyTo(SpritePreviews[i]);
			}

			if(selectedIndex != null && Selection.SelectedItem?.SpriteIndex != selectedIndex) {
				Selection.SelectedItem = null;
			}
		}

		partial void OnShowListViewChanged(bool value)
		{
			Config.ShowListView = value;
			ListViewHeight = value ? Config.ListViewHeight : 0;
			MinListViewHeight = value ? 100 : 0;
			RefreshList(true);
		}

		partial void OnListViewHeightChanged(double value)
		{
			if(ShowListView) {
				Config.ListViewHeight = value;
			} else {
				ListViewHeight = 0;
			}
		}
	}
}
