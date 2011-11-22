// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CoinHelperFunctions.hpp>
#include <CoinTime.hpp>

namespace COIN
{
	public ref class CoinUtils
	{
	private:
		CoinUtils(void);

	public:
		static double* NewDoubleArray(const int size);
		static int* NewIntArray(const int size);

		static void DeleteArray(double *to);
		static void DeleteArray(int *to);

		static void CoinDisjointCopyN(const double * from, const int size, double * to);
		static void CoinZeroN(double * to, const int size);

		static void CoinDisjointCopyN(array<double>^ from, const int size, array<double> ^to);
		static void CoinZeroN(array<double>^ to, const int size);

		static double CoinCpuTime();
	};

	template<typename T>
	inline cli::interior_ptr<T> GetPinablePtr(array<T> ^t)
	{
		return (t == nullptr || t->LongLength == 0)?nullptr:&t[0];
	}
}