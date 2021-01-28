// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

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