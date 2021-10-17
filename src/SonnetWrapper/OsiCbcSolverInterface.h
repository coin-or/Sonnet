// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <OsiClpSolverInterface.hpp>		// check via ClpSimplex_H (?)
#include <OsiCbcSolverInterface.hpp>		// check via ClpSimplex_H (?)

#include "OsiClpSolverInterface.h"
#include "CbcModel.h"
#include "CbcStrategy.h"
#include "ClpModel.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
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

		/// <summary>
		/// Returns the CbcModel
		/// </summary>
		property CbcModel^ Model
		{
			CbcModel^ get() { return getModelPtr(); }
		}

		/// Get pointer to underlying solver
		inline OsiSolverInterface ^ getRealSolverPtr()
		{
			return getModelPtr()->solver();
		}

		/// <summary>
		/// Get the underlying continuous solver
		/// </summary>
		property OsiSolverInterface^ RealSolver
		{
			OsiSolverInterface^ get() { return getRealSolverPtr(); }
		}

		void resetModelToReferenceSolver();
		void saveModelReferenceSolver();

		/// Get how many Nodes it took to solve the problem.
		inline int getNodeCount()
		{
			return Derived->getNodeCount();
		}

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