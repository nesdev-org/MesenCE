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

	template<typename T>
	__forceinline static void SetBitInArray(T array[], size_t byteCount, size_t position, bool value)
	{
		position %= byteCount * 8;
		array[position / 8] &= ~(1 << (position % 8));
		if(value) {
			array[position / 8] |= 1 << (position % 8);
		}
	}

	template<typename T>
	__forceinline static uint8_t GetBitInArray(T array[], size_t byteCount, size_t position)
	{
		return (uint8_t)((array[(position / 8) % byteCount] >> (position % 8)) & 0x01);
	}
};