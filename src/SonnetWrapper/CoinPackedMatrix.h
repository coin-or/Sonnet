// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

#pragma once

#include "CoinShallowPackedVector.h"
#include "Helpers.h"

#include <CoinPackedMatrix.hpp>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	//////////////////////////////////////////////////////
	///// CoinPackedMatrix
	public ref class CoinPackedMatrix : WrapperBase<::CoinPackedMatrix>
	{
	public:
		CoinPackedMatrix() {}

#ifndef CLP_NO_VECTOR  
		/** Return the i'th vector in matrix. */
		CoinShallowPackedVector ^ getVector(int i)
		{
			::CoinShallowPackedVector *tmp = new ::CoinShallowPackedVector(Base->getVector(i));
			CoinShallowPackedVector ^result = gcnew CoinShallowPackedVector(tmp);
			result->deleteBase = true;
			return result;
		}
#endif

	internal:
		CoinPackedMatrix(const ::CoinPackedMatrix *obj)
			: WrapperBase(obj)
		{
		}
	};
}