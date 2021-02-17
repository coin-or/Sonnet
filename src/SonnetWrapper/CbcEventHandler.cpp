// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include <CbcEventHandler.hpp>

#include "CbcEventHandler.h"
#include "CbcModel.h"

namespace COIN
{
	::CbcEventHandler::CbcAction CbcDelegateEventHandlerProxy::event(::CbcEventHandler::CbcEvent whichEvent)
	{
		// this->getModel() is a cbcModel is likely a _submodel_ used by cbc, not the parent cbcModel of OsiCbc. 
		// Therefore, wrap it again.
		COIN::CbcModel^ wrapperCbcModel = gcnew COIN::CbcModel(this->getModel());
		return (::CbcEventHandler::CbcAction)wrapper->Invoke(wrapperCbcModel, (COIN::CbcEvent)whichEvent);
	}
}