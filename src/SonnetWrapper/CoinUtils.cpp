// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

#include "CoinUtils.h"
#include "Helpers.h"

namespace COIN
{
	CoinUtils::CoinUtils(void)
	{
	}

	void CoinUtils::CoinDisjointCopyN(const double *from, const int size, double *to)
	{
		::CoinDisjointCopyN<double>(from, size, to);
	}
	void CoinUtils::CoinDisjointCopyN(array<double>^ from, const int size, array<double> ^to)
	{
		pin_ptr<double> fromPinned = GetPinablePtr(from);
		pin_ptr<double> toPinned = GetPinablePtr(to);
		::CoinDisjointCopyN<double>(fromPinned, size, toPinned);
	}
	void CoinUtils::CoinZeroN(double *to, const int size)
	{
		::CoinZeroN<double>(to, size);
	}
	void CoinUtils::CoinZeroN(array<double>^ to, const int size)
	{
		pin_ptr<double> toPinned = GetPinablePtr(to);
		::CoinZeroN<double>(toPinned, size);
	}

	double *CoinUtils::NewDoubleArray(const int size)
	{
		return new double[size];
	}

	int *CoinUtils::NewIntArray(const int size)
	{
		return new int[size];
	}

	void CoinUtils::DeleteArray(double *to)
	{
		delete []to;
	}

	void CoinUtils::DeleteArray(int *to)
	{
		delete []to;
	}

	double CoinUtils::CoinCpuTime()
	{
		return ::CoinCpuTime();
	}
}