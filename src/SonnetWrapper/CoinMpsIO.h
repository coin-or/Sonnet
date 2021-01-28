// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include "CoinPackedMatrix.h"

#include <CoinMpsIO.hpp>

#include "CoinMessageHandler.h"
#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CoinMpsIO : WrapperBase<::CoinMpsIO>
	{
	public:
		CoinMpsIO() {}

		/// Set infinity
		void setInfinity(double value);

		/// Get infinity
		double getInfinity();

		/// Read a problem in MPS format from the given filename.
		// Use "stdin" or "-" to read from stdin.
		int readMps(String^ filename); // extension = "mps"
		int readMps(String^ filename, String^ extension);

	    /// Get pointer to array[getNumRows()] of row lower bounds
	    array<double>^ getRowLower();

		/// Get pointer to array[getNumRows()] of row upper bounds
		array<double>^ getRowUpper();

		/// Get pointer to array[getNumCols()] of objective function coefficients
		array<double>^ getObjCoefficients();
		/// Return the problem name
		String^ getProblemName();

		/// Return the objective name
		String^ getObjectiveName();

		/// Get number of columns
		int getNumCols();

		/// Get number of rows
		int getNumRows();

		/// Get number of nonzero elements
		int getNumElements();

		/// Get pointer to array[getNumCols()] of column lower bounds
		array<double>^ getColLower();

		/// Get pointer to array[getNumCols()] of column upper bounds
		array<double>^ getColUpper();

		/** Get pointer to array[getNumRows()] of constraint senses.
		<ul>
		<li>'L': <= constraint
		<li>'E': =  constraint
		<li>'G': >= constraint
		<li>'R': ranged constraint
		<li>'N': free constraint
		</ul>
		*/
		array<__wchar_t>^ getRowSense();
		/// Return true if column is a continuous variable
		bool isContinuous(int colNumber);

		/** Return true if a column is an integer variable

		Note: This function returns true if the the column
		is a binary or general integer variable.
		*/
		bool isInteger(int columnNumber);
		/** Returns the row name for the specified index.

		Returns 0 if the index is out of range.
		*/
		String^ rowName(int index);

		/** Returns the column name for the specified index.

		Returns 0 if the index is out of range.
		*/
		String^ columnName(int index);

		CoinPackedMatrix ^ getMatrixByRow()
		{
			return gcnew CoinPackedMatrix(Base->getMatrixByRow());
		}

		/** Pass in Message handler
  
			Supply a custom message handler. It will not be destroyed when the
			CoinMpsIO object is destroyed.
		*/
		void passInMessageHandler(CoinMessageHandler ^ handler)
		{
			Base->passInMessageHandler(handler->Base);
		}
	};
}