#include "pch.h"
#include "SNES/SnesDefaultVideoFilter.h"
#include "Shared/Emulator.h"
#include "Shared/EmuSettings.h"
#include "Shared/RewindManager.h"
#include "Shared/SettingTypes.h"
#include "Shared/ColorUtilities.h"

SnesDefaultVideoFilter::SnesDefaultVideoFilter(Emulator* emu) : BaseVideoFilter(emu)
{
	InitLookupTable();
	_prevFrame = new uint16_t[512 * 478];
	memset(_prevFrame, 0, 512 * 478 * sizeof(uint16_t));
}

SnesDefaultVideoFilter::~SnesDefaultVideoFilter()
{
	delete[] _prevFrame;
}

FrameInfo SnesDefaultVideoFilter::GetFrameInfo()
{
	if(_emu->GetRomInfo().Format == RomFormat::Spc) {
		//Give a fixed 256x240 of space to SPC player to match NES/GB players
		FrameInfo frame;
		frame.Width = 256;
		frame.Height = 240;
		return frame;
	} else {
		if(_forceFixedRes && _baseFrameInfo.Width == 256) {
			OverscanDimensions overscan = GetOverscan();
			return FrameInfo { 512 - (overscan.Left + overscan.Right) * 2, 478 - (overscan.Top + overscan.Bottom) * 2 };
		} else {
			return BaseVideoFilter::GetFrameInfo();
		}
	}
}

OverscanDimensions SnesDefaultVideoFilter::GetOverscan()
{
	OverscanDimensions overscan = BaseVideoFilter::GetOverscan();
	int overscanMultiplier = _baseFrameInfo.Width == 512 ? 2 : 1;
	overscan.Top *= overscanMultiplier;
	overscan.Bottom *= overscanMultiplier;
	overscan.Left *= overscanMultiplier;
	overscan.Right *= overscanMultiplier;
	return overscan;
}

void SnesDefaultVideoFilter::InitLookupTable()
{
	VideoConfig config = _emu->GetSettings()->GetVideoConfig();
	SnesConfig& snesConfig = _emu->GetSettings()->GetSnesConfig();

	InitConversionMatrix(config.Hue, config.Saturation);

	for(int rgb555 = 0; rgb555 < 0x8000; rgb555++) {
		uint8_t r = ColorUtilities::Convert5BitTo8Bit(rgb555 & 0x1F);
		uint8_t g = ColorUtilities::Convert5BitTo8Bit((rgb555 >> 5) & 0x1F);
		uint8_t b = ColorUtilities::Convert5BitTo8Bit((rgb555 >> 10) & 0x1F);

		if(snesConfig.ColorCorrection == SnesColorCorrectionMode::NtscBlackLevel) {
			ColorUtilities::ApplyNtscBlackLevel(r, g, b);
		} else if(snesConfig.ColorCorrection == SnesColorCorrectionMode::DeepBlackBoost) {
			ColorUtilities::ApplyDeepBlackBoost(r, g, b);
		}

		if(config.Hue != 0 || config.Saturation != 0 || config.Brightness != 0 || config.Contrast != 0) {
			ApplyColorOptions(r, g, b, config.Brightness, config.Contrast);
		}
		_calculatedPalette[rgb555] = 0xFF000000 | (r << 16) | (g << 8) | b;
	}

	_videoConfig = config;
}

void SnesDefaultVideoFilter::OnBeforeApplyFilter()
{
	VideoConfig& config = _emu->GetSettings()->GetVideoConfig();
	SnesConfig& snesConfig = _emu->GetSettings()->GetSnesConfig();

	if(_videoConfig.Hue != config.Hue || _videoConfig.Saturation != config.Saturation || _videoConfig.Contrast != config.Contrast || _videoConfig.Brightness != config.Brightness || _colorCorrection != snesConfig.ColorCorrection) {
		InitLookupTable();
	}
	_forceFixedRes = snesConfig.ForceFixedResolution;
	_highResBlendMode = snesConfig.HighResBlendMode;
	_colorCorrection = snesConfig.ColorCorrection;
	_videoConfig = config;
}

void SnesDefaultVideoFilter::ApplyFilter(uint16_t* ppuOutputBuffer)
{
	if(_emu->GetRomInfo().Format == RomFormat::Spc) {
		return;
	}

	uint32_t* out = GetOutputBuffer();
	FrameInfo frameInfo = _frameInfo;
	OverscanDimensions overscan = GetOverscan();

	uint32_t width = _baseFrameInfo.Width;
	uint32_t xOffset = overscan.Left;
	uint32_t yOffset = overscan.Top * width;
	bool blendFrames =
		_frame.Flags == FrameFlags::Interlaced &&
		!_emu->GetRewindManager()->IsRewinding() &&
		!_emu->IsPaused() &&
		_emu->GetSettings()->GetSnesConfig().DeinterlaceMode == SnesDeinterlaceMode::BobBlend;

	if(blendFrames && _needFrameClear) {
		//Clear previous frame buffer to avoid blending with an old frame (e.g if interlace mode is getting enabled/disabled by the game)
		_needFrameClear = false;
		std::fill(_prevFrame, _prevFrame + 512 * 478, 0);
	}

	if(_baseFrameInfo.Width == 256 && _forceFixedRes) {
		for(uint32_t i = 0; i < frameInfo.Height; i++) {
			for(uint32_t j = 0; j < frameInfo.Width; j++) {
				out[i * frameInfo.Width + j] = GetPixel(ppuOutputBuffer, i / 2 * width + j / 2 + yOffset + xOffset, blendFrames);
			}
		}
	} else {
		for(uint32_t i = 0; i < frameInfo.Height; i++) {
			for(uint32_t j = 0; j < frameInfo.Width; j++) {
				out[i * frameInfo.Width + j] = GetPixel(ppuOutputBuffer, i * width + j + yOffset + xOffset, blendFrames);
			}
		}
	}

	if(_baseFrameInfo.Width == 512) {
		ApplyBlend(frameInfo, out);
	}

	if(blendFrames) {
		std::copy(ppuOutputBuffer, ppuOutputBuffer + 512 * 478, _prevFrame);
	} else {
		_needFrameClear = true;
	}
}

void SnesDefaultVideoFilter::ApplyBlend(FrameInfo frameInfo, uint32_t* out)
{
	if(_highResBlendMode == SnesHighResBlendMode::None) {
		return;
	}

	if(_highResBlendMode == SnesHighResBlendMode::BlendAll) {
		//Very basic blend effect for high resolution modes
		for(uint32_t i = 0; i < frameInfo.Height; i++) {
			for(uint32_t j = 0; j < frameInfo.Width; j++) {
				uint32_t& pixel1 = out[i * frameInfo.Width + j];
				pixel1 = BlendPixels(pixel1, out[i * frameInfo.Width + j + 1]);
			}
		}
	} else {
		//Blend even-odd columns together (looks nice for fake transparency effects, but bad for high resolution text, etc.)
		for(uint32_t i = 0; i < frameInfo.Height; i++) {
			for(uint32_t j = 0; j < frameInfo.Width; j += 2) {
				uint32_t& pixel1 = out[i * frameInfo.Width + j];
				pixel1 = BlendPixels(pixel1, out[i * frameInfo.Width + j + 1]);
				out[i * frameInfo.Width + j + 1] = pixel1;
			}
		}
	}
}

uint32_t SnesDefaultVideoFilter::GetPixel(uint16_t* ppuFrame, uint32_t offset, bool blendFrames)
{
	if(blendFrames) {
		return BlendPixels(_calculatedPalette[_prevFrame[offset]], _calculatedPalette[ppuFrame[offset]]);
	} else {
		return _calculatedPalette[ppuFrame[offset]];
	}
}

uint32_t SnesDefaultVideoFilter::BlendPixels(uint32_t a, uint32_t b)
{
	return ((((a) ^ (b)) & 0xfffefefeL) >> 1) + ((a) & (b));
}
