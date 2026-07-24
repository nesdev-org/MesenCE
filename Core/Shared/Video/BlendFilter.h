#include "pch.h"

class BlendFilter
{
private:
	uint32_t* _prevFrame = nullptr;
	uint32_t _size = 0;
	bool _enabled = false;
	uint32_t _frameNumber = 0;

	static uint32_t BlendPixels(uint32_t a, uint32_t b)
	{
		return ((((a) ^ (b)) & 0xFFFEFEFEL) >> 1) + ((a) & (b));
	}

public:
	BlendFilter() {}
	~BlendFilter()
	{
		delete[] _prevFrame;
	}

	bool IsEnabled()
	{
		return _enabled;
	}

	void SetEnabled(bool enabled)
	{
		_enabled = enabled;
	}

	void ApplyFilter(uint32_t* out, uint32_t size, uint32_t frameNumber)
	{
		if(!_enabled) {
			_frameNumber = 0;
			return;
		}

		bool needFill = _size != size || frameNumber != _frameNumber + 1;
		_frameNumber = frameNumber;

		if(_size != size) {
			delete[] _prevFrame;
			_prevFrame = new uint32_t[size];
			_size = size;
		}

		if(needFill) {
			//Copy frame buffer without blending, to avoid blending with an old frame
			std::copy(out, out + _size, _prevFrame);
		} else if(_enabled) {
			for(uint32_t i = 0; i < size; i++) {
				uint32_t old = _prevFrame[i];

				//Update copy of the previous frame
				_prevFrame[i] = out[i];

				//Blend new frame
				out[i] = BlendPixels(out[i], old);
			}
		}
	}
};