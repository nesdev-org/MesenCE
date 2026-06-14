using Mesen.Interop;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mesen.Config
{
	public partial class MovieRecordConfig : BaseConfig<MovieRecordConfig>
	{
		[ObservableProperty] public partial RecordMovieFrom RecordFrom { get; set; } = RecordMovieFrom.CurrentState;
		[ObservableProperty] public partial string Author { get; set; } = "";
		[ObservableProperty] public partial string Description { get; set; } = "";
	}
}
