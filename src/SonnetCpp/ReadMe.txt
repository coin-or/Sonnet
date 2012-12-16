SonnetCpp aims to implement the .NET Sonnet library in C++.

In principle, let's use the following:
-use std::string for C# string
	in constructors, use "const std::string &foo"
-in c++, constructors cannot call other constructors of the same class; only base class constructors
	therefore, use a "GutsOfConstructor"/"init" method.
-use "friend class" and/or "protected" or "private" for internal, where applicable.

