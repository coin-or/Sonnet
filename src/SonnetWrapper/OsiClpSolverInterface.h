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
			// Careful! At this point, the returned object is a wrapper around the native child object
			// without any further reference to the original managed parent (this). Therefore, the GC
			// may decide that the managed parent object (this) can be disposed if there are no other references to it!
			// However, that would also dispose the wrapped native parent object, and also the native child object
			// And thus, the returned managed object here would be illegally referring to a native child object.
			// To prevent this, either explicity GC.KeepAlive(this), or maintain a managed 
			// reference to the managed parent, such that GC knows not to dispose the parent (this).
			// This caused a crash when running in Release build in Model.NewHelper.
			return gcnew CoinPackedMatrix(Base->getMatrixByRow());
		}

		/// <summary>
		/// Read an mps file from the given filename.
		/// </summary>
		/// <param name="fileName">File to be read</param>
		/// <param name="keepNames">True to use the given row and column names</param>
		/// <param name="ignoreErrors">True to ignore errors during reading of the file</param>
		/// <returns>Status non-zero represents an error</returns>
		int readMps(String^ fileName, bool keepNames, bool ignoreErrors)
		{
			try
			{
				char* charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
				int result = ((::OsiClpSolverInterface *) Base)->readMps(charFileName, keepNames, ignoreErrors);
				Marshal::FreeHGlobal((IntPtr)charFileName);
				return result;
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}

		/// Get pointer to column-wise copy of matrix
		CoinPackedMatrix^ getMatrixByCol()
		{
			return gcnew CoinPackedMatrix(Base->getMatrixByCol());
		}

		virtual property System::String^ Version 
		{
			System::String^ get() override
			{
				return gcnew String(CLP_VERSION);
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