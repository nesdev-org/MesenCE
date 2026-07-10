#pragma once
#include "Utilities/Timer.h"
#include "Utilities/PlatformUtilities.h"

class FrameLimiter
{
private:
	Timer _clockTimer;
	double _targetTime;
	double _delay;
	bool _resetRunTimers;

public:
	FrameLimiter(double delay)
	{
		_delay = delay;
		_targetTime = _delay;
		_resetRunTimers = false;
	}

	void SetDelay(double delay)
	{
		_delay = delay;
		_resetRunTimers = true;
	}

	void ProcessFrame()
	{
		if(_resetRunTimers || (_clockTimer.GetElapsedMS() - _targetTime) > 100) {
			//Reset the timers, this can happen in 3 scenarios:
			//1) Target frame rate changed
			//2) The console was reset/power cycled or the emulation was paused (with or without the debugger)
			//3) As a satefy net, if we overshoot our target by over 100 milliseconds, the timer is reset, too.
			//   This can happen when something slows the emulator down severely (or when breaking execution in VS when debugging Mesen itself, etc.)
			_clockTimer.Reset();
			_targetTime = 0;
			_resetRunTimers = false;
		}

		_targetTime += _delay;
	}

	bool WaitForNextFrame(bool useSpinWait)
	{
		int gap = (int)(_targetTime - _clockTimer.GetElapsedMS());
		if(gap > 50) {
			//When sleeping for a long time (e.g <= 25% speed), sleep in small chunks and check to see if we need to stop sleeping between each sleep call
			std::this_thread::sleep_for(std::chrono::duration<int, std::milli>(40));
			return true;
		}

		if(useSpinWait) {
			if(gap >= 2) {
				//2+ms left to wait, sleep until 1ms before the thread is meant to run
				std::this_thread::sleep_for(std::chrono::duration<int, std::milli>(gap - 1));
			}

			while(_clockTimer.GetElapsedMS() < _targetTime) {
				//Spin wait until the exact time, to improve frame pacing
				PlatformUtilities::IdleLoop();
			}
		} else {
			std::this_thread::sleep_for(std::chrono::duration<int, std::milli>(gap));
		}

		return false;
	}
};