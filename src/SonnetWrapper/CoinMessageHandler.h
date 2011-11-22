// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
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
			
				strcpy(Base->source_, charValue); // warning to use strcpy_s

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

	public class CoinMessageHandlerProxy : public ::CoinMessageHandler
	{
		friend ref class OsiSolverInterface;
	public:
		CoinMessageHandlerProxy(gcroot<COIN::CoinMessageHandler ^> wrapper);

		virtual int print();

	private:
		gcroot<COIN::CoinMessageHandler ^> wrapper;
	};

	public ref class CoinMessageHandler : public IDisposable
	{
	public: 
		CoinMessageHandler()
		{
			proxy = new CoinMessageHandlerProxy(this);
		}

		virtual int logLevel()
		{
			return proxy->logLevel();
		}

		virtual void setLogLevel(int value)
		{
			proxy->setLogLevel(value);
		}

		virtual int print()
		{
			return 0;
		}

		String ^ messageBuffer()
		{
			return gcnew String(proxy->messageBuffer());
		}

		CoinMessageHandler ^ message(int messageNumber, CoinMessages ^ m,  ... array<Object^>^ variableArgs )
		{
			::CoinMessageHandler &h = proxy->message(messageNumber, *(m->Base));

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
		CoinMessageHandlerProxy *proxy;
	private:
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
			// delete NATIVE stuff here
			delete proxy; 
			proxy = nullptr;
		}

	};
}