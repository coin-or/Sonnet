# Sonnet

Sonnet is a modelling API and wrapper for COIN-OR mixed integer linear programming classes for the Microsoft .NET.

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
model.Objective = 3 * x + y;
Solver solver = new Solver(model);
solver.Maximise();
```

You can find the latest binaries and sources from CI builds on [AppVeyor](https://ci.appveyor.com/project/coin-or/sonnet/build/artifacts) (if available).

In particular, the latest binary release:
https://github.com/coin-or/Sonnet/releases

Simply add the x86 or x64 libraries of `Sonnet.dll` and `SonnetWrapper.dll` to your References in Visual Studio.  
Requires .NET Framework 4.x or .NET 5. 

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


### Strongly naming / Signing assemblies

To strongly name (sign) the `Sonnet.dll` and `SonnetWrapper.dll` there are two options:
1) Build the source code using your own key, or
2) Use [ILMerge](https://github.com/dotnet/ILMerge/blob/master/ilmerge-manual.md):
   `ilmerge Weak.dll /keyfile:key.snk /out:Strong.dll`  


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

