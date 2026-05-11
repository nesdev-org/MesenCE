#pragma once
#include "pch.h"
#include "Utilities/Audio/HermiteResampler.h"

struct WavHeader
{
	bool Valid;
	uint32_t HeaderSize;
	uint32_t FmtSize;
	uint32_t DataSize;
	uint32_t SampleRate;
	uint32_t BitsPerSample;
	uint32_t ChannelCount;
};

class WavReader
{
private:
	uint8_t* _fileData = nullptr;

	uint32_t _fileOffset = 0;
	uint32_t _fileSize = 0;
	uint32_t _dataStartOffset = 0;

	bool _done = true;

	HermiteResampler _resampler;

	uint32_t _fileSampleRate = 0;

	WavReader();

public:
	static unique_ptr<WavReader> Create(uint8_t* wavData, uint32_t length);
	~WavReader();

	static WavHeader GetHeader(uint8_t* wavData, uint32_t dataLength, uint32_t fileLength);

	void Play(uint32_t startSample);
	bool IsPlaybackOver();
	void ApplySamples(int16_t* buffer, size_t sampleCount, uint32_t sampleRate);
	int32_t GetPosition();
	uint32_t GetSampleRate();
};
