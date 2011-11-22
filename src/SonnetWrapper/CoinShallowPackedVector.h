// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CoinShallowPackedVector.hpp>

#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CoinShallowPackedVector : WrapperBase<::CoinShallowPackedVector>
	{
	public:
		CoinShallowPackedVector() {}

		/// Get length of indices and elements vectors
		virtual int getNumElements()
		{
			return Base->getNumElements();
		}
		/// Get indices of elements
		virtual array<int>^ getIndices()
		{
			int n = Base->getNumElements();
			int *input = (int *) Base->getIndices();
			array<int> ^result = gcnew array<int>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}
		/// Get element values
		virtual array<double> ^ getElements()
		{
			int n = Base->getNumElements();
			double *input = (double *)Base->getElements();
			array<double> ^result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

	internal:
		CoinShallowPackedVector(const ::CoinShallowPackedVector *obj)
			:WrapperBase(obj)
		{
		}
	};
}