// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#ifndef ModelEntity_H
#define ModelEntity_H

#include <stdio.h>
#include <string> 
#include <list>
#include <algorithm>
#include <functional>

#include "Solver.h"
#include "Utils.h"
#include "Named.h"

using namespace std;

namespace Sonnet
{
	/// <summary>
	/// The class ModelEntity is a base class for entities that are registered with solvers.
	/// </summary>
	class ModelEntity : public Named
	{
		friend class Solver;
		//private static SonnetLog log = SonnetLog.Default; // TODO

	public:
		/// <summary>
		/// Constructs a new ModelEntity with the given name.
		/// </summary>
		/// <param name="name">The name of this entity.</param>
		ModelEntity(const string& name = "")
			: Named(name)
		{
			solver = NULL;
			offset = -1;
		}

	private:
		/// <summary>
		/// Register the given solver with this entity.
		/// Whenever changes are made to this entity, these will be passed on to registerd solvers,
		/// such that the COIN solver can be updated.
		/// Therefore, only Generated entities (passed to the COIN solver) need to be registered.
		/// </summary>
		/// <param name="solver">The solver of which this constraint is part.</param>
		void Register(Solver *solver)
		{
			string name = "hello world";
			Named foo(name);

			if (IsRegistered(solver))
			{
				string message = string_format("A solver can be registered at most once, even for %s '%s'!", getTypeName(), getName()) ;
				//TODO throw SonnetException(message);
			}

			solvers.push_back(solver);
		}

		/// <summary>
		/// Remove the given solver from the list of registered solvers.
		/// If the solver was not registered, an exception will be thrown.
		/// </summary>
		/// <param name="solver">The solver</param>
		void Unregister(Solver *solver)
		{
			auto it = find(solvers.begin(), solvers.end(), solver);

			if (it == solvers.end())
			{
				string message = string_format("Solver %s cannot be unregistered because it is not registered with %s '%s'!", solver->getName(), getTypeName(), getName());
				//TODO throw new SonnetException(message);
				return;
			}

			solvers.erase(it);

			if (AssignedTo(solver))
			{
				Unassign();
			}
		}

		/// <summary>
		/// Returns true iff the given solver is registered with this entity.
		/// </summary>
		/// <param name="solver">The solver</param>
		/// <returns>True iff the given solver is registered with this entity.</returns>
		bool IsRegistered(Solver *solver)
		{
			// first, if this modelEntity is Assigned 
			// ( = has an assigned model which is thus also registered by definition),
			// then simply check if this assigned model is the one requested.
			if (getAssigned() && this->solver == solver) return true;

			//int id = solver->getID();
			//return find_if(solvers.begin(), solvers.end(), [id] (Solver *s) {return s->getID() == id;}) != solvers.end();

			return find(solvers.begin(), solvers.end(), solver) != solvers.end();
			//return solvers.Find(s => string.Equals(s.ID, id)) != null;
		}

		/// <summary>
		/// Set the given solver to be the assigned solver, where this entity has the given offset.
		/// </summary>
		/// <param name="solver">The newly assigned solver.</param>
		/// <param name="offset">The offset of the current entity in the solver.</param>
		void Assign(Solver *solver, int offset)
		{
			// if this modelEntity is already assigned, 
			// then if it is assigned to another model throw an exception
			//      or if it has a different offset within the requested model, throw an exception
			if (getAssigned() && (AssignedTo(solver) == false || offset != this->offset))
			{
				//log.ErrorFormat(
				string message = string_format("Trying to assign to new solver while already assigned to solver %s", solver->getName());
				//TODO throw new SonnetException(message);
			}

			this->solver = solver;
			this->offset = offset;
		}

		void Unassign()
		{
			solver = NULL;
			offset = -1;
		}

		/// <summary>
		/// Gets the offset of this entity in the assigned solver.
		/// </summary>
		int getOffset()
		{
			return offset; 
		}

		/// <summary>
		/// Determines whether the given solver is the current Assigned solver.
		/// </summary>
		/// <param name="solver">The solver to compare.</param>
		/// <returns>True iff the given solver is the same as the current solver.</returns>
		bool AssignedTo(Solver *solver)
		{
			return (this->solver) == solver;
		}

		/// <summary>
		/// Gets the currently assigned solver.
		/// </summary>
		Solver* getAssignedSolver()
		{
			return solver;
		}

		/// <summary>
		/// Determines whether this entity is assigned to a solver.
		/// </summary>
		bool getAssigned()
		{
			return solver != NULL; 
		}

	private:
		/// <summary>
		/// The list of solvers with which this entity is registered.
		/// Derived classes use this to notify solvers of any changes to them (name, bounds, etc)
		/// </summary>
		list<Solver *> solvers;
		Solver *solver;
		int offset;
	};
}

#endif
