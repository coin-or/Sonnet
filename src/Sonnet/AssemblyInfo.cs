// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


// Specifies that types that are ordinarily visible only within the current
// assembly are visible to a specified assembly.
[assembly: InternalsVisibleTo("SonnetTest")]
[assembly: InternalsVisibleTo("SonnetTestNET4")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
#if NET6_0
#if WIN32
[assembly: AssemblyTitle("Sonnet-latest (unstable) (net6.0, x86)")]
#else
[assembly: AssemblyTitle("Sonnet-latest (unstable) (net6.0, x64)")]
#endif
#elif NET5_0
#if WIN32
[assembly: AssemblyTitle("Sonnet-latest (unstable) (net5.0, x86)")]
#else
[assembly: AssemblyTitle("Sonnet-latest (unstable) (net5.0, x64)")]
#endif
#elif NET48
#if WIN32
[assembly: AssemblyTitle("Sonnet-latest (unstable) (net48, x86)")]
#else
[assembly: AssemblyTitle("Sonnet-latest (unstable) (net48, x64)")]
#endif
#else
#error Sonnet AssemblyInfo unclear build
#endif
[assembly: AssemblyDescription("Sonnet offers a .NET modelling API for optimization problems.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("See https://github.com/coin-or/sonnet")]
[assembly: AssemblyProduct("Sonnet")]
[assembly: AssemblyCopyright("This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
// assembly: TargetFramework will contain .NETFramework or .NETCoreApp

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("31965bf3-860b-4643-b104-5b5be1957a65")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.9999")]
