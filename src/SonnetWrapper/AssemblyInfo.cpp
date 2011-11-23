// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

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
[assembly:AssemblyTitleAttribute("SonnetWrapper")];
[assembly:AssemblyDescriptionAttribute("SonnetWrapper is a managed DLL with wrapper classes around existing C++ COIN classes")];
[assembly:AssemblyConfigurationAttribute("")];
[assembly:AssemblyCompanyAttribute("")];
[assembly:AssemblyProductAttribute("SONNET")];
[assembly:AssemblyCopyrightAttribute("Copyright © 2011")];
[assembly:AssemblyTrademarkAttribute("This code is licensed under the terms of the Eclipse Public License (EPL)")];
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

[assembly:AssemblyVersionAttribute("1.0.0.0")];
[assembly:AssemblyFileVersionAttribute("1.0.0.1")];
[assembly:ComVisible(false)];

[assembly:CLSCompliantAttribute(true)];

[assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];
