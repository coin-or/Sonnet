// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#pragma once

#include <gcroot.h>
#include <CbcEventHandler.hpp>

//#include "CbcModel.h"
#include "Helpers.h"


using namespace System;
using namespace System::Runtime::InteropServices;

namespace COIN
{
	ref class CbcModel;

	/// <summary>
	/// Events known to cbc
	/// </summary>
	public enum class CbcEvent
	{ 
		/// <summary>Processing of the current node is complete.</summary>
		node = 200,
		/// <summary>A tree status interval has arrived.</summary>
		treeStatus,
		/// <summary>A solution has been found.</summary>
		solution,
		/// <summary>A heuristic solution has been found.</summary>
		heuristicSolution,
		/// <summary>A solution will be found unless user takes action (first check).</summary>
		beforeSolution1,
		/// <summary>A solution will be found unless user takes action (thorough check).</summary>
		beforeSolution2,
		/// <summary>After failed heuristic.</summary>
		afterHeuristic,
		/// <summary>On entry to small branch and bound.</summary>
		smallBranchAndBound,
		/// <summary>After a pass of heuristic.</summary>
		heuristicPass,
		/// <summary>When converting constraints to cuts.</summary>
		convertToCuts,
		/// <summary>Having generated cuts, allows user to think.</summary>
		generatedCuts,
		/// <summary>End of search.</summary>
		endSearch
	};
			
	/// <summary>
	/// Action codes returned by the event handler.
	///  Specific values are chosen to match ClpEventHandler return codes.
	/// </summary>
	public enum class CbcAction
	{
		/// <summary>Continue --- no action required.</summary>
		noAction = -1,
		/// <summary>Stop --- abort the current run at the next opportunity.</summary>
		stop = 0,
		/// <summary>Restart --- restart branch-and-cut search; do not undo root node processing.</summary>
		restart,
		/// <summary>RestartRoot --- undo root node and start branch-and-cut afresh.</summary>
		restartRoot,
		/// <summary>Add special cuts.</summary>
		addCuts,
		/// <summary>Pretend solution never happened.</summary>
		killSolution,
		/// <summary>Take action on modified data.</summary>
		takeAction
	};

	/// <summary>
	/// Delegate for handling CbcEvents.
	/// </summary>
	public delegate COIN::CbcAction CbcEventHandler(COIN::CbcModel^ model, COIN::CbcEvent cbcEvent);

	/// <summary>
	/// Native class derived from CbcEventHandler. This class wrapps the COIN::CbcEventHandler delegate.
	/// Whenever an event occurs, the delegate is invoked, similar to the navite CbcEventHandler.
	/// In SonnetWrapper the eventHandler is merely a method delegate, not an instance of a class derived from the native CbcEventHandler class.
	/// This is a shortcut with limited functionality, but easier to use.
	/// </summary>
	public class CbcDelegateEventHandlerProxy : public ::CbcEventHandler
	{
	public:
		CbcDelegateEventHandlerProxy(gcroot<COIN::CbcEventHandler^> wrapper)
			: ::CbcEventHandler()
		{
			this->wrapper = wrapper;
		}

		/// <summary>The copy constructor</summary>
		CbcDelegateEventHandlerProxy(const CbcDelegateEventHandlerProxy& rhs)
			: ::CbcEventHandler(rhs)
		{
			this->wrapper = rhs.wrapper;
		}

		/// <summary>Assignment operator.</summary>
		CbcDelegateEventHandlerProxy& operator=(const CbcDelegateEventHandlerProxy& rhs)
		{
			if (this != &rhs)
			{
				::CbcEventHandler::operator=(rhs);
				this->wrapper = rhs.wrapper;
			}
			return *this;
		}

		CbcAction event(CbcEvent whichEvent) override;

		/// <summary>
		/// Clone this derived CbcDelegateEventHandler.
		///	The caller (receiver of the clone) is responsible to delete it
		/// </summary>
		/// <returns></returns>
		CbcEventHandler* clone() const override
		{
			return new CbcDelegateEventHandlerProxy(wrapper);
		}

		//add destructor etc.?
		gcroot<COIN::CbcEventHandler^> getDelegate()
		{
			return wrapper;
		}
	private:
		gcroot<COIN::CbcEventHandler^> wrapper;
	};
};

