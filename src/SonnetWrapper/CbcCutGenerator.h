// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcCutGenerator.hpp>

#include "CglCutGenerator.h"
#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CbcCutGenerator : WrapperBase<::CbcCutGenerator>
	{
	internal:
		CbcCutGenerator(::CbcCutGenerator *copy)
			:WrapperBase(copy)
		{
		}

	public:
		CbcCutGenerator() { }
	
		/// Normal constructor
		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal, bool atSolution,
            bool infeasible, int howOftenInsub,
            int whatDepth, int whatDepthInSub, int switchOffIfLessThan);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal, bool atSolution,
            bool infeasible, int howOftenInsub,
            int whatDepth, int whatDepthInSub);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal, bool atSolution,
            bool infeasible, int howOftenInsub,
            int whatDepth);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal, bool atSolution,
            bool infeasible, int howOftenInsub);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal, bool atSolution,
            bool infeasible);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal, bool atSolution);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name,
            bool normal);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften, String ^ name);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
            int howOften);

		CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator);

		/// Get the \c CglCutGenerator corresponding to this \c CbcCutGenerator.
		CglCutGenerator ^ generator() 
		{
			return CglCutGenerator::CreateDerived(Base->generator());
		}
	};
}
