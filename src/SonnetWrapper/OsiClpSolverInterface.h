// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <OsiClpSolverInterface.hpp>		// check via ClpSimplex_H (?)
//#include <OsiCpxSolverInterface.hpp>		// Uncomment for CPLEX support
//#include <OsiGrbSolverInterface.hpp>		// Uncomment for Gurobi support

#include "OsiSolverInterface.h"
#include "ClpModel.h"
#include "CoinPackedMatrix.h"

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

		/// <summary>
		/// Borrow constructor - dont delete rhs in destructor
		/// </summary>
		/// <param name="rhs"></param>
		OsiClpSolverInterface(ClpSimplex^ rhs /*, bool reallyOwn = false */)
			: OsiClpSolverInterface(rhs, false)
		{ }

		/// <summary>
		/// Borrow constructor - only delete if reallyOwn
		/// </summary>
		/// <param name="rhs"></param>
		/// <param name="reallyOwn"></param>
		OsiClpSolverInterface(ClpSimplex^ rhs, bool reallyOwn)
		{
			::ClpSimplex* rhsBase = (::ClpSimplex*)(rhs->Base);
			Base = new ::OsiClpSolverInterface(rhsBase, reallyOwn);
		}
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

		/// Get pointer to row-wise copy of matrix
		CoinPackedMatrix^ getMatrixByRow()
		{
			return gcnew CoinPackedMatrix(Base->getMatrixByRow());
		}

		/// Get pointer to column-wise copy of matrix
		CoinPackedMatrix^ getMatrixByCol()
		{
			return gcnew CoinPackedMatrix(Base->getMatrixByCol());
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