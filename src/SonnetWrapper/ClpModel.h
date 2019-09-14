// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <ClpModel.hpp>
#include <ClpSimplex.hpp>

#include "CoinError.h"
#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices; // Marshall

namespace COIN
{
	public ref class ClpModel : WrapperBase<::ClpModel>
	{
	internal:
		ClpModel(::ClpModel* obj)
			: WrapperBase(obj)
		{
		}

	public:
		ClpModel() { }

		void modifyCoefficient(int row, int column, double newElement)
		{
			Base->modifyCoefficient(row, column, newElement);
		}

		void modifyCoefficient(int row, int column, double newElement, bool keepZero)
		{
			Base->modifyCoefficient(row, column, newElement, keepZero);
		}

	};

	// This class is merely an intermediate class for all derived classes
	template <class T>
	public ref class ClpModelGeneric abstract : public ClpModel
	{
	public:
		ClpModelGeneric()
		{
			try
			{
				// Create the derived object (ClpSimplexe, etc) that will be Base.
				Base = new T();
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}

	protected:
		ClpModelGeneric(T* base)
			:ClpModel(base)
		{
		}

	internal:
		property T* Base
		{
			T* get()
			{
				return static_cast<T*>(ClpModel::Base);
			}
		}
	};

	public ref class ClpSimplex : ClpModelGeneric<::ClpModel>
	{
	public:
		ClpSimplex()
		{
			// base class ClpModelGeneric takes care of constructing ::base.
		}

		ClpSimplex(::ClpSimplex* base)
			:ClpModelGeneric(base)
		{
		}
	};
}