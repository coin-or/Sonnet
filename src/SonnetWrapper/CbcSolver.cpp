// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "CbcSolver.h"

namespace COIN
{
	static int NativeCallBackProxy(::CbcModel* model, int whereFrom)
	{
		if (COIN::CbcSolver::CallBack != nullptr)
		{
			COIN::CbcModel^ wrapperCbcModel = gcnew COIN::CbcModel(model);
			return COIN::CbcSolver::CallBack->Invoke(wrapperCbcModel, whereFrom);
		}

		return 0;
	}
}