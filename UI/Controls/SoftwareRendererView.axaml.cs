using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Config;
using Mesen.Interop;
using Mesen.Localization;
using Mesen.Utilities;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mesen.Controls
{
	public class SoftwareRendererView : UserControl
	{
		private SimpleImageViewer _frame;
		private SimpleImageViewer _emuHud;
		private SimpleImageViewer _scriptHud;
		private SoftwareRendererViewModel _model = new();

		public SoftwareRendererView()
		{
			InitializeComponent();

			_frame = this.GetControl<SimpleImageViewer>("Frame");
			_emuHud = this.GetControl<SimpleImageViewer>("EmuHud");
			_scriptHud = this.GetControl<SimpleImageViewer>("ScriptHud");
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);
			if(DataContext is SoftwareRendererViewModel model) {
				_model = model;
			}
		}

		private unsafe void UpdateSurface(SoftwareRendererSurface frame, DynamicBitmap? surface, Action<DynamicBitmap> updateSurfaceRef)
		{
			PixelSize frameSize = new PixelSize((int)frame.Width, (int)frame.Height);
			if(surface?.PixelSize != frameSize) {
				surface = new DynamicBitmap(frameSize, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
				updateSurfaceRef(surface);
			}

			int size = (int)frame.Width * (int)frame.Height * sizeof(UInt32);
			using(var bitmapLock = surface.Lock()) {
				var srcSpan = new Span<byte>((byte*)frame.FrameBuffer, size);
				var dstSpan = new Span<byte>((byte*)bitmapLock.FrameBuffer.Address, size);
				srcSpan.CopyTo(dstSpan);
			}
		}

		public unsafe void UpdateSoftwareRenderer(SoftwareRendererFrame frameInfo)
		{
			UpdateSurface(frameInfo.Frame, _model.FrameSurface, s => _model.FrameSurface = s);
			if(frameInfo.EmuHud.IsDirty) {
				UpdateSurface(frameInfo.EmuHud, _model.EmuHudSurface, s => _model.EmuHudSurface = s);
			}
			if(frameInfo.ScriptHud.IsDirty) {
				UpdateSurface(frameInfo.ScriptHud, _model.ScriptHudSurface, s => _model.ScriptHudSurface = s);
			}

			Dispatcher.UIThread.Post(() => {
				_frame.UseBilinearInterpolation = ConfigManager.Config.Video.UseBilinearInterpolation;
				_frame.InvalidateVisual();
				_emuHud.InvalidateVisual();
				_scriptHud.InvalidateVisual();
			}, DispatcherPriority.MaxValue);
		}
	}

	public partial class SoftwareRendererViewModel : ViewModelBase
	{
		[ObservableProperty] public partial DynamicBitmap? FrameSurface { get; set; }
		[ObservableProperty] public partial DynamicBitmap? EmuHudSurface { get; set; }
		[ObservableProperty] public partial DynamicBitmap? ScriptHudSurface { get; set; }
		[ObservableProperty] public partial double Width { get; set; }
		[ObservableProperty] public partial double Height { get; set; }
	}
}
