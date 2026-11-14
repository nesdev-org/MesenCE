#pragma once
#include "pch.h"
#include "TraceLogOutput.h"

#include "Utilities/Socket.h"
#include "Core/Shared/MessageManager.h"
#include "Utilities/CompressionHelper.h"

#include <memory>
#include <thread>
#include <mutex>
#include <queue>
#include <atomic>

class TraceLogOutputNetworkSocket : public TraceLogOutput
{
public:
	struct Settings
	{
		uint16_t ConnectionPort;
	};

private:
	enum class State
	{
		Uninitialized,
		Initialized,
		SocketCreated,
		Listening,
		Connected,
		Logging,
	};

	Settings _selfSettings;

	std::atomic<State> _state = State::Uninitialized;
	std::unique_ptr<Socket> _socket = nullptr;
	std::vector<std::unique_ptr<Socket>> _openConnections;

public:
	virtual ~TraceLogOutputNetworkSocket()
	{
		CloseSocket();
	}

	bool OpenSocket(const Settings& selfSettings, const TraceLogOutput::Settings& sharedSettings)
	{
		if(_state == State::Uninitialized) {
			_selfSettings = selfSettings;
			_sharedSettings = sharedSettings;

			if(!AllocateBuffers()) {
				return false;
			}

			_state = State::Initialized;
		}

		if(_state == State::Initialized) {
			_socket = std::make_unique<Socket>();

			if(_socket->ConnectionError()) {
				_socket.reset(nullptr);
				CloseSocket();
				return false;
			}

			_socket->Bind(_selfSettings.ConnectionPort);

			if(_socket->ConnectionError()) {
				_socket.reset(nullptr);
				CloseSocket();
				return false;
			}

			_state = State::SocketCreated;
		}

		if(_state == State::SocketCreated) {
			_socket->Listen(10);

			if(_socket->ConnectionError()) {
				CloseSocket();
				return false;
			}

			_state = State::Listening;
		}

		return true;
	}

	void CloseSocket()
	{
		EndConnection();

		if(_state == State::Listening) {
			_state = State::SocketCreated;
		}

		if(_state == State::SocketCreated) {
			_socket->Close();
			_socket.reset(nullptr);
			_state = State::Initialized;
		}

		if(_state == State::Initialized) {
			FreeBuffers();

			_loggingQueue = std::queue<WorkItem>();
			_compressionInputQueue = std::queue<WorkItem>();
			_compressionOutputQueue = std::queue<WorkItem>();
			_sendQueue = std::queue<WorkItem>();

			_state = State::Uninitialized;
		}
	}

	bool BeginConnection()
	{
		if(_state == State::Listening) {
			if(AcceptConnection()) {
				_state = State::Connected;
				return true;
			}
		}

		return false;
	}

	void EndConnection()
	{
		StopLogging();

		if(_state == State::Connected) {
			for(std::unique_ptr<Socket>& socket : _openConnections) {
				socket->Close();
			}

			_openConnections.clear();
			_state = State::Listening;
		}
	}

	bool StartLogging()
	{
		if(_state == State::Connected) {
			StartThreads();
			_state = State::Logging;
		}

		return true;
	}

	void StopLogging()
	{
		if(_state == State::Logging) {
			StopThreads();
			_state = State::Connected;
		}
	}

protected:
	void FlushItemToStream(const WorkItem& item) override
	{
		AcceptConnection();

		for(auto it = _openConnections.begin(); it != _openConnections.end();) {
			std::unique_ptr<Socket>& socket = *it;

			socket->Send((char*)item.Buffer, (int)item.Size, 0u);

			// Erase a connection if it produced a connection error. Probably not much else
			// we can do here. The "AcceptConnection()" above should hopefully re-establish
			// the connection if desired.
			if(socket->ConnectionError()) {
				it = _openConnections.erase(it);
			} else {
				++it;
			}
		}
	}

	bool AcceptConnection()
	{
		unique_ptr<Socket> socket = _socket->Accept();

		if(socket->ConnectionError()) {
			return false;
		}

		_openConnections.push_back(std::move(socket));
		return true;
	}
};