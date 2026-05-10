using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mesen.Utilities
{
	public static class EnumExtensions
	{
		[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Fields are not trimmed for enum types")]
		public static T? GetAttribute<T>(this Enum val) where T : Attribute
		{
			return val.GetType().GetField(val.ToString(), BindingFlags.Public | BindingFlags.Static)!.GetCustomAttribute<T>();
		}
	}
}
