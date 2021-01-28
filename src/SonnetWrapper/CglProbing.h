// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <CglProbing.hpp>

#include "CglCutGenerator.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	public ref class CglProbing : public CglCutGeneratorGeneric<::CglProbing>
	{
	public:
		CglProbing() {}

		/**@name Whether use objective as constraint */
		//@{
		/** Set
			0 don't
			1 do
			-1 don't even think about it
		*/
		void setUsingObjective(int yesNo) { Base->setUsingObjective(yesNo); }
		/**@name Change maxima */
		//@{
		/// Set maximum number of passes per node
		void setMaxPass(int value)	{ Base->setMaxPass(value); }
		/// Get maximum number of passes per node
		int getMaxPass() { return Base->getMaxPass(); }
		/// Set log level - 0 none, 1 - a bit, 2 - more details
		void setLogLevel(int value) { Base->setLogLevel(value); }
		/// Get log level
		int getLogLevel() { return Base->getLogLevel(); }
		/// Set maximum number of unsatisfied variables to look at
		void setMaxProbe(int value) { Base->setMaxProbe(value); }
		/// Get maximum number of unsatisfied variables to look at
		int getMaxProbe() { return Base->getMaxProbe(); }
		/// Set maximum number of variables to look at in one probe
		void setMaxLook(int value) { Base->setMaxLook(value); }
		/// Get maximum number of variables to look at in one probe
		int getMaxLook() { return Base->getMaxLook(); }
		/// Set maximum number of elements in row for it to be considered
		void setMaxElements(int value) { Base->setMaxElements(value); }
		/// Get maximum number of elements in row for it to be considered
		int getMaxElements() { return Base->getMaxElements(); }
		/// Set maximum number of passes per node  (root node)
		void setMaxPassRoot(int value) { Base->setMaxPassRoot(value); }
		/// Get maximum number of passes per node (root node)
		int getMaxPassRoot() { return Base->getMaxPassRoot(); }
		/// Set maximum number of unsatisfied variables to look at (root node)
		void setMaxProbeRoot(int value) { Base->setMaxProbeRoot(value); }
		/// Get maximum number of unsatisfied variables to look at (root node)
		int getMaxProbeRoot() { return Base->getMaxProbeRoot(); }
		/// Set maximum number of variables to look at in one probe (root node)
		void setMaxLookRoot(int value) { Base->setMaxLookRoot(value); }
		/// Get maximum number of variables to look at in one probe (root node)
		int getMaxLookRoot() { return Base->getMaxLookRoot(); }
		/// Set maximum number of elements in row for it to be considered (root node)
		void setMaxElementsRoot(int value) { Base->setMaxElementsRoot(value); }
		/// Get maximum number of elements in row for it to be considered (root node)
		int getMaxElementsRoot() { return Base->getMaxElementsRoot(); }
	};
}