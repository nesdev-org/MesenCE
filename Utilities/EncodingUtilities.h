#pragma once

// Lightweight encoding helpers used to correctly decode filenames stored inside
// archives (zip/7z).  ZIP files created on Chinese (and other non-UTF-8) systems
// store their filenames as raw CP936/GBK bytes without setting the UTF-8 flag.
// When those raw bytes are later interpreted as UTF-8 they turn into the familiar
// "锟斤拷" mojibake.  These helpers detect that situation and re-encode the name
// from GBK to UTF-8 so the game name is displayed correctly.

#include <string>
#include <cstdint>

#ifdef _WIN32
	#ifndef WIN32_LEAN_AND_MEAN
		#define WIN32_LEAN_AND_MEAN
	#endif
	#include <Windows.h>
#else
	#include <iconv.h>
#endif

namespace EncodingUtilities
{
	// Strict UTF-8 validator.  Returns false as soon as an invalid/overlong/
	// surrogate/out-of-range sequence is encountered.
	inline bool IsValidUtf8(const std::string& s)
	{
		size_t i = 0;
		size_t n = s.size();
		while(i < n) {
			uint8_t b = (uint8_t)s[i];
			if(b <= 0x7F) {
				i++;
				continue;
			}

			int extra;
			uint32_t cp;
			if((b & 0xE0) == 0xC0) {
				extra = 1;
				cp = b & 0x1F;
			} else if((b & 0xF0) == 0xE0) {
				extra = 2;
				cp = b & 0x0F;
			} else if((b & 0xF8) == 0xF0) {
				extra = 3;
				cp = b & 0x07;
			} else {
				return false;
			}

			if(i + (size_t)extra >= n) {
				return false;
			}

			for(int k = 1; k <= extra; k++) {
				uint8_t cb = (uint8_t)s[i + k];
				if((cb & 0xC0) != 0x80) {
					return false;
				}
				cp = (cp << 6) | (cb & 0x3F);
			}

			if(extra == 1 && cp < 0x80) {
				return false;
			}
			if(extra == 2 && cp < 0x800) {
				return false;
			}
			if(extra == 2 && cp >= 0xD800 && cp <= 0xDFFF) {
				return false;
			}
			if(extra == 3 && (cp < 0x10000 || cp > 0x10FFFF)) {
				return false;
			}

			i += (size_t)(1 + extra);
		}
		return true;
	}

	// Converts a CP936/GBK-encoded string to UTF-8.  Falls back to the original
	// string if the conversion is unavailable for any reason.
	inline std::string GbkToUtf8(const std::string& s)
	{
		if(s.empty()) {
			return s;
		}

#ifdef _WIN32
		int wlen = MultiByteToWideChar(936, 0, s.c_str(), (int)s.size(), nullptr, 0);
		if(wlen <= 0) {
			return s;
		}
		std::wstring w(wlen, L'\0');
		MultiByteToWideChar(936, 0, s.c_str(), (int)s.size(), &w[0], wlen);

		int ulen = WideCharToMultiByte(CP_UTF8, 0, w.c_str(), (int)w.size(), nullptr, 0, nullptr, nullptr);
		if(ulen <= 0) {
			return s;
		}
		std::string out(ulen, '\0');
		WideCharToMultiByte(CP_UTF8, 0, w.c_str(), (int)w.size(), &out[0], ulen, nullptr, nullptr);
		return out;
#else
		iconv_t cd = iconv_open("UTF-8", "GBK");
		if(cd == (iconv_t)-1) {
			return s;
		}

		size_t inBytesLeft = s.size();
		char* inBuf = const_cast<char*>(s.c_str());

		size_t outCapacity = s.size() * 4 + 4;
		std::string out(outCapacity, '\0');
		char* outBuf = &out[0];
		size_t outBytesLeft = outCapacity;

		std::string result = s;
		if(iconv(cd, &inBuf, &inBytesLeft, &outBuf, &outBytesLeft) != (size_t)-1) {
			out.resize(outCapacity - outBytesLeft);
			result = out;
		}
		iconv_close(cd);
		return result;
#endif
	}

	// Returns the string as UTF-8.  Strings that are already valid UTF-8 (this
	// includes plain ASCII) are returned unchanged; everything else is assumed to
	// be GBK/CP936 and re-encoded.
	inline std::string ToUtf8(const std::string& s)
	{
		if(IsValidUtf8(s)) {
			return s;
		}
		return GbkToUtf8(s);
	}
}
