// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).
#include "AssemblyInfo.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

[assembly:AssemblyTitleAttribute(VER_FILENAME)] ;
[assembly:AssemblyDescriptionAttribute(VER_FILECOMMENTS)] ;
[assembly:AssemblyConfigurationAttribute(L"")] ;
[assembly:AssemblyCompanyAttribute(L"")] ;
[assembly:AssemblyProductAttribute(L"SONNET")] ;
[assembly:AssemblyCopyrightAttribute(VER_COPYRIGHT)] ;
[assembly:AssemblyTrademarkAttribute(VER_TRADEMARK)] ;
[assembly:AssemblyCultureAttribute(L"")] ;

[assembly:AssemblyVersionAttribute(VER_ASSEMBLYVERSION_STR)] ;
[assembly:AssemblyFileVersionAttribute(VER_FILEVERSION_STR)] ;

[assembly:ComVisible(false)] ;

[assembly:CLSCompliantAttribute(true)] ;