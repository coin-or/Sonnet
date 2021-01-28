// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcModel.hpp>

#include "CbcStrategy.h"
#include "CglCutGenerator.h"
#include "CbcCutGenerator.h"
#include "OsiSolverInterface.h"
#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CbcModel : WrapperBase<::CbcModel>
	{
	internal:
		CbcModel(::CbcModel* obj)
			: WrapperBase(obj)
		{
		}

	public:
		CbcModel() { }

		/// Solve the initial LP relaxation
		void initialSolve()
		{
			Base->initialSolve();
		}

		/** \brief Invoke the branch \& cut algorithm

	The method assumes that initialSolve() has been called to solve the
	LP relaxation. It processes the root node, then proceeds to explore the
	branch & cut search tree. The search ends when the tree is exhausted or
	one of several execution limits is reached.
	If doStatistics is 1 summary statistics are printed
	if 2 then also the path to best solution (if found by branching)
	if 3 then also one line per node
  */
		void branchAndBound(int doStatistics)
		{
			Base->branchAndBound(doStatistics);
		}

		void branchAndBound()
		{
			branchAndBound(0);
		}

		OsiSolverInterface^ solver() {
			return OsiSolverInterface::CreateDerived(Base->solver());
		}

		#pragma region void addCutGenerator
		/** Add one generator - up to user to delete generators.
        howoften affects how generator is used. 0 or 1 means always,
        >1 means every that number of nodes.  Negative values have same
        meaning as positive but they may be switched off (-> -100) by code if
        not many cuts generated at continuous.  -99 is just done at root.
        Name is just for printout.
        If depth >0 overrides how often generator is called (if howOften==-1 or >0).
		*/
		void addCutGenerator(CglCutGenerator ^ generator,
                         int howOften, String ^ name,
                         bool normal, bool atSolution,
                         bool infeasible, int howOftenInSub,
                         int whatDepth, int whatDepthInSub)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->Base, howOften, charName, normal, atSolution, 
				infeasible, howOftenInSub, whatDepth, whatDepthInSub);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}

		void addCutGenerator(CglCutGenerator ^ generator, int howOften, String ^ name, bool normal, bool atSolution, bool infeasible, int howOftenInSub, int whatDepth)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->TransferBase(), howOften, charName, normal, atSolution, 
				infeasible, howOftenInSub, whatDepth);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}

		void addCutGenerator(CglCutGenerator ^ generator, int howOften, String ^ name, bool normal, bool atSolution, bool infeasible, int howOftenInSub)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->TransferBase(), howOften, charName, normal, atSolution, 
				infeasible, howOftenInSub);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}

		void addCutGenerator(CglCutGenerator ^ generator, int howOften, String ^ name, bool normal, bool atSolution, bool infeasible)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->TransferBase(), howOften, charName, normal, atSolution, 
				infeasible);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}	    

		void addCutGenerator(CglCutGenerator ^ generator, int howOften, String ^ name, bool normal, bool atSolution)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->TransferBase(), howOften, charName, normal, atSolution);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}	    

		void addCutGenerator(CglCutGenerator ^ generator, int howOften, String ^ name, bool normal)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->TransferBase(), howOften, charName, normal);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}	    

		void addCutGenerator(CglCutGenerator ^ generator, int howOften, String ^ name)
		{
			char * charName = nullptr;
			if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

			Base->addCutGenerator(generator->TransferBase(), howOften, charName);

			if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
		}	    

		void addCutGenerator(CglCutGenerator ^ generator, int howOften)
		{
			Base->addCutGenerator(generator->TransferBase(), howOften);
		}	    

		void addCutGenerator(CglCutGenerator ^ generator)
		{
			Base->addCutGenerator(generator->TransferBase());
		}	    
		#pragma endregion

		CbcStrategy ^ strategy()
		{
			return CbcStrategy::CreateDerived(Base->strategy());
		}

		/// Set the strategy. assigns
		void setStrategy(CbcStrategy ^ strategy) 
		{
			if (strategy != nullptr) 
			{
				Base->setStrategy(strategy->TransferBase());
			}
			else Base->setStrategy(nullptr);
		}

		bool isProvenOptimal()
		{
			return Base->isProvenOptimal();
		}

		/// Get current objective function value
		inline double getCurrentObjValue()
		{
			return Base->getCurrentObjValue();
		}

		/// Get current minimization objective function value
		inline double getCurrentMinimizationObjValue()
		{
			return Base->getCurrentMinimizationObjValue();
		}

		/// Get best objective function value as minimization
		inline double getMinimizationObjValue()
		{
			return Base->getMinimizationObjValue();
		}

		/// Set best objective function value as minimization
		inline void setMinimizationObjValue(double value)
		{
			Base->setMinimizationObjValue(value);
		}

		/// Get best objective function value
		inline double getObjValue()
		{
			return Base->getObjValue();
		}
		/** Get best possible objective function value.
			  This is better of best possible left on tree
			  and best solution found.
			  If called from within branch and cut may be optimistic.
		  */
		double getBestPossibleObjValue()
		{
			return Base->getBestPossibleObjValue();
		}
		
		/// Set best objective function value
		inline void setObjValue(double value)
		{
			Base->setObjValue(value);
		}

		/// Get solver objective function value (as minimization)
		inline double getSolverObjValue()
		{
			return Base->getSolverObjValue();
		}

		/** The best solution to the integer programming problem.

			The best solution to the integer programming problem found during
			the search. If no solution is found, the method returns null.
		  */
		inline double* bestSolution()
		{
			return Base->bestSolution();
		}

		/// Final status of problem - 0 finished, 1 stopped, 2 difficulties
		int status()
		{ 
			return Base->status();
		}

		/// Get the number of cut generators
		int numberCutGenerators() 
		{
			return Base->numberCutGenerators();
		}
    
		/// Get the list of cut generators
		System::Collections::Generic::IEnumerable<CbcCutGenerator ^> ^ cutGenerators() 
		{
			int n = Base->numberCutGenerators();
			array<CbcCutGenerator ^> ^result = gcnew array<CbcCutGenerator ^>(n);

			for(int i = 0; i < n; i++)
			{
				result[i] = gcnew CbcCutGenerator(Base->cutGenerator(i));
			}

			return result;
		}
		///Get the specified cut generator
		CbcCutGenerator ^ cutGenerator(int i) 
		{
			return gcnew CbcCutGenerator(Base->cutGenerator(i));
		}
	};
}