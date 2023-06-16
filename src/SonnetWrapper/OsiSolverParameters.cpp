// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

#include "OsiSolverParameters.h"
#include <OsiSolverParameters.hpp>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	::OsiStrParam GetOsiStrParam(COIN::OsiStrParam key)
	{
		switch (key)
		{
		case COIN::OsiStrParam::OsiProbName:
			return ::OsiProbName;
		case COIN::OsiStrParam::OsiSolverName:
			return ::OsiSolverName;
		case COIN::OsiStrParam::OsiLastStrParam:
			return ::OsiLastStrParam;
		}

		throw gcnew ArgumentException(L"Unknown OsiStrParam", ((int)key).ToString()); 
	}

	::OsiDblParam GetOsiDblParam(COIN::OsiDblParam key)
	{
		switch (key)
		{
		case COIN::OsiDblParam::OsiDualObjectiveLimit:
			return ::OsiDualObjectiveLimit;
		case COIN::OsiDblParam::OsiPrimalObjectiveLimit:
			return ::OsiPrimalObjectiveLimit;
		case COIN::OsiDblParam::OsiDualTolerance:
			return ::OsiDualTolerance;
		case COIN::OsiDblParam::OsiPrimalTolerance:
			return ::OsiPrimalTolerance;
		case COIN::OsiDblParam::OsiObjOffset:
			return ::OsiObjOffset;
		}

		throw gcnew ArgumentException(L"Unknown OsiDblParam", key.ToString()); 
	}

	::OsiIntParam GetOsiIntParam(COIN::OsiIntParam key)
	{
		switch (key)
		{
		case COIN::OsiIntParam::OsiMaxNumIteration:
			return ::OsiMaxNumIteration;
		case COIN::OsiIntParam::OsiMaxNumIterationHotStart:
			return ::OsiMaxNumIterationHotStart;
		case COIN::OsiIntParam::OsiNameDiscipline:
			return ::OsiNameDiscipline;
		case COIN::OsiIntParam::OsiLastIntParam:
			return ::OsiLastIntParam;
		}

		throw gcnew ArgumentException(L"Unknown OsiIntParam", key.ToString()); 
	}

	::OsiHintParam GetOsiHintParam(COIN::OsiHintParam key)
	{
		switch (key)
		{
		case COIN::OsiHintParam::OsiDoCrash:
			return ::OsiDoCrash;
		case COIN::OsiHintParam::OsiDoDualInInitial:
			return ::OsiDoDualInInitial;
		case COIN::OsiHintParam::OsiDoDualInResolve:
			return ::OsiDoDualInResolve;
		case COIN::OsiHintParam::OsiDoInBranchAndCut:
			return ::OsiDoInBranchAndCut;
		case COIN::OsiHintParam::OsiDoPresolveInInitial:
			return ::OsiDoPresolveInInitial;
		case COIN::OsiHintParam::OsiDoPresolveInResolve:
			return ::OsiDoPresolveInResolve;
		case COIN::OsiHintParam::OsiDoReducePrint:
			return ::OsiDoReducePrint;
		case COIN::OsiHintParam::OsiDoScale:
			return ::OsiDoScale;
		case COIN::OsiHintParam::OsiLastHintParam:
			return ::OsiLastHintParam;
		}

		throw gcnew ArgumentException(L"Unknown OsiHintParam", key.ToString()); 
	}

	::OsiHintStrength GetOsiHintStrength(COIN::OsiHintStrength key)
	{
		switch (key)
		{
		case COIN::OsiHintStrength::OsiForceDo:
			return ::OsiForceDo;
		case COIN::OsiHintStrength::OsiHintDo:
			return ::OsiHintDo;
		case COIN::OsiHintStrength::OsiHintIgnore:
			return ::OsiHintIgnore;
		case COIN::OsiHintStrength::OsiHintTry:
			return ::OsiHintTry;
		}

		throw gcnew ArgumentException(L"Unknown OsiHintStrength", key.ToString()); 
	}




	COIN::OsiStrParam GetOsiStrParam(::OsiStrParam key)
	{
		switch (key)
		{
		case ::OsiProbName:
			return COIN::OsiStrParam::OsiProbName;
		case ::OsiSolverName:
			return COIN::OsiStrParam::OsiSolverName;
		case ::OsiLastStrParam:
			return COIN::OsiStrParam::OsiLastStrParam;
		}

		throw gcnew ArgumentException(L"Unknown OsiStrParam", ((int)key).ToString()); 
	}

	COIN::OsiDblParam GetOsiDblParam(::OsiDblParam key)
	{
		switch (key)
		{
		case ::OsiDualObjectiveLimit:
			return COIN::OsiDblParam::OsiDualObjectiveLimit;
		case ::OsiPrimalObjectiveLimit:
			return COIN::OsiDblParam::OsiPrimalObjectiveLimit;
		case ::OsiDualTolerance:
			return COIN::OsiDblParam::OsiDualTolerance;
		case ::OsiPrimalTolerance:
			return COIN::OsiDblParam::OsiPrimalTolerance;
		case ::OsiObjOffset:
			return COIN::OsiDblParam::OsiObjOffset;
		}

		throw gcnew ArgumentException(L"Unknown OsiDblParam", ((int)key).ToString()); 
	}

	COIN::OsiIntParam GetOsiIntParam(::OsiIntParam key)
	{
		switch (key)
		{
		case ::OsiMaxNumIteration:
			return COIN::OsiIntParam::OsiMaxNumIteration;
		case ::OsiMaxNumIterationHotStart:
			return COIN::OsiIntParam::OsiMaxNumIterationHotStart;
		case ::OsiNameDiscipline:
			return COIN::OsiIntParam::OsiNameDiscipline;
		case ::OsiLastIntParam:
			return COIN::OsiIntParam::OsiLastIntParam;
		}

		throw gcnew ArgumentException(L"Unknown OsiIntParam", ((int)key).ToString()); 
	}

	COIN::OsiHintParam GetOsiHintParam(::OsiHintParam key)
	{
		switch (key)
		{
		case ::OsiDoCrash:
			return COIN::OsiHintParam::OsiDoCrash;
		case ::OsiDoDualInInitial:
			return COIN::OsiHintParam::OsiDoDualInInitial;
		case ::OsiDoDualInResolve:
			return COIN::OsiHintParam::OsiDoDualInResolve;
		case ::OsiDoInBranchAndCut:
			return COIN::OsiHintParam::OsiDoInBranchAndCut;
		case ::OsiDoPresolveInInitial:
			return COIN::OsiHintParam::OsiDoPresolveInInitial;
		case ::OsiDoPresolveInResolve:
			return COIN::OsiHintParam::OsiDoPresolveInResolve;
		case ::OsiDoReducePrint:
			return COIN::OsiHintParam::OsiDoReducePrint;
		case ::OsiDoScale:
			return COIN::OsiHintParam::OsiDoScale;
		case ::OsiLastHintParam:
			return COIN::OsiHintParam::OsiLastHintParam;
		}

		throw gcnew ArgumentException(L"Unknown OsiHintParam", ((int)key).ToString()); 
	}

	COIN::OsiHintStrength GetOsiHintStrength(::OsiHintStrength key)
	{
		switch (key)
		{
		case ::OsiForceDo:
			return COIN::OsiHintStrength::OsiForceDo;
		case ::OsiHintDo:
			return COIN::OsiHintStrength::OsiHintDo;
		case ::OsiHintIgnore:
			return COIN::OsiHintStrength::OsiHintIgnore;
		case ::OsiHintTry:
			return COIN::OsiHintStrength::OsiHintTry;
		}

		throw gcnew ArgumentException(L"Unknown OsiHintStrength", ((int)key).ToString()); 
	}
}