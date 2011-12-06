// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using COIN;

namespace Sonnet
{
    /// <summary>
    /// Specifies the types of solvers.
    /// At runtime the actual Osi solver instance is attempted to be created using reflection.
    /// The solvers must be available through the SonnetWrapper.
    /// </summary>
    public enum SolverType
    {
        /// <summary>
        /// No solver type set
        /// </summary>
        Undefined,
        /// <summary>
        /// Represents the COIN LP solver Clp via OsiClpSolverInterface
        /// </summary>
        ClpSolver,
        /// <summary>
        /// Represents the CON Branch-and-Cut solver Cbc via OsiCbcSolverInterface
        /// </summary>
        CbcSolver,
        /// <summary>
        /// Represents the COIN Volume algorithm via OsiVolSolverInterface (availability is checked at runtime)
        /// </summary>
        VolSolver,
        /// <summary>
        /// Represents the CPLEX solver	via OsiCpxSolverInterface (availability is checked at runtime )
        /// </summary>
        CpxSolver
    }

    public class Solver : Named, IDisposable
    {
        private static SonnetLog log = SonnetLog.Default;

        #region Constructors
        public Solver(Model model, SolverType solverType, string name = "")
            : this(model, CreateSolver(solverType), name)
        {
        }

        public Solver(Model model, OsiSolverInterface solver, string name = "")
        {
            Ensure.NotNull(model, "model");
            Ensure.NotNull(solver, "solver");

            GutsOfConstructor(model, solver);
        }
        #endregion

        public static bool CanCreateSolver(SolverType value)
        {
            return CreateSolver(value) != null;
        }

        public static OsiSolverInterface CreateSolver(SolverType value)
        {
            OsiSolverInterface result = null;
            switch (value)
            {
                case SolverType.ClpSolver:
                    result = new OsiClpSolverInterface();
                    break;
                case SolverType.CbcSolver:
                    result = new OsiCbcSolverInterface();
                    break;
                case SolverType.VolSolver:
                    result = (OsiSolverInterface)System.Reflection.Assembly.GetAssembly(typeof(COIN.OsiSolverInterface)).CreateInstance("COIN.OsiVolSolverInterface");
                    break;
                case SolverType.CpxSolver:
                    result = (OsiSolverInterface)System.Reflection.Assembly.GetAssembly(typeof(COIN.OsiSolverInterface)).CreateInstance("COIN.OsiCpxSolverInterface");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get or set the amount of logging for the current solver
        /// </summary>
        public int LogLevel
        {
            get { return SonnetLog.Default.LogLevel; }
            set { SonnetLog.Default.LogLevel = value; }
        }
        
        /// <summary>
        /// When true, automatically reset the solver after a MIP has been solved (= Default)
        /// When false, you have to manually Save before and Reset after a MIP solve.
        /// Note, this only affects MIP.
        /// </summary>
        public bool AutoResetMIPSolve
        {
            get { return this.autoResetMIPSolve; }
            set { this.autoResetMIPSolve = value; }
        }
        
        #region OsiSolver Properties and Parameters
        /// <summary>
        /// The name discipline; specifies how the solver will handle row and
        ///  column names.
        /// - 0: Auto names: Names cannot be set by the client. Names of the form
        /// Rnnnnnnn or Cnnnnnnn are generated on demand when a name for a
        /// specific row or column is requested; nnnnnnn is derived from the row
        /// or column index. Requests for a vector of names return a vector with
        /// zero entries.
        /// - 1: Lazy names: Names supplied by the client are retained. Names of the
        /// form Rnnnnnnn or Cnnnnnnn are generated on demand if no name has been
        /// supplied by the client. Requests for a vector of names return a
        /// vector sized to the largest index of a name supplied by the client;
        /// some entries in the vector may be null strings.
        /// - 2: Full names: Names supplied by the client are retained. Names of the
        /// form Rnnnnnnn or Cnnnnnnn are generated on demand if no name has been
        /// supplied by the client. Requests for a vector of names return a
        /// vector sized to match the constraint system, and all entries will
        /// contain either the name specified by the client or a generated name.
        /// </summary>
        public int NameDiscipline
        {
            get
            {
                int value;
                solver.getIntParam(OsiIntParam.OsiNameDiscipline, out value);
                return value;
            }
            set { solver.setIntParam(OsiIntParam.OsiNameDiscipline, value); }
        }

        /// <summary>
        /// Get the Infinity of the current solver
        /// </summary>
        public double Infinity { get { return solver.getInfinity(); } }

        /// <summary>
        /// Gets the number of generated constraints.
        /// </summary>
        public int NumberOfConstraints { get { return solver.getNumRows(); } }
        /// <summary>
        /// Gets the number of generated variables.
        /// </summary>
        public int NumberOfVariables { get { return solver.getNumCols(); } }
        /// <summary>
        /// Gets the number of elements in the constraint matrix
        /// </summary>
        public int NumberOfElements { get { return solver.getNumElements(); } }
        /// <summary>
        /// Gets the number of integer variables
        /// </summary>
        public int NumberOfIntegerVariables { get { return solver.getNumIntegers(); } }

        /// <summary>
        /// Returns true iff there is at least one integer variable
        /// </summary>
        public bool IsMIP { get { return NumberOfIntegerVariables > 0; } }

        #endregion

        #region Static Properties
        /// <summary>
        /// Gets the Version number of this assembly
        /// </summary>
        public static Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
        /// <summary>
        /// Gets the default solver type (CbcSolver). Any new Model without an explicit solver type will use this solver.
        /// </summary>
        public static SolverType DefaultSolverType { get { return SolverType.ClpSolver; } }
        #endregion

        public Model Model
        {
            get { return this.model; }
        }

        public OsiSolverInterface OsiSolver
        {
            get { return this.solver; }
        }

        #region Get Model propeties
        /// <summary>
        /// Get whether this model was already generated (at least) once before.
        /// Note, if alterations have been made after the last solve (Generate), then still this will return true.
        /// </summary>
        protected bool Generated
        {
            get { return generated; }
        }

        #endregion

        internal void ApplyObjective(Objective objective)
        {
            Ensure.NotNull(objective, "objective");
            Ensure.NotNull(this.objective, "this.objective");
            // this method should ONLY be called FROM the model

            if (Generated)
            {
                this.objective.Unregister(this);
                this.objective = objective;
                Generate(this.objective);
            }
            else
            {
                this.objective = objective;
            }
        }

        internal void ApplyObjectiveSense(ObjectiveSense sense)
        {


            // (1 for min (default), -1 for max)
            switch (sense)
            {
                case ObjectiveSense.Maximise:
                    solver.setObjSense(-1.0);
                    break;
                case ObjectiveSense.Minimise:
                    solver.setObjSense(1.0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown objective function sense : " + sense);
            }
        }

        /// <summary>
        /// Adds (a reference) the given constraint to the mode with the given name
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        internal void Add(Constraint con)// overrides any given name. Adds a reference!
        {
            Ensure.NotNull(con, "con");

            rawconstraints.Add(con);
        }



        #region Solve, Resolve, Maximise and Minimise Methods
        ///<summary>
        /// Maximises the model
        /// This implicitly sets the objective sense to Maximise
        /// Uses branch and bound if model is MIP and LP otherwise
        /// </summary>
        /// <param name="forceRelaxation">Force solving LP relaxation</param>
        public void Maximise(bool forceRelaxation = false)
        {
            ApplyObjectiveSense(ObjectiveSense.Maximise);
            if (!Generated) Solve(forceRelaxation);
            else Resolve(forceRelaxation);
        }

        ///<summary>
        /// Minimises the model
        /// This implicitly sets the objective sense to Minimise
        /// Uses branch and bound if model is MIP and LP otherwise
        /// </summary>
        /// <param name="forceRelaxation">Force solving LP relaxation</param>
        public void Minimise(bool forceRelaxation = false)
        {
            ApplyObjectiveSense(ObjectiveSense.Minimise);
            if (!Generated) Solve(forceRelaxation);
            else Resolve(forceRelaxation);
        }

        /// <summary>
        /// Solve the model according to the ObjectiveSense settings
        /// Uses branch and bound if model is MIP and LP otherwise
        /// </summary>
        /// <param name="forceRelaxation">Force solving LP relaxation</param>
        public void Solve(bool forceRelaxation = false)
        {
            Solve(false, forceRelaxation);
        }

        /// <summary>
        /// Resolve the model according to the ObjectiveSense settings
        /// Uses branch and bound if model is MIP and LP otherwise
        /// </summary>
        /// <param name="forceRelaxation">Force solving LP relaxation</param>
        public void Resolve(bool forceRelaxation = false)
        {
            Solve(true, forceRelaxation);
        }

        private void Solve(bool doResolve, bool forceRelaxation)
        {
            if (doResolve)
            {
                if (!Generated) throw new SonnetException("Cannot Resolve a model that hasn't been generated yet.");
            }

            double genStart = CoinUtils.CoinCpuTime();

            // Note: always call Generate!
            Generate();

            try
            {
                if (forceRelaxation == false && IsMIP)
                {
                    isSolving = true;
                    if (AutoResetMIPSolve) SaveBeforeMIPSolveInternal();

                    solver.branchAndBound();

                    AssignSolution();
                    if (AutoResetMIPSolve) ResetAfterMIPSolveInternal(); // mainly to reset bounds etc, but use AssignSolutionStatus because the Reset messes up the IsProvenOptimal etc!
                }
                else
                {
                    isSolving = true;
                    if (doResolve) solver.resolve();
                    else solver.initialSolve();
                    AssignSolution();
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw;
            }
            finally
            {
                isSolving = false;
            }

            log.InfoFormat(" IsAbandoned: {0}", IsAbandoned);
            log.InfoFormat(" IsProvenOptimal: {0}", IsProvenOptimal);
            log.InfoFormat(" IsProvenDualInfeasible: {0}", IsProvenDualInfeasible);
            log.InfoFormat(" IsProvenPrimalInfeasible: {0}", IsProvenPrimalInfeasible);
            log.InfoFormat(" IsPrimalObjectiveLimitReached: {0}", IsPrimalObjectiveLimitReached);
            log.InfoFormat(" IsDualObjectiveLimitReached: {0}", IsDualObjectiveLimitReached);

            log.InfoFormat("Done resolving after {0}", (CoinUtils.CoinCpuTime() - genStart));
        }

        #endregion

        public bool IsFeasible()
        {
            bool feasible = true;
            StringBuilder tmp = new StringBuilder();

            tmp.Append(string.Concat("Claimed Solution status: ", ((IsProvenOptimal) ? ("Optimal") : "not Optimal")));
            tmp.Append("\n");

            StringBuilder infeasibleVariables = new StringBuilder();
            int m = variables.Count;
            for (int j = 0; j < m; j++)
            {
                Variable aVar = variables[j];
                if (!aVar.IsFeasible())
                {
                    infeasibleVariables.Append(aVar.ToLevelString());
                    feasible = false;
                }
            }

            if (infeasibleVariables.Length > 0)
            {
                tmp.Append("Infeasible variables:");
                tmp.Append(infeasibleVariables.ToString());
            }

            StringBuilder infeasibleConstraints = new StringBuilder();
            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                Constraint aCon = constraints[i];
                if (!aCon.IsFeasible())
                {
                    infeasibleConstraints.Append(aCon.ToLevelString());
                    feasible = false;
                }
            }

            if (infeasibleConstraints.Length > 0)
            {
                tmp.Append("Infeasible constraints:");
                tmp.Append(infeasibleConstraints.ToString());
            }

            tmp.Append("End");
            Debug.WriteLine(tmp.ToString());

            return feasible;
        }

        #region Get / Set WarmStart methods
        ///<summary>
        /// Gets the warm start object for the current problem
        ///</summary>
        public WarmStart GetWarmStart()
        {
            return WarmStart.NewWarmStart(solver);
        }

        ///<summary>
        /// Gets an empty warm start object for the current problem
        ///</summary>
        public WarmStart GetEmptyWarmStart()
        {
            return WarmStart.NewEmptyWarmStart(solver);
        }

        ///<summary>
        /// Set (apply) the given warm start object to the current problem
        ///</summary>
        public void SetWarmStart(WarmStart warmStart)
        {
            Ensure.NotNull(warmStart, "WarmStart");

            warmStart.ApplyWarmStart(solver);
        }
        #endregion;

        #region Reset / Save for MIP Solver methods
        public void ResetAfterMIPSolve()
        {
            if (AutoResetMIPSolve)
            {
                throw new NotSupportedException("ResetAfterMIPSolve is automatically called");
            }

            SaveBeforeMIPSolveInternal();
        }
        private void ResetAfterMIPSolveInternal()
        {
            //TODO: this needs some work: ResetAfterMIPSolve also clears the solution, so afterwards any AssignSolution yields all zero values!
            // so, either we always AssignSolution, and discourage the user from calling it (in MIP),
            // or we (force) manual ResetAfterMIPSolve from the user in MIP.
            // Decision: *automatic* assign solution and but dont make this private: we need AssignSolution
            // for the use-case with multiple models using the same var/con : after solving in both models, 
            // the user needs to be able to switch the solutions in the variables by calling AssignSolution on 
            // the prefered model.
            // But the problem with ResetAfterMIPSolve is bigger: also the solver.get.. functions (getObjValue etc) no longer work
            if (solver is OsiCbcSolverInterface)
            {
                // Since the model was already generated before, we can safely Reset to the reference solver instance 
                // see also at the end of Generate()
                OsiCbcSolverInterface osiCbcModel = ((OsiCbcSolverInterface)solver);
                osiCbcModel.resetModelToReferenceSolver();
            }
            else
            {
                solver.restoreBaseModel(solver.getNumRows());
                // Restore bounds and dual limit
                solver.setColLower(saveColLower);
                solver.setColUpper(saveColUpper);
                solver.setDblParam(OsiDblParam.OsiDualObjectiveLimit, saveOsiDualObjectiveLimit);
            }
        }

        public void SaveBeforeMIPSolve()
        {
            if (AutoResetMIPSolve)
            {
                throw new NotSupportedException("SaveBeforeMIPSolve is automatically called");
            }

            SaveBeforeMIPSolveInternal();
        }
        private void SaveBeforeMIPSolveInternal()
            {
            if (solver is OsiCbcSolverInterface)
            {
                // save the new reference solver including the new constraints.
                ((OsiCbcSolverInterface)solver).saveModelReferenceSolver();
            }
            else
            {
                saveColLower = solver.getColLower(); // a copy!
                saveColUpper = solver.getColUpper(); // a copy!
                solver.getDblParam(OsiDblParam.OsiDualObjectiveLimit, out saveOsiDualObjectiveLimit);
                solver.saveBaseModel();
            }
        }
        #endregion

        #region Generate methods
        /// <summary>
        /// Generate the given constraint, assuming it hasnt been generated before.
        /// If the model has _not_ been generated (yet), the constraint is not loaded into the solver here. Instead, this is done in bulk in ::Generate()
        /// If the model has been generated, the constraint is added into the solver here.
        /// </summary>
        /// <param name="con"></param>
        private void Generate(Constraint con)
        {
            Ensure.NotNull(con, "constraint");

            con.Assemble();
            con.Register(this);
            int offset = constraints.Count;
            ((ModelEntity)con).Assign(this, offset);	// immediately also Assign the offset!
            // the offset is either for model-use, or can be Stored at the constraint, when the solution is loaded into the variables and constraints

            constraints.Add(con);

            GenerateVariables(con.Coefficients);

            if (Generated)
            {
                // Now, the new constraints are added here.

                // if the model has already been generated, then this will load the constraint also into the MODEL
                CoefVector coefs = con.Coefficients;
                int numberElements = coefs.Count;
                double rowlb = con.Lower;
                double rowub = con.Upper;
                int[] columns = new int[numberElements];
                double[] element = new double[numberElements];

                numberElements = 0;
                int n = coefs.Count;
                for (int j = 0; j < n; j++, numberElements++)
                {
                    Coef c = coefs[j];
                    int voffset = Offset(c.var);
#if (DEBUG)
                    if (voffset == -1)
                        throw new SonnetException("Trying to use variable that is not part of this model!");
#endif
                    columns[numberElements] = voffset;
                    element[numberElements] = c.coef;
                }

                //virtual void addRow(int numberElements, const int * columns, const double * element,
                // const double rowlb, const double rowub) ;
                solver.addRow(numberElements, columns, element, rowlb, rowub);

                if (!con.Enabled)
                {
                    SetConstraintEnabled(con, false);
                }

                if (NameDiscipline > 0)
                {
                    solver.setRowName(Offset(con), con.Name);
                }
            }
        }

        /// <summary>
        /// Generate the given objective: Assemble the objective and register it to be part of this model. Also assign is to this model for quick reference.
        /// Then generate its (new) variables.
        /// If the model has _not_ been generated (yet), the objective is not loaded into the solver here. Instead, this is done in bulk in ::Generate()
        /// If the model has been generated, the constraint is added into the solver here.
        /// </summary>
        /// <param name="obj"></param>
        private void Generate(Objective obj)
        {
            Ensure.NotNull(obj, "objective");

            //this->objective->Unregister(this); // this should have already been done!

            // Registering the objective is important for changing coefficients: If we change coefs of an objective,
            // these changes have to be passed on to all registered models.
            obj.Assemble();
            obj.Register(this);
            obj.Assign(this, 0.0); // immediately also STORE!

            GenerateVariables(obj.Coefficients);

            if (Generated)
            {
                // now load the objective into the solver
                int n = NumberOfVariables;	// the NEW number of variables
                double[] c = new double[n];
                // initialise all the coefficient at 0.0
                for (int j = 0; j < n; j++)
                {
                    c[j] = 0.0;
                }

                CoefVector coefs = obj.Coefficients;
                for (int k = 0; k < coefs.Count; k++)
                {
                    Coef kc = coefs[k];
                    int j = Offset(kc.var);
                    c[j] = kc.coef;	// without objective->Assemble() (in Generate(obj), we could do +=
                }

                // which ever works faster..

                /*	// change coefficients individually
                for (int j = 0; j < n; j++)
                {
                solver->setObjCoeff(j, c[j]);
                }
                */

                // change all coefficients at the same time
                solver.setObjective(c);

                // skip this: doesnt work as expected with max/min problems
                // the constant part goes in via the ObjOffset
                // solver->setDblParam(OsiObjOffset, obj->Constant);
            }
        }

        private void Generate(Variable var)
        {
            Ensure.NotNull(var, "variable");

            // Registerd at the variable (mostly, this is cheaper than IsRegisterd(i->var), 
            // since a model with have more vars, than a var is registered at models
            var.Register(this);
            int offset = variables.Count;
            var.Assign(this, offset);	// immediately also Assign the offset!		

            variables.Add(var);		// from offset to variables;

            // if the model has already been generated before, then Add this new variable to the solver
            if (Generated)
            {
#if (SONNET_USE_SEMICONTVAR)
    			if (var is SemiContinuousVariable) throw new SonnetException("Cannot add semi continuous variables after the model was generated!");
#endif
#if (DEBUG)
                int oldn = solver.getNumCols();
#endif
                solver.addCol(0, null, null, var.Lower, var.Upper, 0.0);
#if (DEBUG)
                int newn = solver.getNumCols();
                if (oldn + 1 != newn) throw new SonnetException("Adding a variable to an already generated model failed! Variable was not added correctly.");

                if (Offset(var) != oldn) throw new SonnetException("Adding a variable to an already generated model failed! Variable was added with wrong offset.");
#endif
                SetVariableName(var, var.Name);
                if (var.Type == VariableType.Integer)
                {
                    solver.setInteger(Offset(var));
                }
            }
        }

        private void GenerateVariables(CoefVector av)
        {
            Ensure.NotNull(av, "coefvector");

            for (int i = 0; i < av.Count; i++)
            {
                Coef c = av[i];
                if (!c.var.IsRegistered(this)) Generate(c.var);
            }
        }

        ///<summary>
        /// Generates (builds) the model to be solved. This can be called explicitly, but is done automatically within a solve.
        ///</summary>
        public void Generate()
        {
            #region If already Generated
            if (Generated)
            {
                // CBCs and CLPs branchAndBound() behaves 'strangely': 
                // throughout the b&b, the bounds on variables etc (and dual limit)
                // are set. However, after the b&b is completed, these bounds still remain. Therefore, we save and reset to the originals.
                // Reset here doesnt work: a lot of changes are made outsite of Generate
                // for example change type of variable, individual bounds, etc.
                // which are undone by ResetAfterMIPSolve here..
                //if (ProblemType == ProblemType.MILP) ResetAfterMIPSolve();

                if (rawconstraints.Count > 0)
                {
                    for (int i = 0; i < rawconstraints.Count; )
                    {
                        Constraint con = rawconstraints[i];
                        try
                        {
                            Generate(con);
                        }
                        catch (System.Exception e)
                        {
                            string message = string.Format("Error generating constraint {0}.", con.Name);
                            throw new SonnetException(message, e);
                        }

                        Utils.Remove<Constraint>(rawconstraints, i);

                        //static_cast<ModelEntity *>(constraint)->Unassign();	// Assign was done in Generate(con)
                    }
                }
                return;
            }
            #endregion

            System.GC.Collect();

            if (variables.Count > 0)
            {
                string message = log.ErrorFormat("Cannot generate an already generated model: Variables are existing. Either Ungenerate, or dont Generate.");
                throw new SonnetException(message);
            }

            if (constraints.Count > 0)
            {
                string message = log.ErrorFormat("Cannot generate an already generated model: Constraints are existing. Either Ungenerate, or dont Generate.");
                throw new SonnetException(message);
            }

            // Generate coefficient matrix and right hand side
            // to "assemble" means to check that all the coefficients of one variable
            // in one constaint are added up, to get the true coefficient of this variable
            // skipping assembling can be done if this is not necessary since this condition
            // is always met.. 
            double genStart = CoinUtils.CoinCpuTime();
            int nz = 0;

            // Assemble objective function coefficients

            Generate(objective); // insert the variables in the objective into the overall set Variables

            log.DebugFormat("Done generating objective after {0}.", (CoinUtils.CoinCpuTime() - genStart));

            for (int i = 0; i < rawconstraints.Count; )
            {
                Constraint con = rawconstraints[i];
                try
                {
                    Generate(con);
                }
                catch (System.Exception e)
                {
                    string message = log.ErrorFormat("Error generating constraint {0}.", con.Name);
                    throw new SonnetException(message, e);
                }

                CoefVector coefs = con.Coefficients;

                nz += coefs.Count;
                Utils.Remove<Constraint>(rawconstraints, i);
            }

#if (SONNET_USE_SEMICONTVAR)
		// now that all the regular constraints and variables are registered,
		// register the additional constraints and helper variables for any semi-continuous variables
		for (int j = 0; j < variables->Count; j++)
		{
			Variable ^var = variables[j];
			if (var->GetType()->Equals(__typeof(SemiContinuousVariable)))
			{
				SemiContinuousVariable *scvar = static_cast<SemiContinuousVariable *>(var);
				double sclower = scvar->SemiContinuousLower;
				double upper = scvar->Upper;
				String ^name = String::Concat(scvar->Name, "SCHelper");

				Variable ^scvarHelper = gcnew Variable(name, 0.0, 1.0, VariableType::Integer);

				Constraint ^con = gcnew Constraint(String::Concat(name, "Con1"), gcnew Expression(scvar), ConstraintType::LE, gcnew Expression(upper, scvarHelper));
				try
				{
					Generate(con);
				}
				catch (System::Exception^ e)
				{
					String ^message = String::Concat("Error generating constraint ", con->Name, ".");
					throw new SonnetException(message, e);
				}

				CoefVector *coefs = con->Coefficients;

				nz += coefs->size();

				con = gcnew Constraint(String::Concat(name, "Con2"), gcnew Expression(sclower, scvarHelper), ConstraintType::LE, gcnew Expression(scvar));

				Generate(con)
				coefs = con->Coefficients;

				nz += coefs->size();
			}
		}
#endif

            log.DebugFormat("Done generating matrix after ", (CoinUtils.CoinCpuTime() - genStart));

            unsafe
            {
                int n = variables.Count;
                int m = constraints.Count;

                double* Elm;	// The nonzero elements
                int* Rnr;		// The constraint index number per nonzero element
                int* Cst;		// per variable, the starting position of its nonzero data
                int* Clg;		// per variable, the number of nonzeros in its column
                double* c;		// per variable, the objective function coefficient
                double cOffset;  // the objective function constant term (the offset)
                double* l;		// lowerbound per variable
                double* u;		// upperbound per variable
                double* bl;		// lowerbound per constraint
                double* bu;		// upperbound per constraint

#if (LEANLOADPROBLEM)
                if (solver is OsiClpSolverInterface)
                {
                    // Especially for memory (and other performance issues)
                    // we want to prevent the copying of large arrays of data
                    // Therefore, especially for the CLP solver, we will
                    // use CLP arrays directory.
                    OsiClpSolverInterface osiClp = (OsiClpSolverInterface)solver;
                    osiClp.LeanLoadProblemInit(n, m, nz, &Cst, &Clg, &Rnr, &Elm, &l, &u, &c, &bl, &bu);
                }
                else
#endif
                {
                    Elm = CoinUtils.NewDoubleArray(nz);		// The nonzero elements
                    Rnr = CoinUtils.NewIntArray(nz);				// The constraint index number per nonzero element
                    Cst = CoinUtils.NewIntArray(n + 1);			// per variable, the starting position of its nonzero data
                    Clg = CoinUtils.NewIntArray(n);				// per variable, the number of nonzeros in its column
                    c = CoinUtils.NewDoubleArray(n);			// per variable, the objective function coefficient
                    l = CoinUtils.NewDoubleArray(n);		// lowerbound per variable
                    u = CoinUtils.NewDoubleArray(n);		// upperbound per variable
                    bl = CoinUtils.NewDoubleArray(m);		// lowerbound per constraint
                    bu = CoinUtils.NewDoubleArray(m);		// upperbound per constraint
                }

                double inf = Infinity;

                // set the number of nonzeros per column to zero.
                for (int j = 0; j < n; j++)
                {
                    Clg[j] = 0;
                }

                // calculate the number of nonzeros per variable
                foreach (Constraint con in Constraints)
                {
                    CoefVector coefs = con.Coefficients;
                    //double rhs = con.RhsConstant;

                    foreach (Coef coef in coefs)
                    {
                        int col = Offset(coef.var);
#if (DEBUG)
                        if (coef.var.AssignedSolver != this) throw new SonnetException("Trying to use variable that is not part of this model!");

                        if (col < 0 || col >= n) throw new SonnetException("Variable offset has error value.");
#endif
                        Clg[col]++;
                    }
                }

                Cst[0] = 0;
                for (int j = 0; j < n; j++)
                {
                    Cst[j + 1] = Cst[j] + Clg[j];			// calculate the starting positions per variable
                    Clg[j] = 0;						// reset the number of nonzeros per variable
                }

                // now start the real seting of the nonzero elements
                foreach (Constraint con in Constraints)
                {
                    CoefVector coefs = con.Coefficients;
                    //double rhs = con.RhsConstant;
                    int row = Offset(con);

                    int ki = 0;
                    foreach (Coef coef in coefs)
                    {
                        int col = Offset(coef.var);
#if (DEBUG)
                        if (col < 0 || col >= n) throw new SonnetException("Variable offset has error value.");
#endif

                        Elm[Cst[col] + Clg[col]] = coef.coef;
                        Rnr[Cst[col] + Clg[col]] = row;
                        Clg[col]++;

                        ki++;
                    }
                }

                // Row bounds
                //Type *rangeConstraintType = __typeof(SONNET.RangeConstraint);
                foreach (Constraint con in Constraints)
                {
                    int row = Offset(con);
                    bu[row] = con.Upper;
                    bl[row] = con.Lower;

                    //double rhs = con.RhsConstant;
                    //if (con.GetType().Equals(rangeConstraintType))
                    //{
                    //	RangeConstraint ^ rangeCon = static_cast<RangeConstraint ^>(con);
                    //	bu[row] = rangeCon.Upper;
                    //	bl[row] = rangeCon.Lower;
                    //}
                    //else
                    //{
                    //	switch (con.Type) 
                    //	{
                    //	case LE:
                    //		bu[row] = rhs;
                    //		bl[row] = - inf;
                    //		break;
                    //	case GE:
                    //		bu[row] = inf;
                    //		bl[row] = rhs;
                    //		break;
                    //	case EQ:
                    //		bu[row] = rhs;
                    //		bl[row] = rhs;
                    //		break;		
                    //	}
                    //}
                }

                // generate the objective function coefficients :
                // 1) the c array is NOT given with only the nonzeros, so we have to set all to zero first
                for (int j = 0; j < n; j++)
                {
                    c[j] = 0.0;
                }

                // generate the objective function coefficients :
                // 2) assign the non-zero coefs
                CoefVector objcoefs = objective.Coefficients;
                foreach (Coef coef in objcoefs)
                {
                    int col = Offset(coef.var);
                    c[col] = coef.coef;
                }
                cOffset = objective.Constant;

                // generate the column bounds
                // This is not necessary, since all bounds are applied below
                //for (int j=0; j<n; j++) 
                //{
                //	l[j] = 0.0;
                //	u[j] = inf;
                //}

                foreach (Variable var in variables)
                {
                    int col = Offset(var);

                    // I'm not sure if we should even bother transforming the max/min infinity bounds
                    // This should be checked and handled at the solver side.
                    l[col] = var.Lower;
                    u[col] = var.Upper;
                }

                Trace.WriteLine(string.Concat("Ready to load the problem after ", (CoinUtils.CoinCpuTime() - genStart)));

                // note that the model is loaded in standard form:
                // all constraints are of type   bl <= expression <= bu !
                // where the sense of the constraint has been translated into correct bl and bu (see above)
                // If we use loadProblem with constraints based on rowsense info, then the passed Ranges are only used for Range (R) rows!

                /* Just like the other loadProblem() methods except that the matrix is
                given in a standard column major ordered format (without gaps). */
                //solver.loadProblem(n, m, Cst, Rnr, Elm, l, u, c, rowsen, rowrhs, rowrng);

#if (LEANLOADPROBLEM)
                if (solver is OsiClpSolverInterface)
                {
                    // special for Clp
                    Trace.WriteLine("Using new CLP-specific matrix loading!");

                    OsiClpSolverInterface osiClp = (OsiClpSolverInterface)solver;
                    osiClp.LeanLoadProblem(n, m, nz, &Cst, &Clg, &Rnr, &Elm, &l, &u, &c, &bl, &bu);
                }
                else
#endif
                {
                    solver.loadProblemUnsafe(n, m, Cst, Rnr, Elm, l, u, c, bl, bu);
                }

                // skip this: doesnt work as expected with max/min problems
                // the constant part goes in via the ObjOffset
                //solver.setDblParam(OsiObjOffset, cOffset);
            } // end unsafe

            Trace.WriteLine("Problem fully loaded after " + (CoinUtils.CoinCpuTime() - genStart));

            foreach (Variable var in variables)
            {
                int col = Offset(var); //j.second;

                if (var.Type == VariableType.Integer)
                {
                    solver.setInteger(col);
                }
            }

            foreach (Constraint con in Constraints)
            {
                int row = Offset(con);

                if (!con.Enabled)
                {
                    this.SetConstraintEnabled(con, false);
                }
            }

            Trace.WriteLine("  number of variables  : " + (NumberOfVariables));
            Trace.WriteLine("  number of constraints: " + (NumberOfConstraints));
            Trace.WriteLine("  number of elements   : " + (NumberOfElements));

            //static_cast<ModelEntity *>(objective).Unassign();

            // Full Names
            if (NameDiscipline > 0)
            {
                foreach (Constraint con in Constraints)
                {
                    int offset = Offset(con);
                    solver.setRowName(offset, con.Name);
                }
                Trace.WriteLine(string.Concat("done naming constraints after ", (CoinUtils.CoinCpuTime() - genStart)));

                foreach (Variable var in variables)
                {
                    int offset = Offset(var);

                    solver.setColName(offset, var.Name);
                }

                Trace.WriteLine(string.Concat("done naming variables after ", (CoinUtils.CoinCpuTime() - genStart)));
            }

            // Dump Hint Settings
            foreach (OsiHintParam hintParam in Enum.GetValues(typeof(OsiHintParam)))
            {
                if (hintParam == OsiHintParam.OsiLastHintParam) continue;

                bool yesNo;
                OsiHintStrength hintStrength;

                solver.getHintParam(hintParam, out yesNo, out hintStrength);
                Trace.WriteLine(string.Concat("Hint: ", hintParam, " : ", (yesNo), " at ", hintStrength));
            }

            System.GC.Collect();
            generated = true;

            //if (ProblemType == ProblemType.MILP) SaveBeforeMIPSolve();
        }

        /// <summary>
        /// Ungenerate the model
        /// </summary>
        public void UnGenerate()
        {
            if (Generated)
            {
                generated = false;

#if (DEBUG)
                if (object.ReferenceEquals(null, objective)) throw new NullReferenceException("Ungenerate: A generated model must have a valid objective function");
                if (!objective.IsRegistered(this)) throw new SonnetException("Ungenerate: A generated model must have a registered objective function");
#endif
                objective.Unregister(this);
                foreach (Constraint con in constraints)
                {
#if (DEBUG)
                    if (!con.IsRegistered(this)) throw new SonnetException("Ungenerate: A generated constraint must have be registered");
#endif
                    con.Unregister(this);
                    rawconstraints.Add(con);
                }
                // empty constraints
                constraints.Clear();

                foreach (Variable var in variables)
                {
#if (DEBUG)
                    if (!var.IsRegistered(this)) throw new SonnetException("Ungenerate: A generated variable must have be registered");
#endif
                    var.Unregister(this);
                }
                // empty variables and variablesMap
                variables.Clear();
            }
        }
        #endregion

        /// <summary>
        /// Exports the model in either MPS, LP or SONNET format, depending on the extension of the given filename
        /// Note, after solving a model, the Bounds etc could be left at non-original values!
        /// If you want to export the original bounds etc, then call Generate() before Exporting.
        /// </summary>
        /// <param name="filename"></param>
        public void ExportModel(string filename)
        {
            string directoryName = System.IO.Path.GetDirectoryName(filename);
            if (directoryName.Length == 0) directoryName = ".";

            string fullPathWithoutExtension = string.Concat(directoryName, System.IO.Path.DirectorySeparatorChar, System.IO.Path.GetFileNameWithoutExtension(filename));
            string extension = System.IO.Path.GetExtension(filename);

            if (extension.Equals(".mps"))
            {
                solver.writeMps(fullPathWithoutExtension);//, solver.getObjValue());
            }
            else if (extension.Equals(".lp"))
            {
                solver.writeLp(fullPathWithoutExtension);
            }
            else if (extension.Equals(".sonnet"))
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);
                sw.WriteLine(ToString());
                sw.Close();
            }
            else
            {
                throw new SonnetException(string.Concat("Cannot export file ", filename, " : unknown extension '", extension, "'."));
            }
        }

        #region ToString methods
        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();

            foreach (Variable aVar in Variables)
            {
                tmp.Append(aVar.ToString());
                tmp.Append("\n");
            }

            tmp.Append("Objective: ");
            if (object.ReferenceEquals(null, objective))
            {
                tmp.Append("No objective defined");
                tmp.Append("\n");
            }
            else
            {
                tmp.Append(objective.ToString());
                tmp.Append("\n");
            }

            if (rawconstraints.Count > 0)
            {
                tmp.Append("Ungenerated constraints:\n");
                foreach (Constraint aCon in rawconstraints)
                {
                    tmp.Append(aCon.ToString());
                    tmp.Append("\n");
                }
            }

            if (constraints.Count > 0)
            {
                tmp.Append("Constraints:\n");
                foreach (Constraint aCon in Constraints)
                {
                    tmp.Append(aCon.ToString());
                    tmp.Append("\n");
                }
            }

            if (rawconstraints.Count == 0 && constraints.Count == 0)
            {
                tmp.Append("Model does not contain any constraints!");
            }

            return tmp.ToString();
        }

        public string ToSolutionString()
        {
            StringBuilder tmp = new StringBuilder();

            tmp.Append(string.Concat("Model status: ", ((this.IsProvenOptimal) ? ("Optimal") : "not Optimal")));
            tmp.Append("\n");
            tmp.Append("Objective:");
            tmp.Append(objective.Level());
            tmp.Append("\n");

            tmp.Append("Variables:\n");
            int n = variables.Count;
            for (int j = 0; j < n; j++)
            {
                Variable aVar = variables[j];
                tmp.Append(aVar.ToLevelString());
                tmp.Append("\n");
            }

            tmp.Append("Constraints:\n");
            for (int i = 0; i < constraints.Count; i++)
            {
                Constraint aCon = constraints[i];
                tmp.Append(aCon.ToLevelString());
                tmp.Append("\n");
            }

            tmp.Append("End");
            return tmp.ToString();
        }
        
        public string ToStatisticsString()
        {
            StringBuilder tmp = new StringBuilder();
            tmp.Append("Model stastics for model ").Append(Name).Append("\n");
            tmp.Append(" Number of variables  : ").Append(NumberOfVariables.ToString()).Append("\n");
            tmp.Append(" Number of constraints: ").Append(NumberOfConstraints.ToString()).Append("\n");
            tmp.Append(" Number of elements   : ").Append(NumberOfElements.ToString()).Append("\n");

            return tmp.ToString();

        }
        #endregion

        public bool IsRegistered(Objective obj)
        {
            Ensure.NotNull(obj, "objective");

            return this.objective.Equals(obj);
        }
        public bool IsRegistered(Variable v)
        {
            Ensure.NotNull(v, "variable");

            foreach (Variable variable in variables)
            {
                if (object.Equals(variable, v)) return true;
            }

            return false;
        }
        public bool IsRegistered(Constraint c)
        {
            Ensure.NotNull(c, "constraint");

            foreach (Constraint constraint in constraints)
            {
                if (object.Equals(constraint, c)) return true;
            }

            return false;
        }

        //bool Contains(Variable v);		// IsRegistered OR in the (to-be) added list -> NOT do this, since we'd have to check all the new constraints 

        /// <summary>
        /// IsRegistered OR in the (to-be) added list
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool Contains(Constraint c)
        {
            // first, check the registered constraint
            if (IsRegistered(c)) return true;

            // otherwise, check the constraints still to be added.
            foreach (Constraint constraint in rawconstraints)
            {
                if (Equals(constraint, c)) return true;
            }
            return false;
        }

        /// <summary>
        /// Get the element index (offset) of the constraint in this model. An exception is thrown if no offset found.
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private int Offset(Constraint con)
        {
            Ensure.NotNull(con, "constraint");

#if (DEBUG)
            if (!con.IsRegistered(this)) throw new SonnetException("Constraint not registered with model.");
#endif
            int offset = -1;
            if (this == con.AssignedSolver) offset = con.Offset;
            else offset = constraints.IndexOf(con);

            if (offset < 0 || offset >= constraints.Count) throw new SonnetException(string.Format("Error retrieving offset of constraint {0} : {1}", con.Name, offset));
            return offset;
        }

        /// <summary>
        /// Get the element index (offset) of the variable in this model
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        private int Offset(Variable var)
        {
            Ensure.NotNull(var, "variable");

#if (DEBUG)
            if (!var.IsRegistered(this))
            {
                if (Generated) throw new SonnetException("Variable not registered with model.");
                else throw new SonnetException("Variable not registered with model, because the model is not generated.");
            }
#endif
            int offset = -1;
            if (this == var.AssignedSolver) offset = var.Offset;
            else offset = variables.IndexOf(var);

            if (offset < 0 || offset >= variables.Count) throw new SonnetException(string.Format("Error retrieving offset of variable {0} : {1}", var.Name, offset));
            return offset;
        }

        /// <summary>
        /// Gets the generated constraints
        /// </summary>
        public IEnumerable<Constraint> Constraints
        {
            get { return constraints; }
        }

        /// <summary>
        /// Gets the variables currenlty generated
        /// </summary>
        public IEnumerable<Variable> Variables
        {
            get { return variables; }
        }

        /// <summary>
        /// Return the constraint, given its element index in this model
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Constraint GetConstraint(int index)
        {
            if (index < 0 || index >= constraints.Count)
            {
                throw new SonnetException(string.Format("Offset of value {0} is not allowed for constraints", index));
            }

            return constraints[index];
        }

        /// <summary>
        /// Return the variable, given its element index in this model
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Variable GetVariable(int index)
        {
            if (index < 0 || index >= variables.Count)
            {
                throw new SonnetException(string.Format("Offset of value {0} is not allowed for variables", index));
            }

            return variables[index];
        }

        /// <summary>
        /// return the constraint, given its element index in this model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Constraint GetConstraint(string name)
        {
            foreach (Constraint con in constraints) if (con.Name.Equals(name)) return con;
            foreach (Constraint con in rawconstraints) if (con.Name.Equals(name)) return con;
            return null;
        }

        // we dont (bother to) store the constraint and variable names in a Map, so we just search
        // NOTE: since we dont EXplicitly add variables to any model, but rather this is done
        // implicitly when the constraints are generated, this means that we cannot find a variable by name
        // until the model is generated, and thus the list "variables" is built.
        // Of course, we could enforce that all variables be explicitly add, just like constraints, but 
        // I dont think this will be necessary.
        // Alternatively, we could also find the variable by going passed all the variables in the constraints....


        /// <summary>
        /// return the variable, given its element index in this model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Variable GetVariable(string name)
        {
            foreach (Variable var in variables) if (var.Name.Equals(name)) return var;
            return null;
        }

        #region Obsolete/Deprecated methods
        /// <summary>
        /// Deprecated: use property of Variable
        /// looks up the ElementID, and retreives value (no storing)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [Obsolete("Deprecated: use property Objective.Value", true)]
        public double Value(Variable v)
        {
            Ensure.NotNull(v, "variable");

            int offset = Offset(v);
            unsafe
            {
                double* values = solver.getColSolutionUnsafe();
                return values[offset];
            }
        }

        /// <summary>
        /// Deprecated: use property of Variable
        /// same, but for the reduced costs (no storing)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [Obsolete("Deprecated: use property of Variable", true)]
        public double ReducedCost(Variable v)
        {
            Ensure.NotNull(v, "variable");

            int offset = Offset(v);
            unsafe
            {
                double* values = solver.getReducedCostUnsafe();
                return values[offset];
            }
        }

        /// <summary>
        /// Deprecated: Use property of Constraint.
        /// same, but for the prices of constraints (dual sol) (no storing)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [Obsolete("Deprecated: Use property of Constraint", true)]
        public double Price(Constraint c)
        {
            Ensure.NotNull(c, "constraint");

            int offset = Offset(c);
            unsafe
            {
                double* values = solver.getRowPriceUnsafe();
                return values[offset];
            }
        }
        #endregion

        #region Assign Solution methods
        public void AssignSolution()
        {
            AssignSolutionStatus();
            AssignVariableSolution();
            AssignConstraintSolution();
        }

        /// <summary>
        /// Get and store the solution status values (isProvenOptimal etc)
        /// </summary>
        private void AssignSolutionStatus()
        {
            Ensure.NotNull(solver, "solver");

            isAbandoned = solver.isAbandoned();
            isProvenOptimal = solver.isProvenOptimal();
            isProvenPrimalInfeasible = solver.isProvenPrimalInfeasible();
            isProvenDualInfeasible = solver.isProvenDualInfeasible();
            isPrimalObjectiveLimitReached = solver.isPrimalObjectiveLimitReached();
            isDualObjectiveLimitReached = solver.isDualObjectiveLimitReached();
            isIterationLimitReached = solver.isIterationLimitReached();
            iterationCount = solver.getIterationCount();
        }


        /// <summary>
        /// Get and store the primal solution values to the variables and reduced cost
        /// </summary>
        private void AssignVariableSolution()
        {
#if (DEBUG)
            int n_variables = variables.Count;
            int n_solver = solver.getNumCols();

            if (n_variables != n_solver)
            {
                throw new SonnetException(string.Format("Number of variables in the solution {0} is not equal to the number of registered variables {1}", n_variables, n_solver));
            }
#endif
            unsafe
            {
                double* values = solver.getColSolutionUnsafe();
                double* reducedCost = solver.getReducedCostUnsafe();

                for (int col = 0; col < variables.Count; col++)
                {
                    Variable var = variables[col];

                    var.Assign(this, col, values[col], reducedCost[col]);
#if (DEBUG)
                    if (IsProvenOptimal)
                    {
                        if (!values[col].IsBetween(var.Lower, var.Upper))
                        {
                            Debug.WriteLine(string.Format("Solution is optimal, but variable value {0} is outside of bounds [{1},{2}] ", var, var.Lower, var.Upper));
                        }
                    }
#endif
                }
            } // unsafe

            objective.Assign(this, solver.getObjValue() + objective.Constant);

#if (DEBUG)
            if (IsProvenOptimal)
            {
                if (objective.Level().CompareToEps(objective.Value) != 0)
                {
                    Trace.WriteLine(string.Format("Solution is optimal, but objective value {0} is not corrected for constant term (offset) {1}", objective.Value, objective.Level()));
                }
            }
#endif
        }

        /// <summary>
        /// get and store the values of the primal constraints in the dual solution
        /// get and propagate the values of the primal constraints in the dual solution
        /// and the total LHS( or "middle") value of the constraints in the current (primal) solution, with all var. moved left
        /// </summary>
        private void AssignConstraintSolution()
        {
#if (DEBUG)
            int m_constraints = constraints.Count;
            int m_solver = solver.getNumRows();
            if (m_constraints != m_solver)
            {
                throw new SonnetException(string.Format("Number of constraints in the solution {0} is not equal to the number of registered variables {1}", m_constraints, m_solver));
            }
#endif

            unsafe
            {
                double* prices = solver.getRowPriceUnsafe();
                double* values = solver.getRowActivityUnsafe();
                for (int row = 0; row < constraints.Count; row++)
                {
                    Constraint con = constraints[row];
                    con.Assign(this, row, prices[row], values[row]);
                }
            }
        }
        #endregion //Assign Solution methods

        public bool IsSolving { get { return this.isSolving; } }
        /// <summary>
        /// Are there numerical difficulties?
        /// </summary>
        public bool IsAbandoned { get { return this.isAbandoned; } }
        /// <summary>
        /// Is optimality proven?
        /// </summary>
        public bool IsProvenOptimal { get { return this.isProvenOptimal; } }
        /// <summary>
        /// Is primal infeasiblity proven?
        /// </summary>
        public bool IsProvenPrimalInfeasible { get { return this.isProvenPrimalInfeasible; } }
        /// <summary>
        /// Is dual infeasiblity proven?
        /// </summary>
        public bool IsProvenDualInfeasible { get { return this.isProvenDualInfeasible; } }
        /// <summary>
        /// Is the given primal objective limit reached?
        /// </summary>
        public bool IsPrimalObjectiveLimitReached { get { return this.isPrimalObjectiveLimitReached; } }
        /// <summary>
        /// Is the given dual objective limit reached?
        /// </summary>
        public bool IsDualObjectiveLimitReached { get { return this.isDualObjectiveLimitReached; } }
        /// <summary>
        /// Iteration limit reached?
        /// </summary>
        public bool IsIterationLimitReached { get { return this.isIterationLimitReached; } }
        /// <summary>
        /// Get the number of iterations it took to solve the (latest) problem (whatever ``iteration'' means to the solver).
        /// </summary>
        public int IterationCount { get { return this.iterationCount; } }


        #region Changing Solver data
        // Using these methods to change variables, constraints, or the objective, will NOT change the variable etc. data itself!
        // methods for changing variables
        internal void SetVariableUpper(Variable var, double upper)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(var);
            solver.setColUpper(offset, upper);
        }
        internal void SetVariableLower(Variable var, double lower)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(var);
            solver.setColLower(offset, lower);
        }
        internal void SetVariableBounds(Variable var, double lower, double upper)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(var);
            solver.setColBounds(offset, lower, upper);
        }
        internal void SetVariableType(Variable var, VariableType type)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(var);
            if (type == VariableType.Continuous) solver.setContinuous(offset);
            else if (type == VariableType.Integer) solver.setInteger(offset);
        }

        /// <summary>
        /// Set the name of the given variable within the solver (only) 
        /// </summary>
        /// <param name="var"></param>
        /// <param name="name"></param>
        internal void SetVariableName(Variable var, string name)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(name, "name");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(var);
            solver.setColName(offset, name);
        }

        /// <summary>
        /// methods for changing the objective function
        /// </summary>
        /// <param name="var"></param>
        /// <param name="value"></param>
        internal void SetObjectiveCoefficient(Variable var, double value)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(var);
            solver.setObjCoeff(offset, value);
        }
        
#if (CONSTRAINT_SET_COEF)
        // methods for changing Range Constraints
        internal void SetCoefficient(RangeConstraint con, Variable var, double value)
        {
            Ensure.NotNull(con, "range constraint");
            Ensure.NotNull(var, "variable");
            Ensure.NotNull(solver, "solver");

            int conOffset = Offset(con);
            int varOffset = Offset(var);

            solver.setCoef(conOffset, varOffset, value);
        }
#endif
        internal virtual void SetConstraintUpper(RangeConstraint con, double upper)
        {
            Ensure.NotNull(con, "range constraint");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(con);
            //if (SolverType == SolverType.CpxSolver) solver.setRowBounds(offset, con.Lower, upper);
            solver.setRowUpper(offset, upper);
        }
        internal void SetConstraintLower(RangeConstraint con, double lower)
        {
            Ensure.NotNull(con, "range constraint");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(con);
            //if (SolverType == SolverType.CpxSolver) solver.setRowBounds(offset, lower, con.Upper);
            solver.setRowLower(offset, lower);
        }
        internal void SetConstraintBounds(RangeConstraint con, double lower, double upper)
        {
            Ensure.NotNull(con, "range constraint");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(con);
            solver.setRowBounds(offset, lower, upper);
        }
        internal void SetConstraintEnabled(Constraint con, bool enable)
        {
            Ensure.NotNull(con, "range constraint");
            Ensure.NotNull(solver, "solver");

            int offset = Offset(con);
            if (!enable)
            {
                double range = con.Upper - con.Lower;
                solver.setRowType(offset, (sbyte)'N', con.RhsConstant, range);
            }
            else
            {
                double range = con.Upper - con.Lower;
                // the range is not used in this method, unless the new type is a Range constraint
                if (con is RangeConstraint)
                {
                    solver.setRowType(offset, (sbyte)'R', con.RhsConstant, range);
                }
                else
                {
                    char sense = con.Type.GetOsiConstraintType();
                    solver.setRowType(offset, (sbyte)sense, con.RhsConstant, range);
                }
            }
        }
        internal void SetConstraintName(Constraint con, string name)
        {
            Ensure.NotNull(con, "constraint");
            Ensure.NotNull(solver, "solver");

            if (NameDiscipline == 0) return;

            int offset = Offset(con);
            solver.setRowName(offset, name);
        }
        #endregion

        private void GutsOfConstructor(Model model, OsiSolverInterface solver)
        {
            Ensure.NotNull(model, "model");
            Ensure.NotNull(solver, "solver");
            id = numberOfSolvers++;

            DumpAssemblyInfo();

            this.model = model;
            this.solver = solver;
            generated = false;

            model.Register(this);

            objective = model.Objective;
            variables = new List<Variable>();
            rawconstraints = new List<Constraint>(model.Constraints);
            constraints = new List<Constraint>();

            log.PassToSolver(solver);
            ApplyObjectiveSense(model.ObjectiveSense);

            log.InfoFormat("Model {0} created with solver {1}", Name, "TODO");
        }

        private static void DumpAssemblyInfo()
        {
            Trace.WriteLine("Initialising SONNET Model.\nAssembly information:");
            Trace.WriteLine(String.Concat("File path: ", System.Reflection.Assembly.GetExecutingAssembly().Location));
            Trace.WriteLine(String.Concat("File date: ", System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString()));
            Trace.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().FullName);
            Trace.WriteLine(String.Concat("Framework: ", Environment.Version.ToString()));
            Trace.WriteLine(String.Concat("Assembly runtime version: ", System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion));

            System.Reflection.PortableExecutableKinds portableExecutableKinds;
            System.Reflection.ImageFileMachine imageFileMachine;
            System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.GetPEKind(out portableExecutableKinds, out imageFileMachine);
            Trace.WriteLine("Portable executable kinds: " + portableExecutableKinds.ToString());
            Trace.WriteLine("Image file machine: " + imageFileMachine.ToString());
            Trace.WriteLine("----------------------------------------------------------");
        }

        private static int numberOfSolvers = 0;

        private Objective objective;
        private List<Variable> variables;
        private List<Constraint> constraints;
        private List<Constraint> rawconstraints;

        private bool generated;
        private bool autoResetMIPSolve = true;
        private double saveOsiDualObjectiveLimit;
        private double[] saveColLower;
        private double[] saveColUpper;

        // solution status
        private bool isAbandoned;
        private bool isProvenOptimal;
        private bool isProvenPrimalInfeasible;
        private bool isProvenDualInfeasible;
        private bool isPrimalObjectiveLimitReached;
        private bool isDualObjectiveLimitReached;
        private bool isIterationLimitReached;
        private int iterationCount;

        private bool isSolving = false; // used for interrupting a solve

        OsiSolverInterface solver;
        Model model;

        #region IDisposable Members

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                model.Unregister(this);

                // Free other state (managed objects).
                UnGenerate();

                this.objective = null;
                this.rawconstraints.Clear();
                this.rawconstraints = null;

                solver.Dispose();
                solver = null;
            }

            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        // Use C# destructor syntax for finalization code.
        ~Solver()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion

    }
}
