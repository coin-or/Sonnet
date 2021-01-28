// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#include "CoinMpsIO.h"
#include "CoinPackedMatrix.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	/// Set infinity
	void CoinMpsIO::setInfinity(double value)
	{
		Base->setInfinity(value);
	}

	/// Get infinity
	double CoinMpsIO::getInfinity()
	{
		return Base->getInfinity();
	}
	/** Read a problem in MPS format from the given filename.

	Use "stdin" or "-" to read from stdin.
	*/
	int CoinMpsIO::readMps(String^ fileName)
	{
		return readMps(fileName, "mps");
	}

	int CoinMpsIO::readMps(String^ fileName, String ^ extension)
	{
		char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
		char * charExtension = (char*)Marshal::StringToHGlobalAnsi(extension).ToPointer();
		int result = Base->readMps(charFileName,charExtension);
		Marshal::FreeHGlobal((IntPtr)charFileName);
		Marshal::FreeHGlobal((IntPtr)charExtension);
		return result;
	}

	/// Get pointer to array[getNumRows()] of row lower bounds
	array<double>^ CoinMpsIO::getRowLower()
	{
		int n = Base->getNumRows();
		double *input = (double *) Base->getRowLower();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	/// Get pointer to array[getNumRows()] of row upper bounds
	array<double>^ CoinMpsIO::getRowUpper()
	{
		int n = Base->getNumRows();
		double *input = (double *) Base->getRowUpper();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	/// Get pointer to array[getNumCols()] of objective function coefficients
	array<double>^ CoinMpsIO::getObjCoefficients()
	{
		int n = Base->getNumCols();
		double *input = (double *) Base->getObjCoefficients();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	/// Return the problem name
	String^ CoinMpsIO::getProblemName()
	{
		String ^ result = gcnew String(Base->getProblemName());
		return result;
	}

	/// Return the objective name
	String ^ CoinMpsIO::getObjectiveName()
	{
		String ^ result = gcnew String(Base->getObjectiveName());
		return result;
	}

	/// Get number of columns
	int CoinMpsIO::getNumCols()
	{
		return Base->getNumCols();
	}

	/// Get number of rows
	int CoinMpsIO::getNumRows()
	{
		return Base->getNumRows();
	}

	/// Get number of nonzero elements
	int CoinMpsIO::getNumElements()
	{
		return Base->getNumElements();
	}

	/// Get pointer to array[getNumCols()] of column lower bounds
	array<double>^ CoinMpsIO::getColLower()
	{
		int n = Base->getNumCols();
		double *input = (double *) Base->getColLower();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	/// Get pointer to array[getNumCols()] of column upper bounds
	array<double>^ CoinMpsIO::getColUpper()		
	{
		int n = Base->getNumCols();
		double *input = (double *) Base->getColUpper();
		array<double> ^result = gcnew array<double>(n);
		System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
		return result;
	}

	/** Get pointer to array[getNumRows()] of constraint senses.
	<ul>
	<li>'L': <= constraint
	<li>'E': =  constraint
	<li>'G': >= constraint
	<li>'R': ranged constraint
	<li>'N': free constraint
	</ul>
	*/
	array<__wchar_t>^ CoinMpsIO::getRowSense()
	{
		int n = Base->getNumRows();
		if (n == 0) return gcnew array<__wchar_t>(n);
		String ^ result = gcnew String(Base->getRowSense(),0,n);
		return result->ToCharArray();
	}

	/// Return true if column is a continuous variable
	bool CoinMpsIO::isContinuous(int colNumber)
	{
		return Base->isContinuous(colNumber);
	}

	/** Return true if a column is an integer variable

	Note: This function returns true if the the column
	is a binary or general integer variable.
	*/
	bool CoinMpsIO::isInteger(int columnNumber)
	{
		return Base->isInteger(columnNumber);
	}

	/** Returns the row name for the specified index.
	Returns 0 if the index is out of range.
	*/
	String ^ CoinMpsIO::rowName(int index)
	{
		String ^ result = gcnew String(Base->rowName(index));
		return result;
	}

	/** Returns the column name for the specified index.
	Returns 0 if the index is out of range.
	*/
	String ^ CoinMpsIO::columnName(int index)
	{
		String ^ result = gcnew String(Base->columnName(index));
		return result;
	}
};