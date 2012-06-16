// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcModel.hpp>

#include "CbcModel.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CbcSolver //: WrapperBase<::CbcModel>
	{
	public:
		void static CbcMain0(CbcModel ^ cbcModel)
		{
			::CbcMain0(*(cbcModel->Base));
		}

		int static CbcMain1(array<System::String ^> ^args, CbcModel ^ cbcModel)
		{
			int argc = 0;
			if (args != nullptr) argc = args->Length;
			
			char **argv = new char *[argc];
			for(int i = 0; i < argc; i++)
			{
				argv[i] = (char*)Marshal::StringToHGlobalAnsi(args[i]).ToPointer();
			}

			int result = ::CbcMain1(argc, (const char **)argv, *(cbcModel->Base));

			for(int i = 0; i < argc; i++)
			{
				Marshal::FreeHGlobal((IntPtr)argv[i]);
			}
			return result;
		}
	};
}