// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CoinError.hpp>

using namespace System;

namespace COIN
{
	public ref class CoinError : System::ApplicationException
	{
	internal:
		CoinError(::CoinError &err);

	private:
		static String ^ GetMessage(::CoinError &err);
		static String ^ GetInnerMessage(::CoinError &err);
	};
}