// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include <CbcModel.hpp>
#include <CbcStrategy.hpp>

#include "CbcStrategy.h"
#include "CbcModel.h"

namespace COIN
{
	CbcStrategy ^ CbcStrategy::CreateDerived(::CbcStrategy *derived)
	{
		if (dynamic_cast<::CbcStrategyNull *>(derived))
		{
			CbcStrategy ^ result = gcnew CbcStrategyNull();
			result->Base = (::CbcStrategyNull *)(derived);
			result->TransferBase();
			return result;
		}
		else if (dynamic_cast<::CbcStrategyDefault *>(derived))
		{
			CbcStrategy ^ result = gcnew CbcStrategyDefault();
			result->Base = (::CbcStrategyDefault *)(derived);
			result->TransferBase();
			return result;
		}
		else if (dynamic_cast<::CbcStrategyDefaultSubTree *>(derived))
		{
			CbcStrategy ^ result = gcnew CbcStrategyDefaultSubTree();
			result->Base = (::CbcStrategyDefaultSubTree *)(derived);
			result->TransferBase();
			return result;
		}
		else
		{
			CbcStrategy ^ result = gcnew CbcStrategy();
			result->Base = (::CbcStrategy *)(derived);
			result->TransferBase();
			return result;
		}

		throw gcnew ArgumentException(L"Unknown CbcStrategy", gcnew String(typeid(derived).name())); 
	}


	/// Setup cut generators
	/*virtual*/ void CbcStrategy::setupCutGenerators(CbcModel ^ model)
	{
		Base->setupCutGenerators(*(model->Base));
	}

	/// Setup heuristics
	/*virtual*/ void CbcStrategy::setupHeuristics(CbcModel ^ model)
	{
		Base->setupHeuristics(*(model->Base));
	}

	/// Do printing stuff
	/*virtual*/ void CbcStrategy::setupPrinting(CbcModel ^ model, int modelLogLevel)
	{
		Base->setupPrinting(*(model->Base), modelLogLevel);
	}

	/// Other stuff e.g. strong branching and preprocessing
	/*virtual*/ void CbcStrategy::setupOther(CbcModel ^ model)
	{
		Base->setupOther(*(model->Base));
	}


	CbcStrategyDefaultSubTree::CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot, int numberStrong, int numberBeforeTrust, int printLevel)
	{
		::CbcModel *baseParent = (parent != nullptr)?parent->Base:nullptr;
		Base = new ::CbcStrategyDefaultSubTree(baseParent, cutsOnlyAtRoot, numberStrong, numberBeforeTrust, printLevel);
	}

	CbcStrategyDefaultSubTree::CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot, int numberStrong, int numberBeforeTrust)
	{
		::CbcModel *baseParent = (parent != nullptr)?parent->Base:nullptr;
		Base = new ::CbcStrategyDefaultSubTree(baseParent, cutsOnlyAtRoot, numberStrong, numberBeforeTrust);
	}

	CbcStrategyDefaultSubTree::CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot, int numberStrong)
	{
		::CbcModel* baseParent = (parent != nullptr)?parent->Base:nullptr;
		Base = new ::CbcStrategyDefaultSubTree(baseParent, cutsOnlyAtRoot, numberStrong);
	}

	CbcStrategyDefaultSubTree::CbcStrategyDefaultSubTree (CbcModel ^ parent, int cutsOnlyAtRoot)
	{
		::CbcModel *baseParent = (parent != nullptr)?parent->Base:nullptr;
		Base = new ::CbcStrategyDefaultSubTree(baseParent, cutsOnlyAtRoot);
	}

	CbcStrategyDefaultSubTree::CbcStrategyDefaultSubTree (CbcModel ^ parent)
	{
		::CbcModel* baseParent = (parent != nullptr)?parent->Base:nullptr;
		Base = new ::CbcStrategyDefaultSubTree(baseParent);
	}

}