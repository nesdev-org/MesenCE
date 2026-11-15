#pragma once
#include "pch.h"

#include "Core/Shared/MessageManager.h"
#include "Utilities/CompressionHelper.h"

#include <memory>
#include <thread>
#include <mutex>
#include <queue>
#include <atomic>

class TraceLogOutput
{
public:
	enum class CompressionType : uint8_t
	{
		None = 0,
		DiztinguishZlib = 1,
	};

	enum class OutputFormat : uint8_t
	{
		Text = 0,
		Diztinguish = 1,
		DiztinguishAbridged = 2,

	};

	struct Settings
	{
		uint32_t ChunkSize;
		uint32_t ChunkCount;
		bool BatchLogData;
		bool UniqueRowsOnly;
		bool IncludeImplicitPC;
		CompressionType CompressionType;
		OutputFormat Format;
	};

	bool GetUniqueRowsOnly() const
	{
		return _sharedSettings.UniqueRowsOnly;
	}

	bool GetIncludeImplictPC() const
	{
		return _sharedSettings.IncludeImplicitPC;
	}

	OutputFormat GetOutputFormat() const
	{
		return _sharedSettings.Format;
	}

	__forceinline bool IsEnabled() { return _threadsRunning; }

	void Log(const uint8_t* data, int length)
	{
		_insideLogFunction = true;

		if(!_threadsRunning) {
			_insideLogFunction = false;
			return;
		}

		if(_currentLogItem.Size + length > _currentLogItem.Capacity) {
			if(_currentLogItem.Size == 0u) {
				// If this log entry does not fit into our current buffer,
				// yet the current buffer's size is still at 0, it means the
				// log entry is larger than our full buffer size. There is
				// absolutely no way this entry will ever fit into the buffer,
				// so just discard it. Not much else we can do.
				MessageManager::Log("[Trace Log Output] Discarding log entry of size " + std::to_string(length) + " because it exceeds the maximum buffer size.");
				_insideLogFunction = false;
				return;
			}

			FlushCurrentWorkItem();
		}

		std::memcpy(_currentLogItem.Buffer + _currentLogItem.Size, data, length);
		_currentLogItem.Size += length;

		if(!_sharedSettings.BatchLogData) {
			FlushCurrentWorkItem();
		}

		_insideLogFunction = false;
	}

	void UpdateLastLogTimestamp()
	{
		if(_threadsRunning) {
			return;
		}

		_lastLogTime = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
	}

	void FlushOldBuffers()
	{
		if(_threadsRunning) {
			return;
		}

		if(_currentLogItem.Size == 0u) {
			return;
		}

		uint64_t currentTime = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();

		if(currentTime - _lastLogTime >= 5000u) {
			FlushCurrentWorkItem();
		}
	}

protected:
	Settings _sharedSettings;

	// A simple explanation of the general functionality here:
	// - The main thread appends all traces to a ChunkSize-sized buffer from the logging
	//   queue when calling Log().
	// - Once the buffer is full (or, depending on the setting, immediately), that buffer
	//   is dequeued and pushed into the compression queue.
	// - A second thread dequeues a buffer from the compression queue and compresses it
	//   into a CompressedChunkSize-sized buffer using the compression of choice.
	// - The original buffer is returned to the logging queue, and the compressed buffer
	//   pushed to the send queue.
	// - A third thread dequeues a buffer from the send queue, write it to the configured
	//   target output stream, and pushes it back into the compression queue.
	// Ultimately, the goal of all this complexity is to improve performance by moving as
	// much work as possible onto secondary threads so that the main thread spends as little
	// time as possible on the logging functionality.

	struct WorkItem
	{
		uint8_t* Buffer;
		size_t Capacity;
		size_t Size;

		WorkItem()
		{
			Buffer = nullptr;
			Capacity = 0u;
			Size = 0u;
		}
	};

	bool _threadsRunning = false;

	uint8_t* _logMemoryStart = nullptr;
	uint8_t* _compressionMemoryStart = nullptr;

	WorkItem _currentLogItem;
	uint64_t _lastLogTime = 0u;
	std::queue<WorkItem> _loggingQueue;
	std::queue<WorkItem> _compressionInputQueue;
	std::queue<WorkItem> _compressionOutputQueue;
	std::queue<WorkItem> _sendQueue;

	// This bool is a (probably bad) attempt at preventing a race condition
	// that can happen during shutdown because Log() and StopLogging() are
	// called from different threads. Because Log() is a performance-critical
	// function, I don't want to protect it with a mutex.
	volatile bool _insideLogFunction = false;
	bool _compressionThreadQuitRequested = false;
	bool _sendThreadQuitRequested = false;
	std::thread _compressionThread;
	std::thread _sendThread;
	std::mutex _compressionInputMutex;
	std::mutex _compressionOutputMutex;

	struct DiztinguishZlibCompressionHeader
	{
		// 0:     uint8_t:  Character 'Z' as a flag for compressed data.
		// 1 - 4: uint32_t: Length of the uncompressed buffer, little-endian.
		// 5 - 8: uint32_t: Length of the compressed buffer, little-endian.
		uint8_t data[9u];
	};

	bool AllocateBuffers()
	{
		// Just a wild guess as to how much memory the compressed data will need at most.
		// In the average, most likely case, the compressed buffer will need a lot less
		// memory than the input buffers, but this should prepare us for the worst case
		// without needing to constantly reallocate memory.
		const size_t CompressedChunkSize = (size_t)((float)_sharedSettings.ChunkSize * 1.3f);

		_logMemoryStart = new uint8_t[_sharedSettings.ChunkSize * _sharedSettings.ChunkCount];

		if(_logMemoryStart == nullptr) {
			return false;
		}

		_compressionMemoryStart = new uint8_t[CompressedChunkSize * _sharedSettings.ChunkCount];

		if(_compressionMemoryStart == nullptr) {
			delete _logMemoryStart;
			return false;
		}

		_loggingQueue = std::queue<WorkItem>();
		_compressionInputQueue = std::queue<WorkItem>();
		_compressionOutputQueue = std::queue<WorkItem>();
		_sendQueue = std::queue<WorkItem>();

		for(size_t i = 0; i < _sharedSettings.ChunkCount; ++i) {
			WorkItem workItem;
			workItem.Buffer = _logMemoryStart + (i * _sharedSettings.ChunkSize);
			workItem.Capacity = _sharedSettings.ChunkSize;
			workItem.Size = 0u;
			_loggingQueue.push(workItem);
		}

		for(size_t i = 0; i < _sharedSettings.ChunkCount; ++i) {
			WorkItem workItem;
			workItem.Buffer = _compressionMemoryStart + (i * CompressedChunkSize);
			workItem.Capacity = CompressedChunkSize;
			workItem.Size = 0u;
			_compressionOutputQueue.push(workItem);
		}

		return true;
	}

	void FreeBuffers()
	{
		if(_compressionMemoryStart != nullptr) {
			delete _compressionMemoryStart;
			_compressionMemoryStart = nullptr;
		}

		if(_logMemoryStart != nullptr) {
			delete _logMemoryStart;
			_logMemoryStart = nullptr;
		}
	}

	void StartThreads()
	{
		if(_threadsRunning) {
			return;
		}

		_insideLogFunction = false;

		_compressionThreadQuitRequested = false;
		_sendThreadQuitRequested = false;

		_currentLogItem = _loggingQueue.front();
		_loggingQueue.pop();

		_compressionThread = std::thread([this]() { RunCompression(); });
		_sendThread = std::thread([this]() { RunSend(); });

		_lastLogTime = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();

		_threadsRunning = true;
	}

	void StopThreads()
	{
		if(!_threadsRunning) {
			return;
		}

		// Because of a possible race condition with the Log() function, we set the flag right
		// here at the start of the function and then spin-lock until the Log() function is done.
		// This way we make sure it can't be called again while our shutdown is active. Adding
		// a mutex here would probably be bad for performance, since Log() gets called thousands
		// of times within a frame.
		_threadsRunning = false;

		while(_insideLogFunction) {
			std::this_thread::sleep_for(5ms);
		}

		{
			std::lock_guard<std::mutex> guard(_compressionInputMutex);

			if(_currentLogItem.Size > 0) {
				_compressionInputQueue.push(_currentLogItem);
			} else {
				_loggingQueue.push(_currentLogItem);
			}

			_currentLogItem.Buffer = nullptr;
		}

		_compressionThreadQuitRequested = true;
		_compressionThread.join();
		_sendThreadQuitRequested = true;
		_sendThread.join();
	}

	void FlushCurrentWorkItem()
	{
		{
			std::lock_guard<std::mutex> guard(_compressionInputMutex);

			_compressionInputQueue.push(_currentLogItem);
			_currentLogItem.Buffer = nullptr;

			if(!_loggingQueue.empty()) {
				_currentLogItem = _loggingQueue.front();
				_loggingQueue.pop();
			}
		}

		// In the unlikely case we ever run out of buffers, sleep until one becomes available.
		while(_currentLogItem.Buffer == nullptr) {
			std::this_thread::sleep_for(25ms);

			{
				std::lock_guard<std::mutex> guard(_compressionInputMutex);

				if(!_loggingQueue.empty()) {
					_currentLogItem = _loggingQueue.front();
					_loggingQueue.pop();
				}
			}
		}
	}

	void RunCompression()
	{
		while(!_compressionThreadQuitRequested) {
			FlushCompressionQueue();
			std::this_thread::sleep_for(25ms);
		}

		FlushCompressionQueue();
	}

	void FlushCompressionQueue()
	{
		WorkItem currentItem;
		FetchNextCompressionItem(&currentItem);

		while(currentItem.Buffer != nullptr) {
			WorkItem outputItem;
			outputItem.Buffer = nullptr;

			while(outputItem.Buffer == nullptr) {
				{
					std::lock_guard<std::mutex> guard(_compressionOutputMutex);

					if(!_compressionOutputQueue.empty()) {
						outputItem = _compressionOutputQueue.front();
						_compressionOutputQueue.pop();
					}
				}

				if(outputItem.Buffer == nullptr) {
					std::this_thread::sleep_for(25ms);
				}
			}

			switch(_sharedSettings.CompressionType) {
				case CompressionType::None:
					memcpy(outputItem.Buffer, currentItem.Buffer, currentItem.Size);
					outputItem.Size = currentItem.Size;

					{
						std::lock_guard<std::mutex> guard(_compressionInputMutex);

						currentItem.Size = 0u;
						_loggingQueue.push(currentItem);
					}

					{
						std::lock_guard<std::mutex> guard(_compressionOutputMutex);

						_sendQueue.push(outputItem);
					}

					break;

				case CompressionType::DiztinguishZlib: {
					DiztinguishZlibCompressionHeader header;
					header.data[0] = 'Z';
					header.data[1] = (currentItem.Size >> 0) & 0xFF;
					header.data[2] = (currentItem.Size >> 8) & 0xFF;
					header.data[3] = (currentItem.Size >> 16) & 0xFF;
					header.data[4] = (currentItem.Size >> 24) & 0xFF;

					outputItem.Size = 9;

					mz_ulong compressedSize = (mz_ulong)(outputItem.Capacity - outputItem.Size);
					int z_result = compress(outputItem.Buffer + outputItem.Size, &compressedSize, currentItem.Buffer, (mz_ulong)currentItem.Size);

					{
						std::lock_guard<std::mutex> guard(_compressionInputMutex);

						currentItem.Size = 0u;
						_loggingQueue.push(currentItem);
					}

					if(z_result != Z_MEM_ERROR && z_result != Z_BUF_ERROR) {
						outputItem.Size += compressedSize;

						header.data[5] = (compressedSize >> 0) & 0xFF;
						header.data[6] = (compressedSize >> 8) & 0xFF;
						header.data[7] = (compressedSize >> 16) & 0xFF;
						header.data[8] = (compressedSize >> 24) & 0xFF;

						memcpy(outputItem.Buffer, &header, sizeof(header));

						{
							std::lock_guard<std::mutex> guard(_compressionOutputMutex);

							_sendQueue.push(outputItem);
						}
					} else {
						// If there was some compression error, we just discard the result.
						// The most likely cause is the output buffer being too small.
						outputItem.Size = 0u;

						std::lock_guard<std::mutex> guard(_compressionOutputMutex);

						_compressionOutputQueue.push(outputItem);
					}
					break;
				}
			}

			FetchNextCompressionItem(&currentItem);
		}
	}

	void FetchNextCompressionItem(WorkItem* targetItem)
	{
		targetItem->Buffer = nullptr;

		std::lock_guard<std::mutex> guard(_compressionInputMutex);

		if(!_compressionInputQueue.empty()) {
			*targetItem = _compressionInputQueue.front();
			_compressionInputQueue.pop();
		}
	}

	void RunSend()
	{
		while(!_sendThreadQuitRequested) {
			FlushSendQueue();
			std::this_thread::sleep_for(25ms);
		}

		FlushSendQueue();
	}

	void FlushSendQueue()
	{
		WorkItem currentItem;
		FetchNextSendItem(&currentItem);

		while(currentItem.Buffer != nullptr) {
			FlushItemToStream(currentItem);

			{
				currentItem.Size = 0u;

				std::lock_guard<std::mutex> guard(_compressionOutputMutex);
				_compressionOutputQueue.push(currentItem);
			}

			FetchNextSendItem(&currentItem);
		}
	}

	void FetchNextSendItem(WorkItem* targetItem)
	{
		targetItem->Buffer = nullptr;

		std::lock_guard<std::mutex> guard(_compressionOutputMutex);

		if(!_sendQueue.empty()) {
			*targetItem = _sendQueue.front();
			_sendQueue.pop();
		}
	}

	virtual void FlushItemToStream(const WorkItem& item) {}
};