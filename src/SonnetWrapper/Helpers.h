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

		/// <summary>
		/// Transfer the Base object to elsewhere, including the responsibility to eventually delete the base object.
		/// The current object will not delete the base.
		/// </summary>
		/// <returns>The underlying Base object</returns>
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

			/// <summary>
			/// Sets the base to the given value. 
			/// If a base already existed, then delete that object unless base was transferred.
			/// </summary>
			/// <param name="value">The new base</param>
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

		/// <summary>
		/// Construct a wrapper for an existing base object.
		/// The existing base object will not be deleted here.
		/// </summary>
		/// <param name="copy">The existing base object to be used</param>
		WrapperAbstractBase(const T* copy)
		{
			base = (T*) copy;
			deleteBase = false;
			disposed = 0;
		}
	
		/// <summary>
		/// Create a derived wrapper from an existing derived base object.
		/// To be implemented.
		/// </summary>
		/// <param name="derived">The derived base object to be used</param>
		/// <returns>The derived wrapper</returns>
		//static WrapperAbstractBase^ CreateDerived(const T* derived) = 0;

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
		/// <summary>
		/// Construct a wrapper and a new underlying base object.
		/// </summary>
		WrapperBase() : WrapperAbstractBase()
		{
			Base = new T(); 
		}

		/// <summary>
		/// Construct a wrapper for an existing base object.
		/// The existing base object will not be deleted here.
		/// </summary>
		/// <param name="copy">The existing base object to be used</param>
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
