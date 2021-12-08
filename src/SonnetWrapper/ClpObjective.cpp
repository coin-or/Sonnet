// Copyright (C) 2011, Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "ClpObjective.h"

namespace COIN
{
	ClpObjective^ ClpObjective::CreateDerived(const ::ClpObjective* derived)
	{
		if (dynamic_cast<const ::ClpQuadraticObjective*>(derived))
		{
			return gcnew ClpQuadraticObjective((::ClpQuadraticObjective*)derived);
		}
		
		if (dynamic_cast<const ::ClpLinearObjective*>(derived))
		{
			return gcnew ClpLinearObjective((::ClpLinearObjective*)derived);
		}

		return gcnew ClpObjective(derived);
	}
}