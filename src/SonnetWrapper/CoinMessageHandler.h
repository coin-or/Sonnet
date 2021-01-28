// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <gcroot.h>
#include <CoinMessageHandler.hpp>

#include "Helpers.h"

using namespace System;

class OsiSolverInterface;

namespace COIN
{
	ref class OsiSolverInterface;
	ref class CoinMessageHandler;
		public ref class CoinOneMessage : public WrapperBase<::CoinOneMessage>
	{
	public:
		CoinOneMessage() {}
		CoinOneMessage(int externalNumber, int detail, String ^ message)
		{
			/* 
			If the log level is equal or greater than the detail level of a message,
				the message will be printed. A rough convention for the amount of output
				expected is
				- 0 - none
				- 1 - minimal
				- 2 - normal low
				- 3 - normal high
				- 4 - verbose

			CoinMessageHandler assumes the following relationship between the
				external ID number of a message and the severity of the message:
				\li <3000 are informational ('I')
				\li <6000 warnings ('W')
				\li <9000 non-fatal errors ('E')
				\li >=9000 aborts the program (after printing the message) ('S')
			*/
			char* messageText = (char*)Marshal::StringToHGlobalAnsi(message).ToPointer();
			
			Base = new ::CoinOneMessage(externalNumber, (char)detail, messageText);

			Marshal::FreeHGlobal((IntPtr)messageText);
		}
	};

	public ref class CoinMessages : public WrapperBase<::CoinMessages>
	{
	public:
		/** Constructor with number of messages. */
		CoinMessages() {}
		CoinMessages(int numberMessages) 
		{
			Base = new ::CoinMessages(numberMessages);
		}

		property String ^ source
		{
			String ^ get()
			{
				return gcnew String(Base->source_);
			}

			void set(String ^value)
			{
				if (value->Length > 4) value = value->Substring(0,4);

				char* charValue = (char*)Marshal::StringToHGlobalAnsi(value).ToPointer();
			
				strcpy_s(Base->source_, charValue); 

				Marshal::FreeHGlobal((IntPtr)charValue);
			}
		}

		/*! \brief Installs a new message in the specified index position

		Any existing message is replaced, and a copy of the specified message is
		installed.
		*/
		void addMessage(int messageNumber, CoinOneMessage ^ message)
		{
			Base->addMessage(messageNumber, *(message->Base));
		}
	};

	/// <summary>
	/// This class is a native class derived from native CoinMessageHandler.
	/// As such, proxy objects can be passed to native classes, as native CoinMessageHandlers.
	/// Proxy objects are created for a wrapper CoinMessageHandler (not native).
	/// </summary>
	public class CoinMessageHandlerProxy : public ::CoinMessageHandler
	{
		friend ref class OsiSolverInterface;
		friend ref class CoinMessageHandler;

	public:
		CoinMessageHandlerProxy(gcroot<COIN::CoinMessageHandler ^> wrapper);

		/** The copy constructor */
		CoinMessageHandlerProxy(const CoinMessageHandlerProxy& rhs)
			: ::CoinMessageHandler(rhs)
		{
			this->wrapper = rhs.wrapper;
		}
		
		/** Assignment operator. */
		CoinMessageHandlerProxy& operator=(const CoinMessageHandlerProxy&rhs)
		{
			if (this != &rhs)
			{
				CoinMessageHandler::operator=(rhs);
				this->wrapper = rhs.wrapper;
			}
			return *this;
		}

		/// Clone
		virtual CoinMessageHandler * clone() const
		{
			return new CoinMessageHandlerProxy(*this);
		}

		virtual int print();

	private:
		CoinMessageHandlerProxy(const ::CoinMessageHandler &rhs, gcroot<COIN::CoinMessageHandler ^> wrapper)
			: ::CoinMessageHandler(rhs)
		{
			this->wrapper = wrapper;
		}

		int printBase()
		{
			return ::CoinMessageHandler::print();
		}

		gcroot<COIN::CoinMessageHandler ^> wrapper;
	};

	public ref class CoinMessageHandler : public IDisposable
	{
	public: 
		// Use this constructor to use a proxy class to PassIn to native Coin classes
		CoinMessageHandler()
		{
			base = new CoinMessageHandlerProxy(this);
			baseIsProxy = true;
			//callstack = gcnew String(Environment::StackTrace); // used to debug where this object was created.
		}

		virtual int logLevel()
		{
			return base->logLevel();
		}
		virtual void setLogLevel(int value)
		{
			base->setLogLevel(value);
		}

		/** Get alternative log level. */
		virtual int logLevel(int which)
		{
			return base->logLevel(which);
		}
		/*! \brief Set alternative log level value.

			Can be used to store alternative log level information within the handler.
		*/
		virtual void setLogLevel(int which, int value)
		{
			base->setLogLevel(which, value);
		}

		virtual int print()
		{
			// don't call proxy->print() since that will call *this* method again (wrapper->print())
			CoinMessageHandlerProxy *proxy = dynamic_cast<CoinMessageHandlerProxy *>(base);
			if (proxy != nullptr) return proxy->printBase();
			else return base->print();
		}

		String ^ messageBuffer()
		{
			return gcnew String(base->messageBuffer());
		}

		CoinMessageHandler ^ message(int messageNumber, CoinMessages ^ m,  ... array<Object^>^ variableArgs )
		{
			::CoinMessageHandler &h = base->message(messageNumber, *(m->Base));

			int n = variableArgs->Length;
			for (int i = 0; i < n; i++)
			{
				Object ^ o = variableArgs[i];
				if (o == nullptr) o = String::Empty;

				//Type ^ t = int::typeid;
				if (o->GetType() == int::typeid) h << safe_cast<int>(o);
				else if (o->GetType() == char::typeid) h << safe_cast<char>(o);
				else if (o->GetType() == long::typeid) h << safe_cast<long>(o);
				//else if (o->GetType() == long long::typeid) h << safe_cast<long long>(o);
				else if (o->GetType() == double::typeid) h << safe_cast<double>(o);
				else
				{
					// use string
					String ^text = o->ToString();
					char * charText = (char*)Marshal::StringToHGlobalAnsi(text).ToPointer();
					h << charText;
					Marshal::FreeHGlobal((IntPtr)charText);
				}
			}
			return this;
		}

	internal:
		// Use this Constructor to create a wrapper around the given native CoinMessageHandler.
		// In this case, no proxy is used, and also the rhs is expected to be deleted by the owning object, not here.
		CoinMessageHandler(::CoinMessageHandler* rhs)
		{
			base = rhs;
			baseIsProxy = false;
			//callstack = gcnew String(Environment::StackTrace); // used to debug where this object was created.
		}

		property bool HasProxy 
		{ 
			bool get() 
			{ 
				return baseIsProxy && nullptr != dynamic_cast<CoinMessageHandlerProxy *>(base); 
			}
		}

		property ::CoinMessageHandler * Base 
		{
			::CoinMessageHandler * get()
			{
				return base;
			}
		}

	private:
		bool freezeLogLevel;
		::CoinMessageHandler *base;
		bool baseIsProxy;
		//String^ callstack;

		int disposed;
		~CoinMessageHandler()
		{
			// Note: it should be possible to Dispose multiple times without throwing exceptions!
			// After the first time, do nothing. Hence, we should keep a count!
			// See also http://msdn.microsoft.com/en-us/library/b1yfkh5e(VS.80).aspx
			if (disposed > 0) return;

			// delete MANAGED stuff here

			// END delete MANAGED stuff here

 			disposed++;

			this->!CoinMessageHandler();
		}

		!CoinMessageHandler()
		{
			// delete NATIVE stuff here: the proxy

			if (baseIsProxy)
			{
				// Note: If the base object is owned by a native object, then that native object is expected to delete it.
				// In such a case, the base is not a Proxy. But if the base is already deleted, then the type check will fail.
				// Therefore, the baseIsProxy was added.S
				CoinMessageHandlerProxy* proxy = dynamic_cast<CoinMessageHandlerProxy*>(base);
				if (proxy != nullptr)
				{
					delete base;
					base = nullptr;
				}
			}
		}

	};
}