#pragma once

#include <fstream>

namespace utf8
{
	class utf8
	{
	public:
		static std::wstring decode(const std::string& str);
		static std::string encode(const std::wstring& wstr);
		static std::string encode(const std::u16string& wstr);
	};

#ifdef _WIN32
	class ifstream : public std::ifstream
	{
	public:
		ifstream(const std::string& _Str, ios_base::openmode _Mode = ios_base::in) : std::ifstream(utf8::decode(_Str), _Mode) {}
		ifstream() : std::ifstream() {}
		void open(const std::string& _Str, ios_base::openmode _Mode = ios_base::in) { std::ifstream::open(utf8::decode(_Str), _Mode); }
	};

	class ofstream : public std::ofstream
	{
	public:
		ofstream(const std::string& _Str, ios_base::openmode _Mode = ios_base::in) : std::ofstream(utf8::decode(_Str), _Mode) {}
		ofstream() : std::ofstream() {}
		void open(const std::string& _Str, ios_base::openmode _Mode = ios_base::in) { std::ofstream::open(utf8::decode(_Str), _Mode); }
	};
#else
	using std::ifstream;
	using std::ofstream;
#endif
}