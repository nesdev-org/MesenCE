using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Utilities;
using System;
using System.Text.Json;

namespace Mesen.Config
{
	public partial class BaseConfig<T> : ObservableObject where T : class
	{
		public T Clone()
		{
			if(this is T obj) {
				return JsonHelper.Clone<T>(obj);
			} else {
				throw new InvalidCastException();
			}
		}

		public bool IsIdentical(T other)
		{
			string a = JsonSerializer.Serialize(this, this.GetType(), MesenSerializerContext.Default);
			string b = JsonSerializer.Serialize(other, this.GetType(), MesenSerializerContext.Default);
			return a == b;
		}
	}
}
