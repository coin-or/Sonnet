// Copyright (C) 2012, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#ifndef Utils_H
#define Utils_H

#include <stdarg.h>
#include <stddef.h>
#include <string>

using namespace std;

namespace Sonnet
{
	string string_replaceAll(string result, const string &search, const string &replace)
	{
		while (1)
		{
			const int pos = result.find(search);
			if (pos == -1) break;

			result.replace(pos, search.size(), replace);
		}
		return result;
	}

	// from http://stackoverflow.com/questions/2342162/stdstring-formatting-like-sprintf
	// %i or %d for int
	// %c for char
	// %f for float
	// %lf for ouble
	// %s for string
	string string_format(const string &fmt, ...) 
	{
		int size=100;
		string str;
		va_list ap;
		while (1) 
		{
			str.resize(size);
			va_start(ap, fmt);
			int n = vsnprintf((char *)str.c_str(), size, fmt.c_str(), ap);
			va_end(ap);
			if (n > -1 && n < size) 
			{
				str.resize(n);
				return str;
			}
			if (n > -1) size=n+1;
			else size*=2;
		}
	}

	class MathUtils
	{
	public:
		static const double getInfinity()
		{
			return 10000000.0;// TODO
		}

	private:
		MathUtils() {} // DONT USE
	};
}

#endif
