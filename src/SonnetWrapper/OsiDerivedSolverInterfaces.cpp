// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "OsiDerivedSolverInterfaces.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	//////////////////////////////////////////////////////
	///// OsiClpSolverInterface
	//////////////////////////////////////////////////////

#ifdef SONNET_LEANLOADPROBLEM
	void OsiClpSolverInterface::LeanLoadProblemInit(int n, int m, int nz, 
		int*&Cst, int *&Clg, int *&Rnr, double *&Elm, 
		double*&l, double*&u, double *& c, double*&bl, double*&bu)
	{
		::OsiClpSolverInterface *osiClp = dynamic_cast<::OsiClpSolverInterface *> (Base);
		::ClpSimplex *model = osiClp->getModelPtr();
		model->resize(m, n);			// yes, (#rows, #cols)

		Elm = new double[nz];			// The nonzero elements
		Rnr = new int[nz];				// The constraint index number per nonzero element
		Cst = new int[n + 1];				// per variable, the starting position of its nonzero data
		Clg = new int[n];				// per variable, the number of nonzeros in its column
		c =	model->objective();			// per variable, the objective function coefficient
		l = model->columnLower();
		u = model->columnUpper();
		bl = model->rowLower();
		bu = model->rowUpper();
	}

	void OsiClpSolverInterface::LeanLoadProblem(int n, int m, int nz, 
		int*&Cst, int *&Clg, int *&Rnr, double *&Elm, 
		double*&l, double*&u, double *& c, double*&bl, double*&bu)
	{
		::OsiClpSolverInterface *osiClp = dynamic_cast<::OsiClpSolverInterface *> (Base);
		::ClpSimplex *model = osiClp->getModelPtr();

		::CoinPackedMatrix* matrix = new ::CoinPackedMatrix(true, 0.0, 0.0);
		matrix->assignMatrix(true, m, n, nz, Elm, Rnr, Cst, Clg);
		::ClpPackedMatrix* clpMatrix = new ::ClpPackedMatrix(matrix);
		model->replaceMatrix(clpMatrix, true);

		osiClp->loadCurrentProblem();
	}

	// JWG: added to enable lean loading of model (via resize & assignmatrix)
	// This code is for the native C++ OsiClp (not in SonnetWrapper)
	// See other loadProblem implementations above, but without the actual loading..
	void OsiClpSolverInterface::loadCurrentProblem()
	{
	  // Get rid of integer information (modelPtr will get rid of its copy)
	  delete [] integerInformation_;
	  integerInformation_=NULL;
	  linearObjective_ = modelPtr_->objective();
	  freeCachedResults();
	  basis_=CoinWarmStartBasis();
	  if (ws_) {
		 delete ws_;
		 ws_ = 0;
	  }
	}
#endif

	//////////////////////////////////////////////////////
	///// OsiCbcSolverInterface
	//////////////////////////////////////////////////////

	void OsiCbcSolverInterface::resetModelToReferenceSolver()
	{
		::CbcModel *cbcModel = Base->getModelPtr();
		
		cbcModel->resetToReferenceSolver();

		// TODO: this resets very well, but need to copy hints etc.
		// isnth there a better way to allow for good resolve?
		::OsiSolverInterface *tmp = cbcModel->solver();
		
		::CoinMessageHandler *messageHandler = Base->messageHandler();
		
		cbcModel->swapSolver(nullptr);
		Base = new ::OsiCbcSolverInterface(tmp);

		Base->passInMessageHandler(messageHandler);
	}

	void OsiCbcSolverInterface::saveModelReferenceSolver()
	{
		::OsiCbcSolverInterface *osiCbc = dynamic_cast<::OsiCbcSolverInterface *> (Base);
		osiCbc->getModelPtr()->saveReferenceSolver();
	}

	////////////////////////////////////////////////////////
	/////// OsiVolSolverInterface
	////////////////////////////////////////////////////////
	//OsiVolSolverInterface::OsiVolSolverInterface()
	//	:OsiSolverInterface(new ::OsiVolSolverInterface())
	//{
	//}

	//////////////////////////////////////////////////////
	///// OsiCpxSolverInterface
	//////////////////////////////////////////////////////
#ifdef USE_CPLEX
	// implementation here
#endif
}