// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include <CglProbing.hpp>

#include "CglCutGenerator.h"
#include "CglProbing.h"

namespace COIN
{
	CglCutGenerator ^ CglCutGenerator::CreateDerived(::CglCutGenerator *derived)
	{		
		if (dynamic_cast<::CglProbing *>(derived) != nullptr)
		{
			CglCutGenerator ^ result = gcnew CglProbing();
			result->Base = (::CglProbing *)(derived);
			result->TransferBase();
			return result;
		}
		else
		{
			CglCutGenerator ^ result = gcnew CglCutGenerator();
			result->Base = (::CglCutGenerator *)(derived);
			result->TransferBase();
			return result;
		}
		throw gcnew ArgumentException(L"Unknown CglCutGenerator", gcnew String(typeid(derived).name())); 
	}
}