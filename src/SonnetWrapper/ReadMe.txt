About SonnetWrapper
-------------------

The SonnetWrapper library consists of C++/CLI wrappers around 
native C++ classes of various COIN projects. The main purpose 
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

About 64-bit support in Visual Studio Express:
From http://msdn.microsoft.com/en-us/library/9yb4317s(v=VS.100).aspxc
"64-bit tools are not available on Visual C++ Express by default. 
To enable 64-bit tools on Visual C++ Express, 
install the Windows Software Development Kit (SDK) 
in addition to Visual C++ Express."