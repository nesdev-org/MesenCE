using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mesen.Utilities
{
	public class DisplayHelper
	{
		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool EnumDisplaySettings(string? lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

		private const uint MONITOR_DEFAULTTONEAREST = 2;

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int left, top, right, bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct MONITORINFOEX
		{
			public int cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public int dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szDevice;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct DEVMODE
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmDeviceName;

			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public int dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmFormName;

			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}

		/// <summary>
		/// Returns an array of supported refresh rates for the monitor where the main Avalonia window currently resides.
		/// Assumes the caller has already verified this is executing on a Windows environment.
		/// </summary>
		public static UInt32[] GetRefreshRatesForCurrentMonitor()
		{
			IntPtr hwnd = IntPtr.Zero;

			// 1. Safely grab the Avalonia Main Window handle
			if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
				var window = desktop.MainWindow;
				if(window != null) {
					var platformHandle = window.TryGetPlatformHandle();
					if(platformHandle != null) {
						hwnd = platformHandle.Handle;
					}
				}
			}

			string? deviceName = null;

			// 2. If we successfully got the window handle, find out which monitor it's on
			if(hwnd != IntPtr.Zero) {
				IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
				MONITORINFOEX monitorInfo = new MONITORINFOEX();
				monitorInfo.cbSize = Marshal.SizeOf<MONITORINFOEX>();

				if(GetMonitorInfo(hMonitor, ref monitorInfo)) {
					deviceName = monitorInfo.szDevice;
				}
			}

			// 3. Query the refresh rates (passing 'null' as deviceName falls back to primary display)
			HashSet<UInt32> refreshRates = new HashSet<UInt32>();
			DEVMODE vDevMode = new DEVMODE();
			vDevMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();

			int modeIndex = 0;
			while(EnumDisplaySettings(deviceName, modeIndex, ref vDevMode)) {
				if(vDevMode.dmDisplayFrequency > 0) {
					refreshRates.Add((UInt32)vDevMode.dmDisplayFrequency);
				}

				modeIndex++;
			}

			UInt32[] result = refreshRates.ToArray();
			Array.Sort(result);
			return result;
		}

		/// <summary>
		/// Returns the lowest refresh rate that is equal to or a multiple of <paramref name="targetRate"/>.
		/// Falls back to the highest available rate if no compatible rate is found, or if <paramref name="targetRate"/> is 0.
		/// </summary>
		public static UInt32 GetCompatibleRefreshRate(UInt32[] availableRates, UInt32 targetRate)
		{
			if(targetRate == 0) return availableRates.LastOrDefault();

			var compatibleRate = availableRates.FirstOrDefault(rate => rate % targetRate == 0);

			return compatibleRate == 0 ? availableRates.LastOrDefault() : compatibleRate;
		}
	}
}
