using System;
using Mesen.Interop;

namespace Mesen.Config
{
	public class RestrictToPlatformsAttribute : Attribute
	{
		public CpuType[] SupportedCpuTypes { get; set; }

		public RestrictToPlatformsAttribute(params CpuType[] supportedCpuTypes)
		{
			this.SupportedCpuTypes = supportedCpuTypes;
		}
	}
}
