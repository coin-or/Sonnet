// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include "CoinMessageHandler.h"
#include "CoinWarmStart.h"
#include "OsiSolverParameters.h"

#include <CoinTime.hpp>
#include <CoinMessage.hpp>
#include <CoinMpsIO.hpp>
#include <OsiSolverParameters.hpp>
#include <OsiSolverInterface.hpp>

#include "Helpers.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class OsiSolverInterface : WrapperAbstractBase<::OsiSolverInterface>
	{
	public:
		void branchAndBound();
		void initialSolve();
		void resolve();

		double getInfinity();

		/*! \brief Read a problem in MPS format from the given filename.
    
		  The default implementation uses CoinMpsIO::readMps() to read
		  the MPS file and returns the number of errors encountered.
		*/
	    virtual int readMps (String ^fileName, String ^ extension)
		{
			char * charFileName = (char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer();
			char * charExtension = (char*)Marshal::StringToHGlobalAnsi(extension).ToPointer();

			int result = Base->readMps(charFileName, charExtension);

			Marshal::FreeHGlobal((IntPtr)charFileName);
			Marshal::FreeHGlobal((IntPtr)charExtension);

			return result;
		}
	    virtual int readMps (String ^fileName)
		{
			return readMps(fileName, L"mps");
		}

		void writeMps(String ^fileName);
		void writeMps(String ^fileName, String ^extension);
		void writeMps(String ^fileName, String ^extension, double objSense);
		void writeLp(String ^fileName);
		void writeLp(String ^fileName, String ^extension);
		void writeLp(String ^filename, String ^extension, double epsilon, int numberAcross, int decimals, double objSense, bool useRowNames);

		void addRow(int numberElements, array<int> ^columns, array<double> ^elements, double rowlb, double rowub);

		//Parameter set/get methods
		bool setIntParam(COIN::OsiIntParam key, int value);
		bool setDblParam(COIN::OsiDblParam key, double value);
		bool setStrParam(COIN::OsiStrParam key, String ^ value);
		bool setHintParam(COIN::OsiHintParam key);
		bool setHintParam(COIN::OsiHintParam key, bool yesNo);
		bool setHintParam(COIN::OsiHintParam key, bool yesNo, COIN::OsiHintStrength strength);
		bool setHintParam(COIN::OsiHintParam key, bool yesNo, COIN::OsiHintStrength strength, void * otherInformation);
		bool getIntParam(COIN::OsiIntParam key, [Out] int% value);
		bool getDblParam(COIN::OsiDblParam key, [Out] double% value);
		bool getStrParam(COIN::OsiStrParam key, [Out] String^ % value);
		bool getHintParam(COIN::OsiHintParam key, [Out] bool% yesNo, [Out] COIN::OsiHintStrength% strength, [Out] void *% otherInformation);
		bool getHintParam(OsiHintParam key, [Out] bool% yesNo, [Out] COIN::OsiHintStrength% strength);
		bool getHintParam(OsiHintParam key, [Out] bool% yesNo);

		void setRowName(int index, String ^ name);
		void setRowBounds(int index, double lower, double upper);
		void setRowUpper(int index, double upper);
		void setRowLower(int index, double lower);
		void setRowType(int index, char sense, double rhs, double range);

 		void setColName(int index, String ^ name);
		void setColUpper(int index, double upper);
		void setColLower(int index, double lower);
		void setColBounds(int index, double lower, double upper);

		/// Return true if variable is continuous
		bool isContinuous(int colIndex);

		/** Return true if column is integer.
		Note: This function returns true if the the column
		is binary or a general integer.
		*/
		bool isInteger(int colIndex);

		void setContinuous(int index);
		void setInteger(int index);

		void setObjCoeff(int index, double value);
		void setObjSense(double sense);
		void setObjective(array<double> ^coefs);
#ifdef SONNET_CONSTRAINT_SET_COEF
		void setCoef(int conIndex, int varIndex, double value);
#endif
		virtual CoinWarmStart ^ getEmptyWarmStart();
		virtual CoinWarmStart ^ getWarmStart();
		virtual bool setWarmStart(CoinWarmStart ^ warmstart);

		CoinMessageHandler ^ messageHandler();
		virtual void passInMessageHandler(CoinMessageHandler ^wrapper);

		int getNumCols();
		int getNumRows();
		int getNumElements();
		int getNumIntegers();

		double getObjValue();
	    /// Get objective function sense (1 for min (default), -1 for max)
		double getObjSense();
		const double *getColSolutionUnsafe();
		array<double> ^ getColSolution();
		void setColSolutionUnsafe(const double *colsol);
		void setColSolution(array<double> ^colsol);

		const double *getReducedCostUnsafe();
		array<double> ^getReducedCost();
		const double *getRowPriceUnsafe();
		array<double> ^getRowPrice();
		void setRowPriceUnsafe(const double *rowprice);
		void setRowPrice(array<double>^ rowprice);

		const double *getRowActivityUnsafe();
		array<double> ^getRowActivity();
		bool isAbandoned();
		bool isProvenOptimal();
		bool isProvenPrimalInfeasible();
		bool isProvenDualInfeasible();
		bool isPrimalObjectiveLimitReached();
		bool isDualObjectiveLimitReached();
		bool isIterationLimitReached();
		int getIterationCount();

		void addCol(int numberElements, array<int> ^rows, array<double> ^elements, double collb, double colub, double obj);

		void restoreBaseModel(int numberRows);
		void saveBaseModel();

		void loadProblem(int numcols, int numrows, array<CoinBigIndex> ^start, array<int> ^index, array<double> ^value, array<double> ^collb, array<double> ^colub, array<double> ^obj, array<double> ^rowlb, array<double> ^rowub);
		void loadProblemUnsafe(const int numcols, const int numrows,
			      const CoinBigIndex * start, const int* index,
			      const double* value,
			      const double* collb, const double* colub,   
			      const double* obj,
			      const double* rowlb, const double* rowub);
	};

	template <class T> 
	public ref class OsiSolverInterfaceGeneric : public OsiSolverInterface
	{
	public:
		OsiSolverInterfaceGeneric()
		{
			Base = new T();
		}

	protected:
		OsiSolverInterfaceGeneric(T *derived)
			:OsiSolverInterface(derived)
		{
		}

		property T * Base 
		{
			T * get() 
			{ 
				return dynamic_cast<T*>(OsiSolverInterface::Base); 
			} 
		}
	};
}
