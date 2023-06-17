// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

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