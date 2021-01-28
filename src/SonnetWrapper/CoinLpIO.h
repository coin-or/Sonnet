// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include "CoinError.h"
#include "CoinMessageHandler.h"
#include "CoinPackedMatrix.h"
#include "Helpers.h"

#include <CoinLpIO.hpp>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CoinLpIO : WrapperBase<::CoinLpIO>
	{
	public:
		CoinLpIO() {}

		/// Set infinity
		void setInfinity(double value)
		{
			Base->setInfinity(value);
		}

		/// Get infinity
		double getInfinity()
		{
			return Base->getInfinity();
		}

		void readLp(String^ fileName)
		{
			try
			{
				char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
				Base->readLp(charFileName);
				Marshal::FreeHGlobal((IntPtr)charFileName);
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}

		void readLp(String^ fileName, double epsilon)
		{
			try
			{
				char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
				Base->readLp(charFileName, epsilon);
				Marshal::FreeHGlobal((IntPtr)charFileName);
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}

		/// Get pointer to array[getNumRows()] of row lower bounds
		array<double>^ getRowLower()
		{
			int n = Base->getNumRows();
			double *input = (double *) Base->getRowLower();
			array<double> ^result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		/// Get pointer to array[getNumRows()] of row upper bounds
		array<double>^ getRowUpper()
		{
			int n = Base->getNumRows();
			double *input = (double *) Base->getRowUpper();
			array<double> ^result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		/// Get pointer to array[getNumCols()] of objective function coefficients
		array<double>^ getObjCoefficients()
		{
			int n = Base->getNumCols();
			double *input = (double *) Base->getObjCoefficients();
			array<double> ^result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		/// Return the problem name
		String^ getProblemName()
		{
			String ^ result = gcnew String(Base->getProblemName());
			return result;
		}

		/// Return the objective name
		String^ getObjName()
		{
			String ^ result = gcnew String(Base->getObjName());
			return result;
		}

		/// Get number of columns
		int getNumCols()
		{
			return Base->getNumCols();
		}

		/// Get number of rows
		int getNumRows()
		{
			return Base->getNumRows();
		}

		/// Get number of nonzero elements
		int getNumElements()
		{
			return Base->getNumElements();
		}

		/// Get pointer to array[getNumCols()] of column lower bounds
		array<double>^ getColLower()
		{
			int n = Base->getNumCols();
			double *input = (double *) Base->getColLower();
			array<double> ^result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		/// Get pointer to array[getNumCols()] of column upper bounds
		array<double>^ getColUpper()
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
		array<__wchar_t>^ getRowSense()
		{
			int n = Base->getNumRows();
			if (n == 0) return gcnew array<__wchar_t>(n);
			String ^ result = gcnew String(Base->getRowSense(),0,n);
			return result->ToCharArray();
		}

		/// Return true if a column is an integer (binary or general 
		/// integer) variable
		bool isInteger(int columnNumber)
		{
			return Base->isInteger(columnNumber);
		}

		/// Return the row name for the specified index.
		/// Return the objective function name if index = getNumRows().
		/// Return 0 if the index is out of range or if row names are not defined.
		String^ rowName(int index)
		{
			String ^ result = gcnew String(Base->rowName(index));
			return result;
		}

		/// Return the column name for the specified index.
		/// Return 0 if the index is out of range or if column names are not 
		/// defined.
		String^ columnName(int index)
		{
			String ^ result = gcnew String(Base->columnName(index));
			return result;
		}

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