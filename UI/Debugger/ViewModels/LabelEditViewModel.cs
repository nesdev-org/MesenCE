using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mesen.Debugger.Labels;
using Mesen.Interop;
using Mesen.Localization;
using Mesen.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mesen.Debugger.ViewModels
{
	public partial class LabelEditViewModel : DisposableViewModel
	{
		[ObservableProperty] public partial ReactiveCodeLabel Label { get; set; }

		[ObservableProperty] public partial bool OkEnabled { get; private set; } = false;
		[ObservableProperty] public partial string MaxAddress { get; private set; } = "";
		[ObservableProperty] public partial string ErrorMessage { get; private set; } = "";

		public bool AllowDelete { get; } = false;

		public Enum[] AvailableMemoryTypes { get; private set; } = Array.Empty<Enum>();
		public CpuType CpuType { get; }

		private CodeLabel? _originalLabel;

		[Obsolete("For designer only")]
		public LabelEditViewModel() : this(CpuType.Snes, new CodeLabel()) { }

		public LabelEditViewModel(CpuType cpuType, CodeLabel label, CodeLabel? originalLabel = null)
		{
			_originalLabel = originalLabel;

			Label = new ReactiveCodeLabel(label);
			AllowDelete = originalLabel != null;

			if(Design.IsDesignMode) {
				return;
			}

			CpuType = cpuType;
			AvailableMemoryTypes = Enum.GetValues<MemoryType>().Where(t => cpuType.CanAccessMemoryType(t) && t.SupportsLabels() && DebugApi.GetMemorySize(t) > 0).Cast<Enum>().ToArray();
			if(!AvailableMemoryTypes.Contains(Label.MemoryType)) {
				Label.MemoryType = (MemoryType)AvailableMemoryTypes[0];
			}

			Label.PropertyChanged += (s, e) => {
				ValidateLabel(originalLabel);
			};
			ValidateLabel(originalLabel);
		}

		private void ValidateLabel(CodeLabel? originalLabel)
		{
			int maxAddress = DebugApi.GetMemorySize(Label.MemoryType) - 1;
			if(maxAddress <= 0) {
				MaxAddress = "(unavailable)";
			} else {
				MaxAddress = "(Max: $" + maxAddress.ToString("X4") + ")";
			}

			CodeLabel? sameLabel = LabelManager.GetLabel(Label.Label);

			for(UInt32 i = 0; i < Label.Length; i++) {
				CodeLabel? sameAddress = LabelManager.GetLabel(Label.Address + i, Label.MemoryType);
				if(sameAddress != null) {
					if(originalLabel == null || (sameAddress.Label != originalLabel.Label && !sameAddress.Label.StartsWith(originalLabel.Label + "+"))) {
						//A label already exists, we're trying to edit an existing label, but the existing label
						//and the label we're editing aren't the same label.  Can't override an existing label with a different one.
						ErrorMessage = ResourceHelper.GetMessage("AddressHasOtherLabel", sameAddress.Label.Length > 0 ? sameAddress.Label : sameAddress.Comment);
						OkEnabled = false;
						return;
					}
				}
			}

			if(Label.Address + (Label.Length - 1) > maxAddress) {
				ErrorMessage = ResourceHelper.GetMessage("AddressOutOfRange");
				OkEnabled = false;
				return;
			}

			if(Label.Label.Length == 0 && Label.Comment.Length == 0) {
				ErrorMessage = ResourceHelper.GetMessage("LabelOrCommentRequired");
				OkEnabled = false;
				return;
			}

			if(Label.Label.Length > 0 && !LabelManager.LabelRegex.IsMatch(Label.Label)) {
				ErrorMessage = ResourceHelper.GetMessage("InvalidLabel");
				OkEnabled = false;
				return;
			}

			if(sameLabel != null && sameLabel != originalLabel) {
				ErrorMessage = ResourceHelper.GetMessage("LabelNameInUse");
				OkEnabled = false;
				return;
			}

			if(Label.Length >= 1 && Label.Length <= 65536 && !Label.Comment.Contains('\x1')) {
				ErrorMessage = "";
				OkEnabled = true;
				return;
			}

			OkEnabled = false;
		}

		public void DeleteLabel()
		{
			if(_originalLabel != null) {
				LabelManager.DeleteLabel(_originalLabel, true);
			}
		}

		public void Commit()
		{
			Label.Commit();
		}

		public partial class ReactiveCodeLabel : ObservableObject
		{
			private CodeLabel _originalLabel;

			public ReactiveCodeLabel(CodeLabel label)
			{
				_originalLabel = label;

				Address = label.Address;
				Label = label.Label;
				Comment = label.Comment;
				MemoryType = label.MemoryType;
				Flags = label.Flags;
				Length = label.Length > LabelManager.MaxLength ? LabelManager.MaxLength : label.Length;
			}

			public void Commit()
			{
				_originalLabel.Address = Address;
				_originalLabel.Label = Label;
				_originalLabel.Comment = Comment;
				_originalLabel.MemoryType = MemoryType;
				_originalLabel.Flags = Flags;
				_originalLabel.Length = Length;
			}

			[ObservableProperty] public partial UInt32 Address { get; set; }
			[ObservableProperty] public partial MemoryType MemoryType { get; set; }
			[ObservableProperty] public partial string Label { get; set; } = "";
			[ObservableProperty] public partial string Comment { get; set; } = "";
			[ObservableProperty] public partial CodeLabelFlags Flags { get; set; }
			[ObservableProperty] public partial UInt32 Length { get; set; } = 1;

			protected override void OnPropertyChanged(PropertyChangedEventArgs e)
			{
				base.OnPropertyChanged(e);
			}
		}
	}
}
