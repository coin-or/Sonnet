// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once


#include <CglCutGenerator.hpp>
#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{	
	public ref class CglCutGenerator : WrapperAbstractBase<::CglCutGenerator>
	{
	public:
		static CglCutGenerator ^ CreateDerived(::CglCutGenerator *derived);
	};

	template<class T>
	public ref class CglCutGeneratorGeneric : CglCutGenerator
	{
	public:
		CglCutGeneratorGeneric()
		{
			Base = new T();
		}

	protected:
		property T * Base 
		{
			T * get() 
			{ 
				return dynamic_cast<T*>(CglCutGenerator::Base); 
			} 
		}
	};
}