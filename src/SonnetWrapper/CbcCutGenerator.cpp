// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "CbcModel.h"
#include "CglCutGenerator.h"

namespace COIN
{
	/// Normal constructor
	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal, bool atSolution,
        bool infeasible, int howOftenInsub,
        int whatDepth, int whatDepthInSub, int switchOffIfLessThan)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal, atSolution,
			infeasible, howOftenInsub,
			whatDepth, whatDepthInSub, switchOffIfLessThan);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal, bool atSolution,
        bool infeasible, int howOftenInsub,
        int whatDepth, int whatDepthInSub)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal, atSolution,
			infeasible, howOftenInsub,
			whatDepth, whatDepthInSub);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal, bool atSolution,
        bool infeasible, int howOftenInsub,
        int whatDepth)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal, atSolution,
			infeasible, howOftenInsub,
			whatDepth);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal, bool atSolution,
        bool infeasible, int howOftenInsub)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal, atSolution,
			infeasible, howOftenInsub);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal, bool atSolution,
        bool infeasible)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal, atSolution,
			infeasible);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal, bool atSolution)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal, atSolution);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name,
        bool normal)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName,
			normal);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften, String ^ name)
	{
		char * charName = nullptr;
		if (name != nullptr) charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();

		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften, charName);

		if (name != nullptr) Marshal::FreeHGlobal((IntPtr)charName);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator,
        int howOften)
	{
		Base = new ::CbcCutGenerator(model->Base, generator->Base, 
			howOften);
	}

	CbcCutGenerator::CbcCutGenerator(CbcModel ^ model, CglCutGenerator ^ generator)
	{
		Base = new ::CbcCutGenerator(model->Base, generator->Base);
	}
}