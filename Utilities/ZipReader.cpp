#include "pch.h"
#include <string.h>
#include <sstream>
#include "ZipReader.h"
#include "EncodingUtilities.h"

ZipReader::ZipReader()
{
	memset(&_zipArchive, 0, sizeof(mz_zip_archive));
}

ZipReader::~ZipReader()
{
	if(_initialized) {
		mz_zip_reader_end(&_zipArchive);
	}
}

bool ZipReader::InternalLoadArchive(void* buffer, size_t size)
{
	if(_initialized) {
		mz_zip_reader_end(&_zipArchive);
		memset(&_zipArchive, 0, sizeof(mz_zip_archive));
		_initialized = false;
	}

	return mz_zip_reader_init_mem(&_zipArchive, buffer, size, 0) != 0;
}

vector<string> ZipReader::InternalGetFileList()
{
	vector<string> fileList;
	if(_initialized) {
		for(int i = 0, len = (int)mz_zip_reader_get_num_files(&_zipArchive); i < len; i++) {
			mz_zip_archive_file_stat file_stat;
			if(!mz_zip_reader_file_stat(&_zipArchive, i, &file_stat)) {
				std::cout << "mz_zip_reader_file_stat() failed!" << std::endl;
			}

			//ZIP archives created on Chinese (CP936/GBK) systems store filenames as
			//raw GBK bytes without the UTF-8 flag.  Re-encode those names to UTF-8 so
			//they display correctly instead of turning into "锟斤拷" mojibake.
			fileList.push_back(EncodingUtilities::ToUtf8(file_stat.m_filename));
		}
	}
	return fileList;
}

bool ZipReader::ExtractFile(string filename, vector<uint8_t>& output)
{
	if(_initialized) {
		size_t uncompSize = 0;

		//Fast path: the requested name matches the bytes stored in the archive
		//(plain ASCII entries, or entries that were saved with the UTF-8 flag).
		void* p = mz_zip_reader_extract_file_to_heap(&_zipArchive, filename.c_str(), &uncompSize, 0);

		if(!p) {
			//Fallback: the displayed filename was converted from GBK to UTF-8, so it
			//no longer matches the raw bytes stored in the archive.  Match the entry
			//by normalizing each stored filename the same way and extract by index.
			for(int i = 0, len = (int)mz_zip_reader_get_num_files(&_zipArchive); i < len; i++) {
				mz_zip_archive_file_stat file_stat;
				if(!mz_zip_reader_file_stat(&_zipArchive, i, &file_stat)) {
					continue;
				}
				if(EncodingUtilities::ToUtf8(file_stat.m_filename) == filename) {
					p = mz_zip_reader_extract_to_heap(&_zipArchive, i, &uncompSize, 0);
					break;
				}
			}
		}

		if(!p) {
			return false;
		}

		output = vector<uint8_t>((uint8_t*)p, (uint8_t*)p + uncompSize);

		mz_free(p);

		return true;
	}

	return false;
}