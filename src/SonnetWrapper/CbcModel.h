// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcModel.hpp>

#include "CbcEventHandler.h"
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
		CbcModel(const ::CbcModel* obj)
			: WrapperBase(obj)
		{
		}

	public:
		CbcModel() { }

		/// <summary>
		/// Solve the initial LP relaxation
		/// </summary>
		void initialSolve()
		{
			Base->initialSolve();
		}

		/// <summary>
		/// Invoke the branch \& cut algorithm
		///
		/// The method assumes that initialSolve() has been called to solve the
		///	LP relaxation.It processes the root node, then proceeds to explore the
		/// branch& cut search tree.The search ends when the tree is exhausted or
		/// one of several execution limits is reached.
		/// </summary>
		/// <param name="doStatistics">If doStatistics is 1 summary statistics are printed, 
		/// if 2 then also the path to best solution(if found by branching),
		/// if 3 then also one line per node</param>
		void branchAndBound(int doStatistics)
		{
			Base->branchAndBound(doStatistics);
		}

		/// <summary>
		/// Invoke the branch \& cut algorithm, with doStatistics = 0
		/// </summary>
		void branchAndBound()
		{
			branchAndBound(0);
		}

		OsiSolverInterface^ solver() {
			return OsiSolverInterface::CreateDerived(Base->solver());
		}

		/// <summary>
		/// Set an event handler
		/// Note: In SonnetWrapper the eventHandler is merely a method delegate, not an instance of a class derived from the native CbcEventHandler class
		/// This is a shortcut with limited functionality, but easier to use.
		/// Multiple delegates can be assigned to the eventHandler. All will be invoked, 
		/// but onluy the return value (CbcAction) of the last will be used.
		/// </summary>
		/// <param name="eventHandler">A clone of the handler passed as a parameter is stored in CbcModel.</param>
		void passInEventHandler(CbcEventHandler^ eventHandler)
		{
			CbcDelegateEventHandlerProxy handler(eventHandler);
			Base->passInEventHandler(&handler); // clones the handler and will delete it later
		}
		
		/// <summary>
		/// Retrieve a pointer to the event handler
		/// </summary>
		/// <returns>Returns the delegate event handler, or null if none was assigned.</returns>
		inline CbcEventHandler^ getEventHandler()
		{
			::CbcEventHandler* native = Base->getEventHandler();
			CbcDelegateEventHandlerProxy* handler = dynamic_cast<CbcDelegateEventHandlerProxy*>(native);
			if (handler != nullptr)
			{
				return handler->getDelegate();
			}
			return nullptr;
		}

		#pragma region void addCutGenerator

		/// <summary>
		/// Add one generator - up to user to delete generators.	
		/// </summary>
		/// <param name="generator"></param>
		/// <param name="howOften">howoften affects how generator is used. 0 or 1 means always,
		/// > 1 means every that number of nodes.Negative values have same
		/// meaning as positive but they may be switched off(-> - 100) by code if
		/// not many cuts generated at continuous. - 99 is just done at root.</param>
		/// <param name="name">Name is just for printout.</param>
		/// <param name="normal"></param>
		/// <param name="atSolution"></param>
		/// <param name="infeasible"></param>
		/// <param name="howOftenInSub"></param>
		/// <param name="whatDepth">If depth > 0 overrides how often generator is called(if howOften == -1 or > 0).</param>
		/// <param name="whatDepthInSub"></param>
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

		/// <summary>
		/// Is optimality proven?
		/// </summary>
		/// <returns>True if optimality is proven, false otherwise.	</returns>
		bool isProvenOptimal()
		{
			return Base->isProvenOptimal();
		}

		/// <summary>
		/// Get current objective function value
		/// </summary>
		/// <returns>The current objective function value</returns>
		inline double getCurrentObjValue()
		{
			return Base->getCurrentObjValue();
		}

		/// <summary>
		/// Get current minimization objective function value
		/// </summary>
		/// <returns>The current minimization objective function value</returns>
		inline double getCurrentMinimizationObjValue()
		{
			return Base->getCurrentMinimizationObjValue();
		}

		/// <summary>
		/// Get best objective function value as minimization
		/// </summary>
		/// <returns>Best objective function value as minimization</returns>
		inline double getMinimizationObjValue()
		{
			return Base->getMinimizationObjValue();
		}

		/// <summary>
		/// Set best objective function value as minimization
		/// </summary>
		/// <param name="value">best objective function value as minimization</param>
		inline void setMinimizationObjValue(double value)
		{
			Base->setMinimizationObjValue(value);
		}

		/// <summary>
		/// Get best objective function value
		/// </summary>
		/// <returns>Best objective function value</returns>
		inline double getObjValue()
		{
			return Base->getObjValue();
		}

		/// <summary>
		/// Get objective function sense (1 for min (default), -1 for max)
		/// </summary>
		/// <returns></returns>
		inline double getObjSense() 
		{
			return Base->getObjSense();
		}

		/// <summary>
		/// Get best possible objective function value.
		/// This is better of best possible left on tree and best solution found.
		/// If called from within branch and cut may be optimistic.
		/// </summary>
		/// <returns>Best possible objective function value.</returns>
		double getBestPossibleObjValue()
		{
			return Base->getBestPossibleObjValue();
		}
		
		/// <summary>
		/// Set best objective function value
		/// </summary>
		/// <param name="value">best objective function value</param>
		inline void setObjValue(double value)
		{
			Base->setObjValue(value);
		}

		/// <summary>
		/// Get solver objective function value (changed sign to be as minimization)
		/// </summary>
		/// <returns></returns>
		inline double getSolverObjValue()
		{
			return Base->getSolverObjValue();
		}
		
		/// <summary>
		/// The best solution to the integer programming problem.
		///
		///	The best solution to the integer programming problem found during
		///	the search. If no solution is found, the method returns null.
		/// Returns the internal array of best solution (no copy)
		/// </summary>
		/// <returns>internal array of best solution</returns>
		inline double* bestSolution()
		{
			return Base->bestSolution();
		}

		/// <summary>
		/// User callable setBestSolution
		/// 
		/// Sets the best solution and best objective value.
		/// If check false (default) does not check valid
		/// If true then sees if feasible and warns if objective value
		///	worse than given(so just set to COIN_DBL_MAX if you don't care).
		///	If check true then does not save solution if not feasible. 
		/// (If check true then the solution is checked by fixing the bounds of the integer variables and solving
		/// the relaxation. If that problem is feasible and integer, then consider the solution OK.)
		/// </summary>
		/// <param name="solution">The solution as array of column values</param>
		/// <param name="numberColumns">The number of columns in this solution</param>
		/// <param name="objectiveValue">The objective value of this solution</param>
		/// <param name="check">If check true then does not save solution if not feasible.</param>
		inline void setBestSolutionUnsafe(const double* solution, int numberColumns, double objectiveValue, bool check /*= false*/)
		{
			Base->setBestSolution(solution, numberColumns, objectiveValue, check);
		}

		/// <summary>
		/// User callable setBestSolution
		/// 
		/// Sets the best solution and best objective value.
		/// If check false (default) does not check valid
		/// If true then sees if feasible and warns if objective value
		///	worse than given(so just set to COIN_DBL_MAX if you don't care).
		///	If check true then does not save solution if not feasible. 
		/// (If check true then the solution is checked by fixing the bounds of the integer variables and solving
		/// the relaxation. If that problem is feasible and integer, then consider the solution OK.)
		/// </summary>
		/// <param name="solution">The solution as array of column values</param>
		/// <param name="numberColumns">The number of columns in this solution</param>
		/// <param name="objectiveValue">The objective value of this solution</param>
		/// <param name="check">If check true then does not save solution if not feasible.</param>
		inline void setBestSolution(array<double> ^solution, int numberColumns, double objectiveValue, bool check /*= false*/)
		{
			pin_ptr<double> solutionPinned = GetPinablePtr(solution);
			this->setBestSolutionUnsafe(solutionPinned, numberColumns, objectiveValue, check);
		}

		/// <summary>
		/// User callable setBestSolution.
		///
		/// Sets the best solution and best objective value.
		/// Does not check valid.
		/// </summary>
		/// <param name="solution">The solution as array of column values</param>
		/// <param name="numberColumns">The number of columns in this solution</param>
		/// <param name="objectiveValue">The objective value of this solution</param>
		inline void setBestSolutionUnsafe(const double* solution, int numberColumns, double objectiveValue)
		{
			Base->setBestSolution(solution, numberColumns, objectiveValue);
		}

		/// <summary>
		/// User callable setBestSolution.
		///
		/// Sets the best solution and best objective value.
		/// Does not check valid.
		/// </summary>
		/// <param name="solution">The solution as array of column values</param>
		/// <param name="numberColumns">The number of columns in this solution</param>
		/// <param name="objectiveValue">The objective value of this solution</param>
		inline void setBestSolution(array<double>^ solution, int numberColumns, double objectiveValue)
		{
			pin_ptr<double> solutionPinned = GetPinablePtr(solution);
			this->setBestSolutionUnsafe(solutionPinned, numberColumns, objectiveValue);
		}

		/// <summary>
		/// Set cutoff bound on the objective function.
		/// When using strict comparison, the bound is adjusted by a tolerance to
		/// avoid accidentally cutting off the optimal solution.
		///
		/// WARNING: Unexplainable results for max.
		/// </summary>
		/// <param name="value"></param>
		inline void setCutoff(double value)
		{
			Base->setCutoff(value);
		}

		/// <summary>
		/// Get the cutoff bound on the objective function - always as minimize
		/// </summary>
		/// <returns></returns>
		inline double getCutoff()
		{ 
			return Base->getCutoff();
		}

		/// <summary>
		/// Set the CbcModel::CbcMaximumSeconds maximum number of seconds desired.
		/// </summary>
		/// <param name="value">The maximum number of seconds</param>
		/// <returns>True (always)</returns>
		inline bool setMaximumSeconds(double value)
		{
			return Base->setMaximumSeconds(value);
		}

		/// <summary>
		/// Get the CbcModel::CbcMaximumSeconds maximum number of seconds desired.
		/// </summary>
		/// <returns>The maximum number of seconds</returns>
		inline double getMaximumSeconds()
		{
			return Base->getMaximumSeconds();
		}

		/// <summary>
		/// Final status of problem - 0 finished, 1 stopped, 2 difficulties
		/// </summary>
		/// <returns>0 finished, 1 stopped, 2 difficulties</returns>
		int status()
		{ 
			return Base->status();
		}

		/// <summary>
		/// Get the number of cut generators
		/// </summary>
		/// <returns>the number of cut generators</returns>
		int numberCutGenerators() 
		{
			return Base->numberCutGenerators();
		}
    
		/// <summary>
		/// Get the list of cut generators
		/// </summary>
		/// <returns>Enumerable of cut generators</returns>
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