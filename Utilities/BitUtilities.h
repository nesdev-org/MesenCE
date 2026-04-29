#pragma once
#include "pch.h"

class BitUtilities
{
public:
	template<uint8_t bitNumber, typename T>
	__forceinline static void SetBits(T& dst, uint8_t src)
	{
		dst = (dst & ~(0xFF << bitNumber)) | (src << bitNumber);
	}

	template<uint8_t bitNumber, typename T>
	__forceinline static uint8_t GetBits(T value)
	{
		return (uint8_t)(value >> bitNumber);
	}

	//mask must not equal 0.
	__forceinline static uint32_t GetLowestBitIndex(uint32_t mask)
	{
#ifdef _MSC_VER
		unsigned long index;
		_BitScanForward(&index, mask);
		return (uint32_t)index;
#else
		return (uint32_t)__builtin_ctz(mask);
#endif
	}

	//mask must not equal 0.
	__forceinline static uint32_t GetHighestBitIndex(uint32_t mask)
	{
#ifdef _MSC_VER
		unsigned long index;
		_BitScanReverse(&index, mask);
		return (uint32_t)index;
#else
		// __builtin_clz returns leading zeros from the top (bit 31).
		// To get the index (0-31), we subtract from 31.
		return 31 - (uint32_t)__builtin_clz(mask);
#endif
	}

	__forceinline static uint8_t ReverseByte(uint8_t b)
	{
		b = (b & 0xF0) >> 4 | (b & 0x0F) << 4; // Swap nybbles
		b = (b & 0xCC) >> 2 | (b & 0x33) << 2; // Swap quarters
		b = (b & 0xAA) >> 1 | (b & 0x55) << 1; // Swap bits
		return b;
	}
};