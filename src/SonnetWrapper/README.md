SonnetWrapper
-------------

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
```c++	
/// Get infinity
double CoinMpsIO::getInfinity()
{
    return Base->getInfinity();
}
```

Because of such simple implementations, the implementation is often 
done within the header file.

See also INSTALL.