// Copyright (C) 2012, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#ifndef Utils_H
#define Utils_H

#include <stdarg.h>
#include <stddef.h>
#include <limits>
#include <cmath>
#include <algorithm>
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
	// %lf for double
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


	/// <summary>
    /// This class implements various static methods
    /// </summary>
    class MathUtils
    {
	public:
        static const double Infinity;
        static const double Epsilon;
        static bool UseAbsoluteComparisonOnly;
        /// <summary>
        /// Compares this double to the given value.
        /// Returns -1 if b is larger, 1 if b is smaller, and 0 otherwise.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double.</param>
        /// <param name="b">The given double.</param>
        /// <returns>-1 if this double is smaller than b - eps,
        /// 1 if a is larget than b + eps, and 0 otherwise.
        /// </returns>
        static int CompareToEps(double a, double b)
        {
            double eps = Epsilon;
            // if either a or b is close to zero, then do absolute difference
            // otherwise (the 'if' here) do a relative comparison (relative to the absolute value of the highest of a and b)
            if (!UseAbsoluteComparisonOnly &&
                fabs(a) >= Epsilon && fabs(b) >= Epsilon)
                eps *= fabs(max(a, b));

            if (a < b - eps)
                return -1;
            if (a > b + eps)
                return 1;
            return 0;
        }

        /// <summary>
        /// Determines whether this double is positive.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double</param>
        /// <returns>True iff this double is larger than zero.</returns>
        static bool IsPositive(double a)
        {
            return CompareToEps(a, 0.0) > 0;
        }

        /// <summary>
        /// Determines whether this double is negative.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double</param>
        /// <returns>True iff this double is smaller than zero.</returns>
        static bool IsNegative(double a)
        {
            return CompareToEps(a, 0.0) < 0;
        }

        /// <summary>
        /// Determines whether this double is equal to or between the given two bounds.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="x">This double.</param>
        /// <param name="l">the lowwer bound.</param>
        /// <param name="u">The upper bound.</param>
        /// <returns>True iff this double is equal to or between the two bounds.</returns>
        static bool IsBetween(double l, double x, double u)
        {
            if (CompareToEps(x, l) >= 0 && CompareToEps(x, u) <= 0) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this double is equal to zero.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double.</param>
        /// <returns>True iff this double is equal to zero.</returns>
        static bool IsZero(double a)
        {
            return CompareToEps(a, 0.0) == 0;
        }

        /// <summary>
        /// Determines whether this double is integer.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double</param>
        /// <returns>true if this double is integer, and false otherwise.</returns>
        static bool IsInteger(double a)
        {
			int intpart = (int)(( a < 0 ) ? a - 0.5 : a + 0.5);
            return IsZero(intpart - a);
        }

        /// <summary>
        /// Creates a string representation of this double.
        /// Uses "-Inf" and "Inf" whenever applicatable.
        /// </summary>
        /// <param name="a">This double.</param>
        /// <returns>"-Inf" if a is less than or equal to Utils.Infinity, "Inf" is larger or equal, and 
        /// regular a.ToString() otherwise.</returns>
        static string ToDoubleString(double a)
        {
            if (a <= -Infinity) return "-Inf";
            if (a >= Infinity) return "Inf";

            return string_format("%lf", a);
        }

	private:
		MathUtils() {} // DONT USE
	};

	const double MathUtils::Infinity = numeric_limits<double>::max();
    const double MathUtils::Epsilon = 1e-5;
    bool MathUtils::UseAbsoluteComparisonOnly = false;
}

#endif
