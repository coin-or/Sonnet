# Sonnet

Sonnet is a modelling API and wrapper for COIN-OR mixed integer linear programming classes for the Microsoft .NET on **Windows** platforms.

Sonnet uses a wrapper to make the COIN-OR C++ classes available in .NET.  
COIN-OR projects included in the wrapper: 
[BuildTools](https://github.com/coin-or-tools/BuildTools), 
[Cbc](https://github.com/coin-or/Cbc),  
[Cgl](https://github.com/coin-or/Cgl), 
[Clp](https://github.com/coin-or/Clp), 
[Osi](https://github.com/coin-or/Osi).

The Sonnet API allows for code like this:

```C#
using Sonnet;  

Model model = new Model();
Variable x = new Variable();
Variable y = new Variable();
model.Add(2 * x + 3 * y <= 10);

var expr = new Expression(); // Useful in for-loops
expr.Add(3.0, x);
expr.Add(y);
model.Objective = expr; // 3*x + y

Solver solver = new Solver(model);
solver.Maximise();
```

You can find the latest binaries and sources from CI builds on [AppVeyor](https://ci.appveyor.com/project/coin-or/sonnet/build/artifacts) (if available).

In particular, the latest binary release:
https://github.com/coin-or/Sonnet/releases

Simply add the x86 or x64 libraries of `Sonnet.dll` and `SonnetWrapper.dll` to your References in Visual Studio.  
Requires .NET Framework 4.x or .NET 5. 

Sonnet can only be used on **Windows** platforms. While, in principle, [.NET 5 supports cross-platform use](https://docs.microsoft.com/en-us/dotnet/core/introduction), this 
[does not include the mixed C++/CLI](https://docs.microsoft.com/en-us/dotnet/core/porting/cpp-cli) of SonnetWrapper, 
where the native C++ of the COIN-OR C++ is mixed with C++/CLI.

Note: If you use the .NET 5 libraries, then also include `Ijwhost.dll` in your project and set this file to Always Copy to Output folder to prevent runtime errors of `System.BadImageFormatException`.


## Building from source

### Getting the source code

#### Option 1: Get the full src.zip artifact from AppVeyor
The easiest way to get started with Sonnet source code is to use the ..-src.zip artefact (if available) from the latest 
[AppVeyor build artefacts](https://ci.appveyor.com/project/coin-or/sonnet/build/artifacts). This zip contains the Sonnet sources as well as all the relevant COIN-OR dependencies from their master branch. 
Simply unzip to have all the sources in the right place.
Unfortunately, AppVeyor only keeps the artefacts for one month after the build.

#### Option 2: Get sources from GitHub
The latest Sonnet sources can be downloaded from the [github repository](https://github.com/coin-or/Sonnet).
If you use "Download ZIP", the downloaded zip file contains the Sonnet source code and MS Visual Studio 2019 solution. 

Before you can build Sonnet, the sources of the master branches of all the relevant COIN-OR dependencies are also required. 
See https://github.com/coin-or/Cbc#source how to get the relevant sources. 

The Sonnet solution expects the COIN-OR source code to be in the same root folder. 
The folder structure should look like this:

├─ [BuildTools](https://github.com/coin-or-tools/BuildTools)  
├─ [Cbc](https://github.com/coin-or/Cbc)  
├─ [Cgl](https://github.com/coin-or/Cgl)  
├─ [Clp](https://github.com/coin-or/Clp)  
├─ [CoinUtils](https://github.com/coin-or/CoinUtils)  
├─ [Osi](https://github.com/coin-or/Osi)  
└─ Sonnet  

### Building in Visual Studio
For Visual Studio 2019, the solution and project files can be found in `Sonnet\MSVisualStudio\v16`.
Simply open the Sonnet solution in `Sonnet\MSVisualStudio\v16\Sonnet.sln`, and Build. 

The Sonnet projects and solution are provided for Visual Studio 2019 (v16).
The Sonnet solution for Visual Studio 2019 refers to the project files of the relevant COIN-OR libraries.

Solution configurations for Debug, Release and ReleaseParallel are defined for the solution, each with x86 and x64 solution platforms.

Older versions of Sonnet projects and solutions files are available for Visual Studio 2010 (v10), but these are not maintained.

### Platform target (x64 or Win32/x86)

Separate builds are required for x86 or x64, in particular for `SonnetWrapper.dll`.  
Since the C++/CLI of `SonnetWraper.dll` has no "AnyCPU" equivalent, it is *NOT* possible to use
the x86 and/or x64 `Sonnet.dll` and `SonnetWrapper.dll` in an "AnyCPU" project. The project *has* to target x86 or x64.

### Parallel version of Cbc
For Windows builds of Cbc, the standard method to build Sonnet and dependencies will build a single threaded Cbc. However, Cbc can also run with parallel threads to speed up the search. 
Rudimentary steps how to build a parallel version of Cbc in Windows are available [in the Cbc README](https://github.com/coin-or/Cbc/#with-microsoft-visual-studio). 
Here I'll describe the steps in more detail, assuming you already have a setup where you can build Sonnet etc. in Visual Studio 2019.

Several pthread for Windows libraries are available. Sonnet was succesfully tested to work with [GerHobbelt/pthread-win32](https://github.com/GerHobbelt/pthread-win32).
To build Sonnet with pthread, all necessary project settings are available in the respective ReleaseParallel configurations of libCbc, libCbcSolver and SonnetWrapper:
- Solution: Use the ReleaseParallel configuration.
- Define: CBC_THREAD is defined by the projects
- Include: "pthreads" include folder is expected at one level above the Sonnet root folder, so _not_ besides BuildTools, etc., but one level above.
- Linker: SonnetWrapper links to pthread_static_lib.lib that is expected in the lib\Win32 or lib\x64 folder at the root of Sonnet folder. You have to build this first, below.
 
Start by cloning the pthread repo, and opening the solution in VS2019. You can decide to build the pthread dll or the pthread static lib. 
Build the Release configurmation for either the x64 or Win32 platform, depending on your needs.
Within Cbc, the only projects that actually use pthread are libCbc and libCbcSolver.
Since these come together in SonnetWrapper, SonnetWrapper also links to the pthread library in the ReleaseParallel configuration.

SonnetWrapper has been configured to use the pthread_static_lib.lib, which looks for in the Sonnet/lib folder.
Should you want to use pthread as a DLL instead, then copy the pthread.lib to Sonnet/lib and change the SonnetWrapper property for Linker -> Input to pthread.lib instead of pthread_static_lib.lib and ensure that the pthread.dll is copied to the output directory.
Note: cbc (cbc.exe) project has been configured to use the pthread.lib and pthread.dll, but this project is not used by Sonnet.


### Additional functionality

Several parts of the source code have been disabled because they require, e.g., non-standard COIN-OR code.


#### SONNET_LEANLOADPROBLEM (Solver.cs, see also SonnetWrapper)

Especially for CLP, this functionality loads problems more efficiently but requires
non-standard osiClp code. This is really only interesting for large problems (>100K var/con).  
If `SONNET_LEANLOADPROBLEM` is defined, then two additional member functions of SonnetWrapper's OsiClpSolverInterface are defined:
- `LeanLoadProblemInit`
- `LeanLoadProblem`  

and one extra member function must be added for normal OsiClpSolverInterface
- `loadCurrentProblem`
  

#### SONNET_USE_SEMICONTVAR (Solver.cs)

  Unfinished code for adding a variable type of `SemiContinuousVariable`, and automatically generate
  helper variable (binary) and constraint.


#### SONNET_SETWARMROWPRICE (WarmStart.cs)

Whether or not to include row price information in WarmStart

