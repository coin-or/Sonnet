Sonnet
--------------------------------------------------
The Sonnet assembly ('library', 'assembly' and 'project' used as if synonymous) 
is the other half of the SONNET project. 
SonnetWrapper (C++/CLI) makes the .NET link to the native C++ COIN libraries, 
and Sonnet (C#) offers the modelling API.

For info about Strongly Naming the assemblies, see INSTALL.

About 32-bit vs 64-bit
-------------------------
Because C++/CLI projects like SonnetWrapper cannot be built in an "Any CPU" mode that would
work as both 32-bit (x86/win32) and 64-bit (x64), this also has implications for Sonnet.
Eventhough Sonnet _can_ be built to target "Any CPU", this will fail at run time whenever the
SonnetWrapper library is called.
Therefore, the Sonnet assembly doesn't have an Any CPU solution platform.


About Debug vs Release
-------------------------
At runtime, is it OK to mix Release and Debug mode Sonnet and SonnetWrapper builds.


About additional functionality
-------------------------
Several parts of the source code have been disabled because they require, e.g., non-standard COIN-OR code.

SONNET_LEANLOADPROBLEM (Solver.cs, See also SonnetWrapper)
  Especially for CLP, this functionality loads problems more efficiently but requires 
  non-standard osiClp code. This is really only interesting for large problems (>100K var/con)
SONNET_USE_SEMICONTVAR (Solver.cs)
  Unfinished code for adding a variable type of SemiContinuousVariable, and automatically generate
  helper variable (binary) and constraint.
SONNET_SETWARMROWPRICE (WarmStart.cs)
  Whether or not to include row price information in WarmStart
  
