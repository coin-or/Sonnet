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
	internal:
		ClpObjective(const ::ClpObjective* obj)
			: WrapperAbstractBase(obj)
		{
		}
	};

	public ref class ClpQuadraticObjective : public ClpObjective
	{
	public:
		/// <summary>
		/// Get the quadraticObjective of the base
		/// </summary>
		/// <returns></returns>
		CoinPackedMatrix^ quadraticObjective()
		{
			return gcnew CoinPackedMatrix(Base->quadraticObjective());
		}

	protected:
		/// <summary>
		/// Wrapping constructor
		/// </summary>
		/// <param name="obj">Base to be used</param>
		ClpQuadraticObjective(const ::ClpQuadraticObjective* obj)
			: ClpObjective(obj)
		{
		}

	internal:
		/// <summary>
		/// Overloaded Base property
		/// </summary>
		property ::ClpQuadraticObjective* Base
		{
			::ClpQuadraticObjective* get()
			{
				return dynamic_cast<::ClpQuadraticObjective*>(ClpObjective::Base);
			}
		}
	};

	public ref class ClpLinearObjective : public ClpObjective
	{
	internal:
		ClpLinearObjective(const ::ClpLinearObjective* obj)
			: ClpObjective(obj)
		{
		}
	};
}