// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

class CglCutGenerator;
class CbcStrategy;

namespace COIN
{
	ref class CglCutGenerator;
	ref class CbcStrategy;

	template<class T>
	public ref class WrapperAbstractBase
	{
	internal:
		bool deleteBase; 
		T* TransferBase()
		{
			deleteBase = false;
			return base;
		}

	public:
		property T * Base 
		{
			T * get() 
			{ 
				return base; 
			}
		
			protected:
			void set(T* value)
			{
				if (!base) DeleteBase();
				base = value;
			}
		}

	protected:
		void DeleteBase()
		{
			if (deleteBase) delete base; 
			base = nullptr;
		}

		WrapperAbstractBase()
		{
			// for (abstract) base classes, constructing here DOESNT make sense
			// for other classes, it would be nice to "new T()" here
			base = nullptr; 
			deleteBase = true;
			disposed = 0;
		}

		WrapperAbstractBase(const T* copy)
		{
			base = (T*) copy;
			deleteBase = false;
			disposed = 0;
		}
	
	private:
		int disposed;
		T* base;
		~WrapperAbstractBase()
		{
			// Note: it should be possible to Dispose multiple times without throwing exceptions!
			// After the first time, do nothing. Hence, we should keep a count!
			// See also http://msdn.microsoft.com/en-us/library/b1yfkh5e(VS.80).aspx
			if (disposed > 0) return;

			// delete MANAGED stuff here

			// END delete MANAGED stuff here

 			disposed++;

			this->!WrapperAbstractBase();
		}

		!WrapperAbstractBase()
		{
			// delete NATIVE stuff here
			DeleteBase();
		}
	};

	template<class T>
	public ref class WrapperBase : public WrapperAbstractBase<T>
	{
	protected:
		WrapperBase() : WrapperAbstractBase()
		{
			// for (abstract) base classes, constructing here DOESNT make sense
			// for other classes, it would be nice to "new T()" here
			Base = new T(); 
		}

		WrapperBase(const T* copy) : WrapperAbstractBase(copy)
		{
		}
	};

	template<typename T>
	inline cli::interior_ptr<T> GetPinablePtr(array<T> ^t)
	{
		return (t == nullptr || t->LongLength == 0)?nullptr:&t[0];
	}
}
