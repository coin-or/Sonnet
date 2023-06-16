// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).
#include "AssemblyInfo.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly:AssemblyTitleAttribute(VER_FILENAME)];
[assembly:AssemblyDescriptionAttribute(VER_FILECOMMENTS)];
[assembly:AssemblyConfigurationAttribute("")];
[assembly:AssemblyCompanyAttribute("")];
[assembly:AssemblyProductAttribute("SONNET")];
[assembly:AssemblyCopyrightAttribute(VER_COPYRIGHT)];
[assembly:AssemblyTrademarkAttribute(VER_TRADEMARK)];
[assembly:AssemblyCultureAttribute("")];
//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the value or you can default the Revision and Build Numbers
// by using the '*' as shown below:

[assembly:AssemblyVersionAttribute(VER_ASSEMBLYVERSION_STR)];
[assembly:AssemblyFileVersionAttribute(VER_FILEVERSION_STR)];
[assembly:ComVisible(false)];

[assembly:CLSCompliantAttribute(true)];

//obsolete
//[assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];
