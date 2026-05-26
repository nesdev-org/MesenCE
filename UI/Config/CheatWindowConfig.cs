using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Config
{
	public partial class CheatWindowConfig : BaseWindowConfig<CheatWindowConfig>
	{
		public bool DisableAllCheats { get; set; } = false;
		public List<int> ColumnWidths { get; set; } = new();
	}
}
