// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <OsiClpSolverInterface.hpp>		// check via ClpSimplex_H (?)
#include <OsiCbcSolverInterface.hpp>		// check via ClpSimplex_H (?)
//#include <OsiCpxSolverInterface.hpp>		// Uncomment for CPLEX support
//#include <OsiGrbSolverInterface.hpp>		// Uncomment for Gurobi support

#include "OsiSolverInterface.h"
#include "CbcModel.h"
#include "CbcStrategy.h"
#include "ClpModel.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	//////////////////////////////////////////////////////
	///// OsiClpSolverInterface
	//////////////////////////////////////////////////////
	public ref class OsiClpSolverInterface : public OsiSolverInterfaceGeneric<::OsiClpSolverInterface>
	{
	public:
		OsiClpSolverInterface() { }
#ifdef SONNET_LEANLOADPROBLEM
		void OsiClpSolverInterface::LeanLoadProblemInit(int n, int m, int nz, 
			int*&Cst, int *&Clg, int *&Rnr, double *&Elm, 
			double*&l, double*&u, double *& c, double*&bl, double*&bu);
		void LeanLoadProblem(int n, int m, int nz, 
			int*&Cst, int *&Clg, int *&Rnr, double *&Elm, 
			double*&l, double*&u, double *& c, double*&bl, double*&bu);
#endif

		ClpSimplex^ getModelPtr()
		{
			return gcnew ClpSimplex(Base->getModelPtr());
		}
	};

	//////////////////////////////////////////////////////
	///// OsiCbcSolverInterface
	//////////////////////////////////////////////////////
	public ref class OsiCbcSolverInterface : public OsiSolverInterfaceGeneric<::OsiCbcSolverInterface>
	{
	public:
		OsiCbcSolverInterface() { }

		// this is NOT a copy constructor
		OsiCbcSolverInterface(OsiSolverInterface ^solver, CbcStrategy ^ strategy)
		{
			Base = new ::OsiCbcSolverInterface(solver->Base, (strategy != nullptr)?strategy->Base:nullptr);
		}

		// this is NOT a copy constructor
		OsiCbcSolverInterface(OsiSolverInterface ^solver)
		{
			Base = new ::OsiCbcSolverInterface(solver->Base, nullptr);
		}

		/// Get pointer to Cbc model
		CbcModel ^ getModelPtr() 
		{
			return gcnew CbcModel(Base->getModelPtr());
		}

		/// Get pointer to underlying solver
		inline OsiSolverInterface ^ getRealSolverPtr()
		{
			return getModelPtr()->solver();
		}

		void resetModelToReferenceSolver();
		void saveModelReferenceSolver();

	protected:
		property ::OsiCbcSolverInterface * Derived 
		{
			::OsiCbcSolverInterface * get() 
			{ 
				return dynamic_cast<::OsiCbcSolverInterface*>(Base); 
			} 
		}
	};

/*	// Uncomment for CPLEX support
	//////////////////////////////////////////////////////
	///// OsiCpxSolverInterface
	//////////////////////////////////////////////////////
	public ref class OsiCpxSolverInterface : public OsiSolverInterfaceGeneric<::OsiCpxSolverInterface>
	{
	public:
		OsiCpxSolverInterface()	{ }
	};
*/
/*	// Uncomment for Gurobi support
	////////////////////////////////////////////////////
	/// OsiGrbSolverInterface
	////////////////////////////////////////////////////
	public ref class OsiGrbSolverInterface : public OsiSolverInterfaceGeneric<::OsiGrbSolverInterface>
	{
	public:
		OsiGrbSolverInterface()	{ }
	};
*/
}