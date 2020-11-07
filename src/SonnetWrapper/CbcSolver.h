// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcSolver.hpp>
#include <CbcModel.hpp>
#include <msclr\marshal.h> // for string ^ to char * via marshal_context
#include <msclr\marshal_cppstd.h> // for string ^ to std::string via marshal_as

#include "CbcModel.h"
#include "OsiDerivedSolverInterfaces.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace COIN
{
	public ref class CbcSolver //: WrapperBase<::CbcModel>
	{
	public:
		int static callCbc(System::String ^ input2)
		{
			std::string input2s = marshal_as<std::string>(input2);
			return ::callCbc(input2s);
		}

		int static callCbc(System::String ^ input2, OsiClpSolverInterface ^ solver1)
		{
			std::string input2s = marshal_as<std::string>(input2);
			return ::callCbc(input2s, *(solver1->Base));
		}

		int static callCbc(System::String ^ input2, CbcModel ^ babSolver)
		{
			std::string input2s = marshal_as<std::string>(input2);
			return ::callCbc(input2s, *(babSolver->Base));
		}

		int static CbcMain(array<System::String ^> ^args, CbcModel ^ cbcModel)
		{
			marshal_context^ context = gcnew marshal_context();
			int argc = 0;
			if (args != nullptr) argc = args->Length;
			
			const char **argv = new const char *[argc];
			for(int i = 0; i < argc; i++)
			{
				argv[i] = context->marshal_as<const char *>(args[i]);
//					(char*)Marshal::StringToHGlobalAnsi(args[i]).ToPointer();
			}

			int result = ::CbcMain(argc, argv, *(cbcModel->Base));
			delete context;
			delete []argv;

			return result;
		}
		void static CbcMain0(CbcModel ^ cbcModel)
		{
			CbcSolverUsefulData cbcData;
			::CbcMain0(*(cbcModel->Base), cbcData);
		}

		int static CbcMain1(array<System::String ^> ^args, CbcModel ^ cbcModel)
		{
			marshal_context^ context = gcnew marshal_context();
			int argc = 0;
			if (args != nullptr) argc = args->Length;
			
			const char **argv = new const char *[argc];
			for(int i = 0; i < argc; i++)
			{
				argv[i] = context->marshal_as<const char *>(args[i]);
				//argv[i] = (char*)Marshal::StringToHGlobalAnsi(args[i]).ToPointer();
			}
			CbcSolverUsefulData cbcData;
			int result = ::CbcMain1(argc, argv, *(cbcModel->Base), cbcData);
			delete context;
			delete []argv;

			return result;
		}
	};
}