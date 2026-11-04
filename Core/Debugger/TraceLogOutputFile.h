#pragma once
#include "pch.h"
#include "TraceLogOutput.h"

class TraceLogOutputFile : public TraceLogOutput
{
public:
	struct Settings
	{
		// This is coming directly from C# code. I don't know
		// how long this memory is guaranteed to be valid, which
		// might be a problem if we ever want to access the filename
		// after copying the struct over.
		const char* FileName;
		bool NativeNewLines;
	};

private:
	Settings _selfSettings;

	ofstream _outputFile;

public:
	virtual ~TraceLogOutputFile()
	{
		StopLogging();
	}

	bool StartLogging(const Settings& selfSettings, const TraceLogOutput::Settings& sharedSettings)
	{
		_selfSettings = selfSettings;
		_sharedSettings = sharedSettings;

		std::ios_base::openmode openMode = ios::out;

		if(_sharedSettings.Format != OutputFormat::Text || !_selfSettings.NativeNewLines) {
			openMode |= ios::binary;
		}

		_outputFile.open(_selfSettings.FileName, openMode);

		if(!_outputFile.is_open()) {
			return false;
		}

		if(!AllocateBuffers()) {
			_outputFile.close();
			return false;
		}

		StartThreads();

		return true;
	}

	void StopLogging()
	{
		if(!IsEnabled()) {
			return;
		}

		StopThreads();
		FreeBuffers();

		_outputFile.close();
	}

	void FlushItemToStream(const WorkItem& item) override
	{
		_outputFile.write((const char*)item.Buffer, item.Size);
	}
};