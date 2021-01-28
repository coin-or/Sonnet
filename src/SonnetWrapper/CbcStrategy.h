// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CbcStrategy.hpp>

#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	ref class CbcModel;

	// There are two cases we have to worry about wrt to such .NET wrappers:
	// Once the base object is handed over to be deleted by some other entity, 
	// the wrapper object should no longer attempt 
	// 1) use it still and 
	// 2) delete it too
	// By setting deleteBase = false, we can work against 2).
	// Mitigating 1) is more difficult because we dont know when it was deleted by the other entity.
	// We could, for example, implement a proxy class in stead of the base object
	// and override its destructor such that the wrapper would be 'notified'.

	public ref class CbcStrategy : WrapperAbstractBase<::CbcStrategy>
	{
	public:
		static CbcStrategy ^ CreateDerived(::CbcStrategy *derived);

		/// Setup cut generators
		virtual void setupCutGenerators(CbcModel ^ model);

		/// Setup heuristics
		virtual void setupHeuristics(CbcModel ^ model);

		/// Do printing stuff
		virtual void setupPrinting(CbcModel ^ model, int modelLogLevel);

		/// Other stuff e.g. strong branching and preprocessing
		virtual void setupOther(CbcModel ^ model);
		
		/// Set model depth (i.e. how nested)
		inline void setNested(int depth) 
		{
			Base->setNested(depth);
		}
		/// Get model depth (i.e. how nested)
		inline int getNested() 
		{
			return Base->getNested();
		}

		/// Say preProcessing done
		inline void setPreProcessState(int state) 
		{
			Base->setPreProcessState(state);
		}
		/// See what sort of preprocessing was done
		inline int preProcessState() 
		{
			return Base->preProcessState();
		}
	};

	template<class T>
	public ref class CbcStrategyGeneric : CbcStrategy
	{
	protected:
		CbcStrategyGeneric()
		{
			// DONT USE COPY-CONSTRUCTOR (that would make as not-deleteBase
			Base = new T();
		}

	protected:
		property T * Base 
		{
			T * get()
			{ 
				return dynamic_cast<T*>(CbcStrategy::Base); 
			} 
		}
	};

	public ref class CbcStrategyNull : CbcStrategyGeneric<::CbcStrategyNull>
	{
	public:
		CbcStrategyNull() { }
	};

	public ref class CbcStrategyDefault : CbcStrategyGeneric<::CbcStrategyDefault>
	{
	public:
		CbcStrategyDefault() { }

		CbcStrategyDefault(int cutsOnlyAtRoot, int numberStrong, int numberBeforeTrust, int printLevel)			
		{
			Base = new ::CbcStrategyDefault(cutsOnlyAtRoot, numberStrong, numberBeforeTrust, printLevel);
		}

		CbcStrategyDefault(int cutsOnlyAtRoot, int numberStrong, int numberBeforeTrust)
		{
			Base = new ::CbcStrategyDefault(cutsOnlyAtRoot, numberStrong, numberBeforeTrust);
		}

		CbcStrategyDefault(int cutsOnlyAtRoot, int numberStrong)
		{
			Base = new ::CbcStrategyDefault(cutsOnlyAtRoot, numberStrong);
		}

		CbcStrategyDefault(int cutsOnlyAtRoot)
		{
			Base = new ::CbcStrategyDefault(cutsOnlyAtRoot);
		}
	};

	public ref class CbcStrategyDefaultSubTree : CbcStrategyGeneric<::CbcStrategyDefaultSubTree>
	{
	public:
		CbcStrategyDefaultSubTree() { }

		CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot, int numberStrong, int numberBeforeTrust, int printLevel);

		CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot, int numberStrong, int numberBeforeTrust);

		CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot, int numberStrong);

		CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot);

		CbcStrategyDefaultSubTree (CbcModel ^ parent);
	};
}