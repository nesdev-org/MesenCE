using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Debugger.Utilities;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mesen.Debugger.ViewModels;

public partial class MemoryViewerFindViewModel : DisposableViewModel
{
	[ObservableProperty] public partial SearchDataType DataType { get; set; }
	[ObservableProperty] public partial SearchIntType IntType { get; set; }
	[ObservableProperty] public partial bool CaseSensitive { get; set; }
	[ObservableProperty] public partial bool UseTblMappings { get; set; }

	[ObservableProperty] public partial bool FilterNotAccessed { get; set; }
	[ObservableProperty] public partial bool FilterRead { get; set; }
	[ObservableProperty] public partial bool FilterWrite { get; set; }
	[ObservableProperty] public partial bool FilterExec { get; set; }
	[ObservableProperty] public partial bool FilterTimeSpanEnabled { get; set; }
	[ObservableProperty] public partial int FilterTimeSpan { get; set; }

	[ObservableProperty] public partial bool FilterCode { get; set; }
	[ObservableProperty] public partial bool FilterData { get; set; }
	[ObservableProperty] public partial bool FilterUnidentified { get; set; }

	[ObservableProperty] public partial bool IsInteger { get; private set; }
	[ObservableProperty] public partial bool IsString { get; private set; }
	[ObservableProperty] public partial bool IsValid { get; private set; } = false;
	[ObservableProperty] public partial bool ShowNotFoundError { get; set; }
	[ObservableProperty] public partial string SearchString { get; set; } = "";

	private MemoryToolsViewModel _memToolsModel;

	[Obsolete("For designer only")]
	public MemoryViewerFindViewModel() : this(new(new())) { }

	public MemoryViewerFindViewModel(MemoryToolsViewModel memToolsModel)
	{
		_memToolsModel = memToolsModel;

		AddDisposable(this.ObserveProp(nameof(DataType), () => {
			IsInteger = DataType == SearchDataType.Integer;
			IsString = DataType == SearchDataType.String;
		}));

		AddDisposable(this.ObserveProp(nameof(SearchString), () => {
			if(SearchString.Contains(Environment.NewLine)) {
				//Run asynchronously to allow the textbox to update its content correctly
				Dispatcher.UIThread.Post(() => {
					SearchString = SearchString.Replace(Environment.NewLine, " ");
				});
			}
		}));

		AddDisposable(this.ObserveProp([nameof(DataType), nameof(IntType), nameof(SearchString)], () => {
			SearchData? searchData = GetSearchData();
			IsValid = searchData != null && searchData.Data.Length > 0;
		}));
	}

	public SearchData? GetSearchData()
	{
		switch(DataType) {
			case SearchDataType.Hex:
				if(Regex.IsMatch(SearchString, "^[ a-f0-9?]+$", RegexOptions.IgnoreCase)) {
					return new SearchData(HexUtilities.HexToArrayWithWildcards(SearchString));
				}
				break;

			case SearchDataType.String:
				if(UseTblMappings && _memToolsModel.TblConverter != null) {
					if(CaseSensitive) {
						return new SearchData(_memToolsModel.TblConverter.GetBytes(SearchString));
					} else {
						byte[] lcData = _memToolsModel.TblConverter.GetBytes(SearchString.ToLower());
						byte[] ucData = _memToolsModel.TblConverter.GetBytes(SearchString.ToUpper());
						if(lcData.Length != ucData.Length) {
							return null;
						}
						return new SearchData(lcData, ucData);
					}
				} else {
					if(CaseSensitive) {
						return new SearchData(Encoding.UTF8.GetBytes(SearchString));
					} else {
						byte[] lcData = Encoding.UTF8.GetBytes(SearchString.ToLower());
						byte[] ucData = Encoding.UTF8.GetBytes(SearchString.ToUpper());
						if(lcData.Length != ucData.Length) {
							return null;
						}
						return new SearchData(lcData, ucData);
					}
				}

			case SearchDataType.Integer:
				if(long.TryParse(SearchString, out long value)) {
					switch(IntType) {
						case SearchIntType.IntAuto:
							if(value >= Int32.MinValue && value <= UInt32.MaxValue) {
								return new SearchData(new byte[] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 24) & 0xFF) });
							} else if(value >= Int16.MinValue && value <= UInt16.MaxValue) {
								return new SearchData(new byte[] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF) });
							} else if(value >= sbyte.MinValue && value <= byte.MaxValue) {
								return new SearchData(new byte[] { (byte)(value & 0xFF) });
							}
							break;

						case SearchIntType.Int32:
							if(value >= Int32.MinValue && value <= UInt32.MaxValue) {
								return new SearchData(new byte[] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 24) & 0xFF) });
							}
							break;

						case SearchIntType.Int16:
							if(value >= Int16.MinValue && value <= UInt16.MaxValue) {
								return new SearchData(new byte[] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF) });
							}
							break;

						case SearchIntType.Int8:
							if(value >= sbyte.MinValue && value <= byte.MaxValue) {
								return new SearchData(new byte[] { (byte)(value & 0xFF) });
							}
							break;
					}
				}
				break;
		}

		return null;
	}

	public bool IsDataTypeFiltered
	{
		get
		{
			int count = (FilterCode ? 1 : 0) + (FilterData ? 1 : 0) + (FilterUnidentified ? 1 : 0);
			return count > 0 && count < 3;
		}
	}

	public bool IsAccessFiltered
	{
		get
		{
			int count = (FilterNotAccessed ? 1 : 0) + (FilterRead ? 1 : 0) + (FilterWrite ? 1 : 0) + (FilterExec ? 1 : 0);
			return count > 0 && count < 4;
		}
	}
}

public class SearchData
{
	public short[] Data;
	public short[]? DataAlt; //used for case insensitive searches

	public SearchData(byte[] data, byte[]? dataAlt = null)
	{
		Data = new short[data.Length];
		Array.Copy(data, 0, Data, 0, data.Length);
		if(dataAlt != null) {
			DataAlt = new short[dataAlt.Length];
			Array.Copy(dataAlt, 0, DataAlt, 0, dataAlt.Length);
		}
	}

	public SearchData(short[] data)
	{
		Data = data;
	}
}

public enum SearchDataType
{
	Hex,
	String,
	Integer,
}

public enum SearchIntType
{
	IntAuto,
	Int8,
	Int16,
	Int32
}

public enum SearchDirection
{
	Forward,
	Backward
}
