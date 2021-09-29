# SONNET

SONNET is a modelling API and wrapper for COIN-OR mixed integer linear programming classes for the Microsoft .NET Framework.

SONNET uses a wrapper to make the COIN-OR C++ classes available in .NET.  
COIN-OR projects included in the wrapper:
[Cbc](https://github.com/coin-or/Cbc) 
[Cgl](https://github.com/coin-or/Cgl)
[Osi](https://github.com/coin-or/Osi)
[Clp](https://github.com/coin-or/Clp)

The SONNET API allows for code like this:

```C#
using COIN;  
Model model = new Model();
Variable x = new Variable();
Variable y = new Variable();
model.Add(2 * x + 3 * y <= 10);
model.Objective = 3 * x + y;
Solver solver = new Solver(model, typeof(COIN.OsiClpSolverInterface));
solver.Maximise();
```

You can find the latest binaries and sources from CI builds on [AppVeyor](https://ci.appveyor.com/project/coin-or/sonnet/build/artifacts)  

In particular, the latest binary release:
https://github.com/coin-or/SONNET/releases

Simply add the x86 or x64 libraries of `Sonnet.dll` and `SonnetWrapper.dll` to your References in Visual Studio.  
Requires .NET Framework 4.x or .NET 5.


## Building from source

### Folder structure

The SonnetWrapper expects the COIN-OR source code to be in the same root folder.  
The folder structure should look like this:

├─ [Cbc](https://github.com/coin-or/Cbc)  
├─ [Cgl](https://github.com/coin-or/Cgl)  
├─ [Clp](https://github.com/coin-or/Clp)  
├─ [CoinUtils](https://github.com/coin-or/CoinUtils)  
├─ [Osi](https://github.com/coin-or/Osi)  
└─ SONNET  

For Visual Studio 2019, the solution and project files can be found in `MSVisualStudio\v16`.


### Platform target (x64 or Win32/x86)

Specific builds are required for x86 or x64, in particular for `SonnetWrapper.dll`.  
Since the C++/CLI of `SonnetWraper.dll` has no "AnyCPU" equivalent, it is *NOT* possible to use
the x86 and/or x64 `Sonnet.dll` and `SonnetWrapper.dll` in an "AnyCPU" project. The project *has* to target x86 or x64.


### Strongly naming / Signing assemblies

To strongly name (sign) the `Sonnet.dll` and `SonnetWrapper.dll` there are two options:
1) Build the source code using your own key, or
2) Use `ILMERGE`  
   `ilmerge Weak.dll /keyfile:key.snk /out:Strong.dll`  
   https://github.com/dotnet/ILMerge/blob/master/ilmerge-manual.md
   

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

