// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

#pragma once

#define VER_FILEVERSION			1,3,2,0
#define VER_FILEVERSION_STR		"1.3.2.0"

// About Production & Assembly version:
// Did the interface change? 
// -> Yes: Are the changes backward compatible?
//         -> No: Then change the version number
//         -> Yes: Keep same version. 
// -> No: Keep same version.
#define VER_PRODUCTVERSION		1,3,2,0
#define VER_PRODUCTVERSION_STR	"1.3.2.0"
#define VER_ASSEMBLYVERSION_STR	"1.3.2.0" // Can use *

#ifdef _DEBUG
#ifndef WIN32
#define VER_FILEDESCRIPTION "SonnetWrapper 64-bit (Debug), based on Cbc 2.10.11"
#else
#define VER_FILEDESCRIPTION "SonnetWrapper 32-bit (Debug), based on Cbc 2.10.11"
#endif
#else
#ifndef WIN32
#define VER_FILEDESCRIPTION "SonnetWrapper 64-bit, based on Cbc 2.10.11"
#else
#define VER_FILEDESCRIPTION "SonnetWrapper 32-bit, based on Cbc 2.10.11"
#endif
#endif

#define VER_COPYRIGHT "Copyright (C) 2011-2023"
#define VER_TRADEMARK "This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0)"
#define VER_FILENAME "SonnetWrapper.dll"
#define VER_FILECOMMENTS "SonnetWrapper is a managed DLL with wrapper classes around existing C++ COIN-OR classes. This version of SonnetWrapper is based on Cbc 2.10.10. See http://github.com/coin-or/sonnet and http://www.coin-or.org."
