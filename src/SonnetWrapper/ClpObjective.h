// Copyright (C) 2011, Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <ClpModel.hpp>
#include <ClpQuadraticObjective.hpp>
#include <ClpLinearObjective.hpp>
#include <CoinPackedMatrix.hpp>

#include "Helpers.h"
#include "CoinPackedMatrix.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class ClpObjective : WrapperAbstractBase<::ClpObjective>
	{
	public:
		/// <summary>
		/// Returns type (above 63 is extra information)
		/// </summary>
		/// <returns>The type of the objective as integer with 1 for linear, 2 for quadratic.</returns>
		inline int type()
		{
			return Base->type();
		}

	internal:
		ClpObjective(const ::ClpObjective* obj)
			: WrapperAbstractBase(obj)
		{
		}

		static ClpObjective ^ CreateDerived(const ::ClpObjective* derived);
	};

	template<class T>
	public ref class ClpObjectiveGeneric : ClpObjective
	{
	protected:
		ClpObjectiveGeneric()
		{
			// DONT USE COPY-CONSTRUCTOR (that would make as not-deleteBase)
			Base = new T();
		}

		ClpObjectiveGeneric(const T* base)
			: ClpObjective(base)
		{
		}

	protected:
		property T* Base
		{
			T* get()
			{
				return dynamic_cast<T*>(ClpObjective::Base);
			}
		}
	};

	public ref class ClpQuadraticObjective : public ClpObjectiveGeneric<::ClpQuadraticObjective>
	{
	public:
		/// <summary>
		/// Get the quadraticObjective
		/// </summary>
		/// <returns></returns>
		CoinPackedMatrix^ quadraticObjective()
		{
			return gcnew CoinPackedMatrix(Base->quadraticObjective());
		}

		/// <summary>
		/// If a full or half matrix
		/// </summary>
		/// <returns></returns>
		inline bool fullMatrix()
		{
			return Base->fullMatrix();
		}

	internal:
		/// <summary>
		/// Wrapping constructor
		/// </summary>
		/// <param name="obj">Base to be used</param>
		ClpQuadraticObjective(const ::ClpQuadraticObjective* obj)
			: ClpObjectiveGeneric(obj)
		{
		}
	};

	public ref class ClpLinearObjective : public ClpObjectiveGeneric<::ClpLinearObjective>
	{
	internal:
		/// <summary>
		/// Wrapping constructor
		/// </summary>
		/// <param name="obj">Base to be used</param>
		ClpLinearObjective(const ::ClpLinearObjective* obj)
			: ClpObjectiveGeneric(obj)
		{
		}
	};
}