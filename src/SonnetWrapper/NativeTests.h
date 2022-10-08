// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#undef main
#define main mainCbc
#include <CoinSolve.cpp>

#undef main
#define main mainGamsTest
#include <Cbc\test\GamsTest.cpp>

#undef main
#define main mainOsiCbcUnitTest
#include <cbc\test\osiUnitTest.cpp>
#include <cbc\test\OsiCbcSolverInterfaceTest.cpp>

#undef main
#define main mainOsiClpUnitTest
#include <clp\test\osiUnitTest.cpp>
#include <Clp\test\OsiClpSolverInterfaceTest.cpp>

#undef main

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class NativeTests
	{
	public:
		/// <summary>
		/// Runs the Cbc test using cbc.exe and Sample and miplib3 dataset.
		/// This method does not use SonnetWrapper classes
		/// </summary>
		/// <param name="dirSample">The relative or absolute path to the coin-or Sample files. Example: ..\\..\\..\\Data-sample</param>
		/// <param name="dirMipLib">The relative or absolute path to the miplib3 files. Example: ..\\..\\..\\Data-miplib</param>
		/// <returns>Return value of main</returns>
		static int RunCbc(String^ dirSample, String^ dirMipLib)
		{
			assert(dirSample != nullptr || dirMipLib != nullptr);

			char* charDirSample = dirSample != nullptr ? (char*)Marshal::StringToHGlobalAnsi(dirSample).ToPointer() : nullptr;
			char* charDirMipLib = dirMipLib != nullptr ? (char*)Marshal::StringToHGlobalAnsi(dirMipLib).ToPointer() : nullptr;

			// example: dirMipLib = "..\\..\\..\\..\\..\\..\\..\\..\\coin-or-miplib3"
			// cbc.exe -dirMiplib `cygpath - w C : / projects / dist / share / coin - or -miplib3` - unitTest
			// cbc.exe -dirSample `echo /d/a/Cbc/Cbc/dist/share/coin/Data/Sample` -dirMiplib `echo /d/a/Cbc/Cbc/dist/share/coin/Data/miplib3` -unitTest

			int result = 1;

			if (charDirSample != nullptr && charDirMipLib != nullptr)
			{
				const char* argv[6] = { "cbc.exe", "-dirSample", charDirSample, "-dirMiplib", charDirMipLib, "-unitTest" };
				result = ::mainCbc(6, argv);
			}
			else if (charDirSample != nullptr)
			{
				const char* argv[4] = { "cbc.exe", "-dirSample", charDirSample, "-unitTest" };
				result = ::mainCbc(4, argv);
			}
			else if (charDirMipLib != nullptr)
			{
				const char* argv[4] = { "cbc.exe", "-dirMiplib", charDirMipLib, "-unitTest" };
				result = ::mainCbc(4, argv);
			}

			if (charDirSample != nullptr) Marshal::FreeHGlobal((IntPtr)charDirSample);
			if (charDirMipLib != nullptr) Marshal::FreeHGlobal((IntPtr)charDirMipLib);
			return result;
		};

		/// <summary>
		/// Runs the Cbc test using gamsTest.exe
		/// </summary>
		/// <returns>Return value of main</returns>
		static int RunGamsTest()
		{
			return ::mainGamsTest(0, nullptr);
		};

		/// <summary>
		/// Runs the Cbc test osiUnitTest.exe and 'Sample' dataset with exmip1 etc.
		/// </summary>
		/// <returns>Return value of main</returns>
		static int RunOsiCbcUnitTest(String^ mpsDir)
		{
			assert(mpsDir != nullptr);

			//#include <msclr/marshal_cppstd.h>
			//System::String^ foo = "Hello World";
			//std::string converted_foo = msclr::interop::marshal_as<std::string>(foo);
			//const char* charFoo = converted_foo.c_str();

			char* charMpsDir = (char*)Marshal::StringToHGlobalAnsi("-mpsDir=" + mpsDir).ToPointer();
			// example: "-mpsDir=..\\..\\..\\..\\..\\..\\..\\..\\coin-or-sample"
			// osiUnitTest.exe -mpsDir=`cygpath -w C:/projects/dist/share/coin-or-sample`

			const char* argv[2] = { "osiUnitTest.exe", charMpsDir };
			int result = ::mainOsiCbcUnitTest(2, argv);

			if (charMpsDir != nullptr) Marshal::FreeHGlobal((IntPtr)charMpsDir);
			return result;
		};

		/// <summary>
		/// Runs the Clp test osiUnitTest.exe and 'Sample' dataset with exmip1 etc.
		/// </summary>
		/// <returns>Return value of main</returns>
		static int RunOsiClpUnitTest(String^ mpsDir)
		{
			assert(mpsDir != nullptr);

			char* charMpsDir = (char*)Marshal::StringToHGlobalAnsi("-mpsDir=" + mpsDir).ToPointer();
			// example: "-mpsDir=..\\..\\..\\..\\..\\..\\..\\..\\coin-or-sample"
			// osiUnitTest.exe -mpsDir=`cygpath -w C:/projects/dist/share/coin-or-sample`

			const char* argv[2] = { "osiUnitTest.exe", charMpsDir };
			int result = ::mainOsiClpUnitTest(2, argv);

			if (charMpsDir != nullptr) Marshal::FreeHGlobal((IntPtr)charMpsDir);
			return result;
		};
	};
}