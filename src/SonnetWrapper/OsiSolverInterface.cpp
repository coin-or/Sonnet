// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "CoinUtils.h"
#include "OsiSolverInterface.h"
#include "CoinError.h"

#include "OsiDerivedSolverInterfaces.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	void OsiSolverInterface::branchAndBound()
	{
		try
		{
			// some of the Osi mess up the logLevel, so reset
			int saveLogLevel = 0;
			if (messageHandler()) saveLogLevel = messageHandler()->logLevel(); 

			Base->branchAndBound();

			if (messageHandler()) messageHandler()->setLogLevel(saveLogLevel);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::initialSolve()
	{
		try
		{
			Base->initialSolve();
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::resolve()
	{
		try
		{
			Base->resolve();
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	double OsiSolverInterface::getInfinity()
	{
		try
		{
			return Base->getInfinity();
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::writeMps(String ^fileName)
	{
		try
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			Base->writeMps(charFileName);
			Marshal::FreeHGlobal((IntPtr)charFileName);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}

	}

	void OsiSolverInterface::writeMps(String ^fileName, String ^extension)
	{
		try
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			char * charExtension = (char*)Marshal::StringToHGlobalAnsi(extension).ToPointer();
			Base->writeMps(charFileName,charExtension);
			Marshal::FreeHGlobal((IntPtr)charFileName);
			Marshal::FreeHGlobal((IntPtr)charExtension);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}

	}

	void OsiSolverInterface::writeMps(String ^fileName, String ^extension, double objSense)
	{
		try
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			char * charExtension = (char*)Marshal::StringToHGlobalAnsi(extension).ToPointer();
			Base->writeMps(charFileName,charExtension, objSense);
			Marshal::FreeHGlobal((IntPtr)charFileName);
			Marshal::FreeHGlobal((IntPtr)charExtension);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}

	}
	void OsiSolverInterface::writeLp(String ^fileName)
	{
		try
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			Base->writeLp(charFileName);
			Marshal::FreeHGlobal((IntPtr)charFileName);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::writeLp(String ^fileName, String ^extension)
	{
		try
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			char * charExtension = (char*)Marshal::StringToHGlobalAnsi(extension).ToPointer();

			Base->writeLp(charFileName,charExtension);

			Marshal::FreeHGlobal((IntPtr)charFileName);
			Marshal::FreeHGlobal((IntPtr)charExtension);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::writeLp(String ^fileName, String ^extension, double epsilon, 
		int numberAcross, int decimals, double objSense, bool useRowNames)
	{
		try
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			char * charExtension = (char*)Marshal::StringToHGlobalAnsi(extension).ToPointer();

			Base->writeLp(charFileName,charExtension, epsilon, numberAcross, decimals, objSense, useRowNames);

			Marshal::FreeHGlobal((IntPtr)charFileName);
			Marshal::FreeHGlobal((IntPtr)charExtension);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::addRow(int numberElements, array<int> ^columns, array<double> ^elements, 
		double rowlb, double rowub)
	{
		try
		{
			// this method makes a copy!
			pin_ptr<int> columnsPinned = GetPinablePtr(columns);   // entire array is now pinned
			pin_ptr<double> elementsPinned = GetPinablePtr(elements); // when the pin_ptr goes out of scope, the array is unpinned
			Base->addRow(numberElements, columnsPinned, elementsPinned, rowlb, rowub);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	bool OsiSolverInterface::setIntParam(COIN::OsiIntParam key, int value)
	{
		return Base->setIntParam(GetOsiIntParam(key), value);
	}
	bool OsiSolverInterface::setDblParam(COIN::OsiDblParam key, double value)
	{
		return Base->setDblParam(GetOsiDblParam(key), value);
	}
	bool OsiSolverInterface::setStrParam(COIN::OsiStrParam key, String ^ value)
	{
		char * chars = (char*)Marshal::StringToHGlobalAnsi(value).ToPointer();
		bool result = Base->setStrParam(GetOsiStrParam(key), chars);
		Marshal::FreeHGlobal((IntPtr)chars);
		return result;
	}
	bool OsiSolverInterface::setHintParam(COIN::OsiHintParam key)
	{
		return Base->setHintParam(GetOsiHintParam(key));
	}
	bool OsiSolverInterface::setHintParam(COIN::OsiHintParam key, bool yesNo)
	{
		return Base->setHintParam(GetOsiHintParam(key), yesNo);
	}
	bool OsiSolverInterface::setHintParam(COIN::OsiHintParam key, bool yesNo, COIN::OsiHintStrength strength)
	{
		return Base->setHintParam(GetOsiHintParam(key), yesNo, GetOsiHintStrength(strength));
	}
	bool OsiSolverInterface::setHintParam(COIN::OsiHintParam key, bool yesNo, COIN::OsiHintStrength strength, void * otherInformation)
	{
		return Base->setHintParam(GetOsiHintParam(key), yesNo, GetOsiHintStrength(strength), otherInformation);
	}
	bool OsiSolverInterface::getIntParam(COIN::OsiIntParam key,  [Out] int% value)
	{
		int nativeValue;
		bool result = Base->getIntParam(GetOsiIntParam(key), nativeValue);
		value = nativeValue;
		return result;
	}
	bool OsiSolverInterface::getDblParam(COIN::OsiDblParam key, [Out] double% value)
	{
		double nativeValue;
		bool result = Base->getDblParam(GetOsiDblParam(key), nativeValue);
		value = nativeValue;
		return result;
	}

	bool OsiSolverInterface::getStrParam(COIN::OsiStrParam key, [Out] String^ % value)
	{
		std::string nativeValue;
		bool result = Base->getStrParam(GetOsiStrParam(key), nativeValue);
		value = gcnew String(nativeValue.c_str());
		return result;
	}
	bool OsiSolverInterface::getHintParam(COIN::OsiHintParam key, [Out] bool% yesNo, [Out] COIN::OsiHintStrength% strength, [Out] void *% otherInformation)
	{
		bool nativeYesNo;
		::OsiHintStrength nativeStrength;
		void * nativeOtherInformation;
		bool result = Base->getHintParam(GetOsiHintParam(key), nativeYesNo, nativeStrength, nativeOtherInformation);
		yesNo = nativeYesNo;
		strength = GetOsiHintStrength(nativeStrength);
		otherInformation = nativeOtherInformation;
		return result;
	}
	bool OsiSolverInterface::getHintParam(OsiHintParam key, [Out] bool% yesNo, [Out] COIN::OsiHintStrength% strength)
	{
		void *otherInformation;
		return getHintParam(key, yesNo, strength, otherInformation);
	}
	bool OsiSolverInterface::getHintParam(OsiHintParam key, [Out] bool% yesNo)
	{
		COIN::OsiHintStrength strength;
		void *otherInformation;
		return getHintParam(key, yesNo, strength, otherInformation);
	}

	void OsiSolverInterface::setRowName(int index, String ^ name)
	{
		try
		{
			char * charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			Base->setRowName(index, charName);
			Marshal::FreeHGlobal((IntPtr)charName);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setRowBounds(int index, double lower, double upper)
	{
		try
		{
			Base->setRowBounds(index, lower, upper);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setRowUpper(int index, double upper)
	{
		try
		{
			Base->setRowUpper(index, upper);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setRowLower(int index, double lower)
	{
		try
		{
			Base->setRowLower(index, lower);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setRowType(int index, char sense, double rhs, double range)
	{
		try
		{
			Base->setRowType(index, sense, rhs, range);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::setColName(int index, String ^ name)
	{
		try
		{
			char * charName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			Base->setColName(index, charName);
			Marshal::FreeHGlobal((IntPtr)charName);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setColUpper(int index, double upper)
	{
		try
		{
			Base->setColUpper(index, upper);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setColLower(int index, double lower)
	{
		try
		{
			Base->setColLower(index, lower);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setColBounds(int index, double lower, double upper)
	{
		try
		{
			Base->setColBounds(index, lower, upper);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	bool OsiSolverInterface::isContinuous(int index)
	{
		try
		{
			return Base->isContinuous(index);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setContinuous(int index)
	{
		try
		{
			Base->setContinuous(index);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	
	bool OsiSolverInterface::isInteger(int index)
	{
		try
		{
			return Base->isInteger(index);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setInteger(int index)
	{
		try
		{
			Base->setInteger(index);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::setObjCoeff(int index, double value)
	{
		try
		{
			Base->setObjCoeff(index, value);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	double OsiSolverInterface::getObjSense()
	{
		try
		{
			return Base->getObjSense();
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::setObjSense(double sense)
	{
		try
		{
			Base->setObjSense(sense);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::setObjective(array<double> ^coefs)
	{
		try
		{
			pin_ptr<double> coefsPinned = GetPinablePtr(coefs);
			Base->setObjective(coefsPinned);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	CoinWarmStart ^ OsiSolverInterface::getEmptyWarmStart()
	{
		return gcnew CoinWarmStart(Base->getEmptyWarmStart());
	}

	CoinWarmStart ^ OsiSolverInterface::getWarmStart()
	{
		return gcnew CoinWarmStart(Base->getWarmStart());
	}

	bool OsiSolverInterface::setWarmStart(CoinWarmStart ^ warmstart)
	{
		return Base->setWarmStart(warmstart->Base);
	}

	CoinMessageHandler ^ OsiSolverInterface::messageHandler()
	{
		// this will return the base messagehandler IF it is the .net class wrapped.
		// so, of the coin class had a default handler,but we didnt wrap that,
		// then here we wont get a wrapped handler.
		// What if the base Has a handler that is Not our .net class?
		// we wrap it here, but don't pass it in (that would delete the original)
		// This only works if our CoinMessageHandler DOESNT have an overriden print functionality, because that wont be called.
		// In case we implement specific print(), we must 'passIn' that message handler explicitly.
		// Don't replace an existing handler at the construction of osisolver by a wrapper handler!

		::CoinMessageHandler *handler = Base->messageHandler();
		if (handler != nullptr)
		{
			CoinMessageHandlerProxy *proxy = dynamic_cast<CoinMessageHandlerProxy *>(handler);
			if (proxy != nullptr)
			{
				return proxy->wrapper;
			}
			else
			{
				// there is a COIN message handler, but it isnt a Proxy.
				return gcnew CoinMessageHandler(handler);
    		}
		}
		return nullptr;
	}

	void OsiSolverInterface::passInMessageHandler(CoinMessageHandler ^wrapper)
	{
		Base->passInMessageHandler(wrapper->Base);
	}

	int OsiSolverInterface::getNumCols()
	{
		return Base->getNumCols();
	}
	int OsiSolverInterface::getNumRows()
	{
		return Base->getNumRows();
	}
	int OsiSolverInterface::getNumIntegers()
	{
		return Base->getNumIntegers();
	}

	int OsiSolverInterface::getNumElements()
	{
		return Base->getNumElements();
	}


	double OsiSolverInterface::getObjValue()
	{
		return Base->getObjValue();
	}
	const double *OsiSolverInterface::getColSolutionUnsafe()
	{
		return Base->getColSolution();
	}
	array<double> ^ OsiSolverInterface::getColSolution()
	{
		int n = Base->getNumCols();
		if (n == 0) return nullptr;

		double *input = (double *) Base->getColSolution();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	void OsiSolverInterface::setColSolutionUnsafe(const double *colsol)
	{
		try
		{
			Base->setColSolution(colsol);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::setColSolution(array<double> ^colsol)
	{
		pin_ptr<double> colsolPinned = GetPinablePtr(colsol);
		setColSolutionUnsafe(colsolPinned);
	}

	const double *OsiSolverInterface::getReducedCostUnsafe()
	{
		return Base->getReducedCost();
	}
	array<double> ^ OsiSolverInterface::getReducedCost()
	{
		int n = Base->getNumCols();
		if (n == 0) return nullptr;

		double *input = (double *)Base->getReducedCost();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}
	const double *OsiSolverInterface::getRowPriceUnsafe()
	{
		return Base->getRowPrice();
	}
	array<double> ^ OsiSolverInterface::getRowPrice()
	{
		int n = Base->getNumRows();
		if (n == 0) return nullptr;

		double *input = (double *)Base->getRowPrice();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}
	void OsiSolverInterface::setRowPriceUnsafe(const double *rowprice)
	{
		try
		{
			Base->setRowPrice(rowprice);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::setRowPrice(array<double> ^rowprice)
	{
		pin_ptr<double> rowpricePinned = GetPinablePtr(rowprice);
		setRowPriceUnsafe(rowpricePinned);
	}

	const double *OsiSolverInterface::getRowActivityUnsafe()
	{
		return Base->getRowActivity();
	}
	array<double> ^ OsiSolverInterface::getRowActivity()
	{
		int n = Base->getNumRows();
		if (n == 0) return nullptr;

		double *input = (double *)Base->getRowActivity();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	bool OsiSolverInterface::isAbandoned()
	{
		return Base->isAbandoned();
	}
	bool OsiSolverInterface::isProvenOptimal()
	{
		return Base->isProvenOptimal();
	}
	bool OsiSolverInterface::isProvenPrimalInfeasible()
	{
		return Base->isProvenPrimalInfeasible();
	}
	bool OsiSolverInterface::isProvenDualInfeasible()
	{
		return Base->isProvenDualInfeasible();
	}
	bool OsiSolverInterface::isPrimalObjectiveLimitReached()
	{
		return Base->isPrimalObjectiveLimitReached();
	}
	bool OsiSolverInterface::isDualObjectiveLimitReached()
	{
		return Base->isDualObjectiveLimitReached();
	}
	bool OsiSolverInterface::isIterationLimitReached()
	{
		return Base->isIterationLimitReached();
	}
	int OsiSolverInterface::getIterationCount()
	{
		return Base->getIterationCount();
	}

	void OsiSolverInterface::addCol(int numberElements, array<int> ^rows, array<double> ^elements, 
		double collb, double colub, double obj)
	{
		try
		{
			pin_ptr<int> rowsPinned = GetPinablePtr(rows);
			pin_ptr<double> elementsPinned = GetPinablePtr(elements);

			Base->addCol(numberElements, rowsPinned, elementsPinned, collb, colub, obj);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::restoreBaseModel(int numberRows)
	{
		try
		{
			Base->restoreBaseModel(numberRows);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::saveBaseModel()
	{
		try
		{
			Base->saveBaseModel();
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}

	void OsiSolverInterface::loadProblem(int numcols, int numrows, array<CoinBigIndex> ^start, 
		array<int> ^ index, array<double> ^value, array<double> ^collb, array<double> ^colub, 
		array<double> ^obj, array<double> ^rowlb, array<double> ^rowub)
	{
		try
		{
			pin_ptr<CoinBigIndex> startPinned = GetPinablePtr(start);
			pin_ptr<int> indexPinned = GetPinablePtr(index);
			pin_ptr<double> valuePinned = GetPinablePtr(value);
			pin_ptr<double> collbPinned = GetPinablePtr(collb);
			pin_ptr<double> colubPinned = GetPinablePtr(colub);
			pin_ptr<double> objPinned = GetPinablePtr(obj);
			pin_ptr<double> rowlbPinned = GetPinablePtr(rowlb);
			pin_ptr<double> rowubPinned = GetPinablePtr(rowub);

			Base->loadProblem(numcols, numrows, startPinned, indexPinned, valuePinned, 
				collbPinned, colubPinned, objPinned, rowlbPinned, rowubPinned);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}
	void OsiSolverInterface::loadProblemUnsafe(const int numcols, const int numrows,
		const CoinBigIndex * start, const int* index,
		const double* value,
		const double* collb, const double* colub,   
		const double* obj,
		const double* rowlb, const double* rowub)
	{
		try
		{
			Base->loadProblem(numcols, numrows, start, index, value, 
				collb, colub, obj, rowlb, rowub);
		}
		catch (::CoinError err)
		{
			throw gcnew CoinError(err);
		}
	}	

	OsiSolverInterface^ OsiSolverInterface::CreateDerived(::OsiSolverInterface* derived)
	{
		if (dynamic_cast<::OsiClpSolverInterface*>(derived))
		{
			OsiSolverInterface^ result = gcnew OsiClpSolverInterface();
			result->Base = dynamic_cast<::OsiClpSolverInterface*>(derived);
			result->TransferBase();
			return result;
		}
		else if (dynamic_cast<::OsiCbcSolverInterface*>(derived))
		{
			OsiSolverInterface^ result = gcnew OsiCbcSolverInterface();
			result->Base = dynamic_cast<::OsiCbcSolverInterface*>(derived);
			result->TransferBase();
			return result;
		}

		throw gcnew ArgumentException(L"CreateDerived is not implemented for this type", gcnew String(typeid(derived).name()));
	}
}
