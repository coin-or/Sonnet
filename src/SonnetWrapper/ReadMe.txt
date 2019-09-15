About SonnetWrapper
-------------------

The SonnetWrapper library consists of C++/CLI wrappers around 
native C++ classes of various COIN-OR projects. The main purpose 
of the SonnetWrapper library is its use in the overall 
Sonnet library (see there).

After a build, the resulting library is copied to the solution directoy.

For info about Strongly Naming the assemblies, see INSTALL.

The wrapper implementations are very simple:
Almost all have a private member called "Base" which is a pointer 
to the underlying native C++ class. The SonnetWrapper implementation 
then becomes 
	
	/// Get infinity
	double CoinMpsIO::getInfinity()
	{
		return Base->getInfinity();
	}

Because of such simple implementations, the implementation is often 
done within the header file.

About x64 or Win32/x86
-------------------------
Specific builds are required for x86 or x64, in particular for SonnetWrapper.dll.


About additional functionality
-------------------------
Several parts of the source code have been disabled because they require, e.g., non-standard COIN-OR code.

SONNET_LEANLOADPROBLEM (Solver.cs, See also SonnetWrapper)
  Especially for CLP, this functionality loads problems more efficiently but requires 
  non-standard osiClp code. This is really only interesting for large problems (>100K var/con)
  If SONNET_LEANLOADPROBLEM is defined, then two additional member functions of SonnetWrapper's OsiClpSolverInterface are defined
	- LeanLoadProblemInit
	- LeanLoadProblem
  and one extra member function must be added for normal OsiClpSolverInterface
	- loadCurrentProblem
