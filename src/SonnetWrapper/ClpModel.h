// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include "ClpObjective.h"
#include "CoinMessageHandler.h"
#include "CoinError.h"
#include "Helpers.h"

#include <ClpModel.hpp>
#include <ClpSimplex.hpp>


using namespace System;
using namespace System::Runtime::InteropServices; // Marshall

namespace COIN
{
	public ref class ClpModel : WrapperBase<::ClpModel>
	{
	internal:
		ClpModel(::ClpModel* obj)
			: WrapperBase(obj)
		{
		}

	public:
		ClpModel() { }

		void modifyCoefficient(int row, int column, double newElement)
		{
			Base->modifyCoefficient(row, column, newElement);
		}

		void modifyCoefficient(int row, int column, double newElement, bool keepZero)
		{
			Base->modifyCoefficient(row, column, newElement, keepZero);
		}

		///<summary>Load up quadratic objective. This is stored as a CoinPackedMatrix.
		/// Adds quadratic component to any existing linear objective.
		/// It's assumed the matrix is symmetric, so only provide top part.
		/// Also, for diagonal elements pass double value, and for non-diagonals normal value.
		/// start[0 .. n] gives for every column (variable) the starting point in the column and value arrays. start[n] = #nz
		/// column[0 .. nz-1] gives for each nonzero element the second involved column--the first involved column gives start.
		/// value[0 .. nz-1] gives the nonzero (multiplier) value.
		/// EXAMPLE: Quadratic objective 0.5 x1 ^ 2 + x2 ^ 2 - x1x2 becomes
		/// start = { 0, 2, 3}, column = {0 (x1^2), 1 (x1x2), 1 (x2^2) }, 
		///	element = { 1.0 (x1^2, diagonal so double value), -1.0 (x1x2, non-diagonal so normal value), 2.0 (x2^2, diagonal so double value)}
		///</summary>
		void loadQuadraticObjective(int numberColumns, array<CoinBigIndex>^ start,
			array<int>^ column, array<double>^ element)
		{
			pin_ptr<CoinBigIndex> startPinned = GetPinablePtr(start);
			pin_ptr<int> columnPinned = GetPinablePtr(column);
			pin_ptr<double> elementPinned = GetPinablePtr(element);

			loadQuadraticObjectiveUnsafe(numberColumns, startPinned, columnPinned, elementPinned);
		}

		void loadQuadraticObjectiveUnsafe(int numberColumns, const CoinBigIndex* start,
			const int* column, const double* element)
		{
			try
			{
				// If (for what reason?) the full matrix is provided (so x1x2 and x2x1), then non-diagonals elements must be half of the value
				// start = { 0, 2, 4}, column = {0 (x1^2), 1 (x1x2), 0 (x2x1), 1 (x2^2)}, 
				//	element = { 1.0 (x1^2, diagonal so double value), -0.5 (x1x2, non-diagonal so half value), -0.5 (x2x1, non-diagonal so half value), 2.0 (x2^2, diagonal so double value)}
				// but why do this?!
				Base->loadQuadraticObjective(numberColumns, start, column, element);
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}


		//void loadQuadraticObjective(CoinPackedMatrix^ matrix);
		/// Get rid of quadratic objective
		void deleteQuadraticObjective()
		{
			Base->deleteQuadraticObjective();
		}

		ClpObjective^ objectiveAsObject()
		{
			return gcnew ClpObjective(Base->objectiveAsObject());
		}
		
		//void setObjective(ClpObjective^ objective);

		const array<double>^ getObjCoefficients()
		{
			int n = Base->getNumCols();
			if (n == 0) return nullptr;

			double* input = (double*)Base->getObjCoefficients();
			array<double>^ result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		bool isInteger(int columnNumber)
		{
			return Base->isInteger(columnNumber);
		}

		/// Row names
		inline String^ rowName(int index)
		{
			String^ result = gcnew String(Base->rowName(index));
			return result;
		}

		/// Column names
		inline String^ columnName(int index)
		{
			String^ result = gcnew String(Base->columnName(index));
			return result;
		}

		/// Direction of optimization (1 - minimize, -1 - maximize, 0 - ignore
		inline double getObjSense()
		{
			return Base->getObjSense();
		}

		const array<double>^ getRowLower()
		{
			int n = Base->getNumRows();
			if (n == 0) return nullptr;

			double* input = (double*)Base->getRowLower();
			array<double>^ result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		const array<double>^ getRowUpper()
		{
			int n = Base->getNumRows();
			if (n == 0) return nullptr;

			double* input = (double*)Base->getRowUpper();
			array<double>^ result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		const array<double>^ getColLower()
		{
			int n = Base->getNumCols();
			if (n == 0) return nullptr;

			double* input = (double*)Base->getColLower();
			array<double>^ result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}
		
		const array<double>^ getColUpper()
		{
			int n = Base->getNumCols();
			if (n == 0) return nullptr;

			double* input = (double*)Base->getColUpper();
			array<double>^ result = gcnew array<double>(n);
			System::Runtime::InteropServices::Marshal::Copy((System::IntPtr)input, result, 0, n);
			return result;
		}

		/// Matrix (if not ClpPackedmatrix be careful about memory leak
		//TODO
		inline CoinPackedMatrix* matrix() const
		{
			if (matrix_ == NULL)
				return NULL;
			else
				return matrix_->getPackedMatrix();
		}

		/// <summary>
		/// Pass in Message handler (not deleted at end)
		/// </summary>
		/// <param name="handler"></param>
		void passInMessageHandler(CoinMessageHandler^ handler)
		{
			Base->passInMessageHandler(handler->Base);
		}

		/// <summary>
		/// Read an mps file from the given filename. Do not use row and column names. Do not ignore errors.
		/// </summary>
		/// <param name="fileName">File to be read</param>
		/// <returns>Status non-zero represents an error</returns>
		int readMps(String^ fileName)
		{
			return readMps(fileName, false, false);
		}

		/// <summary>
		/// Read an mps file from the given filename. Do not ignore errors.
		/// </summary>
		/// <param name="fileName">File to be read</param>
		/// <param name="keepNames">True to use the given row and column names</param>
		/// <returns>Status non-zero represents an error</returns>
		int readMps(String^ fileName, bool keepNames)
		{
			return readMps(fileName, keepNames, false);
		}
		
		
		/// <summary>
		/// Read an mps file from the given filename.
		/// </summary>
		/// <param name="fileName">File to be read</param>
		/// <param name="keepNames">True to use the given row and column names</param>
		/// <param name="ignoreErrors">True to ignore errors during reading of the file</param>
		/// <returns>Status non-zero represents an error</returns>
		int readMps(String^ fileName, bool keepNames, bool ignoreErrors)
		{
			try
			{
				char* charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
				int result = Base->readMps(charFileName, keepNames, ignoreErrors);
				Marshal::FreeHGlobal((IntPtr)charFileName);
				return result;
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}
	 /** Write the problem in MPS format to the specified file.

	 Row and column names may be null.
	 formatType is
	 <ul>
	   <li> 0 - normal
	   <li> 1 - extra accuracy
	   <li> 2 - IEEE hex
	 </ul>

	 Returns non-zero on I/O error
	 */
		
		int writeMps(String^ fileName)
		{
			return writeMps(fileName, 0);
		}
		int writeMps(String^ fileName, int formatType)
		{
			return writeMps(fileName, formatType, 2);
		}
		int writeMps(String^ fileName, int formatType, int numberAcross)
		{
			return writeMps(fileName, formatType, numberAcross, 0.0);
		}
		int writeMps(String^ fileName, int formatType, int numberAcross, double objSense)
		{
			try
			{
				char* charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
				int result = Base->writeMps(charFileName, formatType, numberAcross, objSense);
				Marshal::FreeHGlobal((IntPtr)charFileName);
				return result;
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}

		void loadProblemUnsafe(const int numcols, const int numrows,
			const CoinBigIndex* start, const int* index,
			const double* value,
			const double* collb, const double* colub,
			const double* obj,
			const double* rowlb, const double* rowub)
		{
			Base->loadProblem(numcols, numrows, start, index, value, collb, colub, obj, rowlb, rowub);
		}

	};

	// This class is merely an intermediate class for all derived classes
	template <class T>
	public ref class ClpModelGeneric abstract : public ClpModel
	{
	public:
		ClpModelGeneric()
		{
			try
			{
				// Create the derived object (ClpSimplexe, etc) that will be Base.
				Base = new T();
			}
			catch (::CoinError err)
			{
				throw gcnew CoinError(err);
			}
		}

	protected:
		ClpModelGeneric(T* base)
			:ClpModel(base)
		{
		}

	internal:
		property T* Base
		{
			T* get()
			{
				return static_cast<T*>(ClpModel::Base);
			}
		}
	};

	public ref class ClpSimplex : ClpModelGeneric<::ClpModel>
	{
	public:
		ClpSimplex()
		{
			// base class ClpModelGeneric takes care of constructing ::base.
		}

		ClpSimplex(::ClpSimplex* base)
			:ClpModelGeneric(base)
		{
		}

		void loadProblem(int numcols, int numrows, array<CoinBigIndex>^ start,
			array<int>^ index, array<double>^ value, array<double>^ collb, array<double>^ colub,
			array<double>^ obj, array<double>^ rowlb, array<double>^ rowub)
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
	};
}