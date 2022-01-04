// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcSolver.hpp>
#include <CbcModel.hpp>
#include <msclr\marshal.h> // for string ^ to char * via marshal_context
#include <msclr\marshal_cppstd.h> // for string ^ to std::string via marshal_as

#include "CbcModel.h"
#include "OsiCbcSolverInterface.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace COIN
{
	/// <summary>
	/// Delegate for handling CbcEvents.
	///	Meaning of whereFrom :
	/// 1 after initial solve by dualsimplex etc
	/// 2 after preprocessing
	/// 3 just before branchAndBound(so user can override)
	/// 4 just after branchAndBound(before postprocessing)
	/// 5 after postprocessing
	/// 6 after a user called heuristic phase
	/// </summary>
	public delegate int CbcSolverCallBack(COIN::CbcModel^ model, int whereFrom);

	// Native call back used for CbcMain1
	// If you're looking for events like solution found etc.
	// then more useful is CbcEventHandler. See Cbc/examples/inc.cpp and interupt.cpp
	static int NativeCallBackProxy(::CbcModel* model, int whereFrom);

	public ref class CbcSolver //: WrapperBase<::CbcSolver>
	{
	public:
		/// <summary>
		/// Call underlying CbcMain0 and CbcMain1, including native callback (not dummy).
		/// </summary>
		/// <param name="args">The arguments for the solve</param>
		/// <param name="cbcModel">The CbcModel instance</param>
		/// <returns>The return code</returns>
		static int CbcMain(array<System::String ^> ^args, CbcModel ^ cbcModel)
		{
			marshal_context^ context = gcnew marshal_context();
			int argc = 0;
			if (args != nullptr) argc = args->Length;

			const char** argv = new const char* [argc];
			for (int i = 0; i < argc; i++)
			{
				argv[i] = context->marshal_as<const char*>(args[i]);
			}
			CbcParameters cbcData;
			cbcData.enablePrinting();
			::CbcMain0(*(cbcModel->Base), cbcData);
			int result = ::CbcMain1(argc, argv, *(cbcModel->Base), NativeCallBackProxy, cbcData);
			delete context;
			delete[]argv;

			return result;
		}

		/// <summary>
		/// The CallBack to delegate to be invoked.
		/// If you add multiple delegates, all will be invoked, 
		/// but the return value will come from the last one.
		///
		///	Meaning of whereFrom :
		/// 1 after initial solve by dualsimplex etc
		/// 2 after preprocessing
		/// 3 just before branchAndBound(so user can override)
		/// 4 just after branchAndBound(before postprocessing)
		/// 5 after postprocessing
		/// 6 after a user called heuristic phase
		/// </summary>
		static CbcSolverCallBack^ CallBack;
	};
}
