#pragma once
#include <cstdint>
#include <ctime>

class TimeUtilities
{
public:
	//Returns a UTC result and updates the tm input's wday variable.
	static time_t TmToUtc(std::tm* tm)
	{
		int64_t year = tm->tm_year + 1900;
		int64_t month = tm->tm_mon;

		//Normalize months and years.
		if(month < 0 || month >= 12) {
			year += (month < 0 ? (month - 11) / 12 : month / 12); //Adjust years by however far off months is.
			month = (month % 12 + 12) % 12; //Normalize months to positive 0-11.
		}

		//We put February at the end of the year to make it easy to add 1 for leap years.
		month -= 2;
		if(month < 0) {
			year -= 1;
			month += 12;
		}

		//These don't need to be normalized; any extra will be included directly.
		int64_t day = tm->tm_mday;
		int64_t hour = tm->tm_hour;
		int64_t minute = tm->tm_min;
		int64_t second = tm->tm_sec;

		//The number of days prior to the epoch (from 0000-03-01 (because of the month adjustment) to 1970-01-01).
		static constexpr int64_t epoch_adjust = (1969 * 365) + (1969 / 4) - (1969 / 100) + (1969 / 400) + 306;

		int64_t days = (day - 1) +
			((month * 306 + 5) / 10) + //This algorithm produces the 31 and 30 pattern for the length of each month.
			(year * 365) + (year / 4) - (year / 100) + (year / 400) -
			epoch_adjust;

		//Update the weekday. 0 = Sunday. The epoch was Thursday (4).
		int wday = (int)((((days + 4) % 7) + 7) % 7);
		tm->tm_wday = wday;

		return (days * 86400) + (hour * 3600) + (minute * 60) + second;
	}
};