// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    public class Named : IComparable<Named>
    {
        public Named()
        {
        }
        public Named(string aName)
        {
            Name = aName;
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        public int ID
        {
            get { return id; }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Named);
        }

        public override int GetHashCode()
        {
            return id;
        }

        bool Equals(Named rhs)
        {
            return id.Equals(rhs.id) && this.GetType().Equals(rhs.GetType());
        }

        public virtual int CompareTo(Named rhs)
        {
            return id.CompareTo(rhs.id);
        }

        internal string name = string.Empty;
        internal int id;
    }

	public class ModelEntity : Named
	{
        private static SonnetLog log = SonnetLog.Default;

        public ModelEntity()
        {
            solver = null;
        }
        public ModelEntity(string name)
            : base(name)
        {
            solver = null;
        }

        internal void Register(Solver solver)
        {
            if (IsRegistered(solver))
            {
                string message = log.ErrorFormat("A solver can be registered at most once, even for {1} '{0}'!", this.Name, this.GetType().Name);
                throw new SonnetException(message);
            }

            solvers.Add(solver);
        }

        internal void Unregister(Solver solver)
        {
            solvers.Remove(solver);
            if (AssignedTo(solver))
            {
                Unassign();
            }
        }
        internal bool IsRegistered()
        {
            return solvers.Count > 0;
        }

        internal bool IsRegistered(Solver solver)
        {
            // first, if this modelEntity is Assigned 
            // ( = has an assigned model which is thus also registered by definition),
            // then simply check if this assigned model is the one requested.
            if (Assigned && object.ReferenceEquals(this.solver, solver)) return true;

            int id = solver.ID;
            return solvers.Find(s => string.Equals(s.ID, id)) != null;
        }

        internal void Assign(Solver solver, int offset)
        {
            // if this modelEntity is already assigned, 
            // then if it is assigned to another model throw an exception
            //      or if it has a different offset within the requested model, throw an exception
            if (Assigned && (AssignedTo(solver) == false || offset != this.offset))
            {
                string message = log.ErrorFormat("Trying to assign to new solver while already assigned to solver ", solver.Name);
                throw new SonnetException(message);
            }

            this.solver = solver;
            this.offset = offset;
        }

        private void Unassign()
        {
            solver = null;
            offset = -1;
        }

        internal List<Solver> Solvers
        {
            get { return this.solvers; }
        }

        internal int Offset
        {
            get { return offset; }
        }

        public bool AssignedTo(Solver solver)
        {
            return this.solver == solver;
        }

        internal Solver AssignedSolver
        {
            get { return solver; }
        }

        internal bool Assigned
        {
            get { return solver != null; }
        }

		protected List<Solver> solvers = new List<Solver>();		// should be set in derived class'  Load(Model ^model,....)
        protected Solver solver = null;
		protected int offset = -1;
	}
}
