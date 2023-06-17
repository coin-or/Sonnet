// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

#include "CoinError.h"
#include <CoinError.hpp>

namespace COIN
{
	CoinError::CoinError(::CoinError &err)
		:ApplicationException(GetMessage(err), gcnew System::Runtime::InteropServices::SEHException(GetInnerMessage(err)))
	{
	}

	String ^ CoinError::GetMessage(::CoinError &err)
	{
		String ^ message = gcnew String(err.message().c_str());
		return message;
	}
	String ^ CoinError::GetInnerMessage(::CoinError &err)
	{
		String ^ message = gcnew String(err.message().c_str());
		String ^ fileName = gcnew String(err.fileName().c_str());
		String ^ className = gcnew String(err.className().c_str());
		int lineNumber = err.lineNumber();
		String ^ methodName = gcnew String(err.methodName().c_str());

		String ^ innerMessage = String::Concat("CoinError: ", message, Environment::NewLine, "   at ", className, ".", methodName, "(...)");
		if (fileName->Length > 0) innerMessage = String::Concat(innerMessage, " in ", fileName, ":line ", lineNumber);

		return innerMessage;
	}
}