// Copyright (C) 2011, Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <ClpModel.hpp>
#include <ClpQuadraticObjective.hpp>

#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class ClpObjective : WrapperBase<::ClpObjective>
	{
	internal:
		ClpObjective(::ClpObjective* obj)
			: WrapperBase(obj)
		{
		}
	};

	public ref class ClpQuadraticObjective : ClpObjective
	{
	internal:
		ClpQuadraticObjective(::ClpQuadraticObjective* obj)
			: ClpObjective(obj)
		{
		}
	};
}