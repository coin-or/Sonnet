// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

// The properties defined below are used in app.rc and AssemblyInfo.cpp

#pragma once

#define VER_FILEVERSION			1,4,0,0
#define VER_FILEVERSION_STR		"1.4.0.0"

// About Production & Assembly version:
// Did the interface change? 
// -> Yes: Are the changes backward compatible?
//         -> No: Then change the version number
//         -> Yes: Keep same version. 
// -> No: Keep same version.
#define VER_PRODUCTVERSION		1,4,0,0
#define VER_PRODUCTVERSION_STR	L"1.4.0.0"
#define VER_ASSEMBLYVERSION_STR	L"1.4.0.0" // Can use * but that's what we DONT want to do

#define VER_COPYRIGHT L"Copyright (C) 2011-2021"
#define VER_TRADEMARK L"This code is licensed under the terms of the Eclipse Public License (EPL)"

#if NETCOREAPP
#ifdef _DEBUG
#ifndef WIN32
#define VER_FILEDESCRIPTION L"SonnetWrapper 64-bit (net5.0, Debug)"
#else
#define VER_FILEDESCRIPTION L"SonnetWrapper 32-bit (net5.0, Debug)"
#endif
#else
#ifndef WIN32
#define VER_FILEDESCRIPTION L"SonnetWrapper 64-bit (net5.0)"
#else
#define VER_FILEDESCRIPTION L"SonnetWrapper 32-bit (net5.0)"
#endif
#endif

#define VER_FILENAME L"SonnetWrapperNETCore.dll"
#define VER_FILECOMMENTS L"SonnetWrapperNETCore is a managed .NET Core DLL with wrapper classes around existing C++ COIN-OR classes surrounding Cbc. See https://github.com/coin-or/Cbc."

#else

#ifdef _DEBUG
#ifndef WIN32
#define VER_FILEDESCRIPTION "SonnetWrapper 64-bit (net40, Debug)"
#else
#define VER_FILEDESCRIPTION "SonnetWrapper 32-bit (net40, Debug)"
#endif
#else
#ifndef WIN32
#define VER_FILEDESCRIPTION "SonnetWrapper 64-bit (net40)"
#else
#define VER_FILEDESCRIPTION "SonnetWrapper 32-bit (net40)"
#endif
#endif

#define VER_FILENAME "SonnetWrapper.dll"
#define VER_FILECOMMENTS "SonnetWrapper is a managed .NET Framework DLL with wrapper classes around existing C++ COIN-OR classes surrounding Cbc. See https://github.com/coin-or/Cbc."

#endif
// Leave lines blank at the end of this file to prevent RC1004

