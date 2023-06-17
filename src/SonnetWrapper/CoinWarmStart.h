// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

#pragma once

#include <CoinWarmStart.hpp>
#include "Helpers.h"

namespace COIN
{
	public ref class CoinWarmStart : WrapperBase<::CoinWarmStart>
	{
	internal:
		CoinWarmStart(::CoinWarmStart *warmStart)
			:WrapperBase(warmStart)
		{
		}
	};
}