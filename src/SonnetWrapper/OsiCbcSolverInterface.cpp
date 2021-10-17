// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "OsiCbcSolverInterface.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	//////////////////////////////////////////////////////
	///// OsiCbcSolverInterface
	//////////////////////////////////////////////////////

	void OsiCbcSolverInterface::resetModelToReferenceSolver()
	{
		::CbcModel* cbcModel = Base->getModelPtr();

		//cbcModel->resetModel();
		cbcModel->resetToReferenceSolver();

		// TODO: this resets very well, but need to copy hints etc.
		// isnth there a better way to allow for good resolve?
		::OsiSolverInterface* tmp = cbcModel->solver();

		::CoinMessageHandler* messageHandler = Base->messageHandler();

		cbcModel->swapSolver(nullptr);
		Base = new ::OsiCbcSolverInterface(tmp);

		Base->passInMessageHandler(messageHandler);
	}

	void OsiCbcSolverInterface::saveModelReferenceSolver()
	{
		::OsiCbcSolverInterface* osiCbc = dynamic_cast<::OsiCbcSolverInterface*> (Base);
		osiCbc->getModelPtr()->saveReferenceSolver();
	}
}