// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

using COIN;

namespace Sonnet
{
    /// <summary>
    /// The Solver class is responsible of optimizing the problems represented by a Model.
    /// This class is the main link back to the COIN Osi solvers (OsiSolverInterface) through the SonnetWrapper.
    /// The Solver can only be constructed for a given type of OsiSolverInterface, or for 
    /// a specific instance derived from OsiSolverInterface. 
    /// The OsiSolver can be retrieved using the OsiSolver proporty.
    /// The available classes in SonnetWrapper that derive from OsiSolverInterface can be retrieved using Solver.GetSolverTypes().
    /// Ideally, the model is built before creating the Solver. Changes after the construction of the solver involve more overhead.
    /// Before the model is given to the OsiSolver, it will be Generated automatically (or explicitly). At this point, the
    /// constraints of the model are transformed into the constraint matrix that is loaded into the OsiSolver.
    /// </summary>
    public class Solver : Named, IDisposable
    {
        private static SonnetLog log = SonnetLog.Default;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Solver class with the given name and model, 
        /// and using the given instance derived from OsiSolverInterface.
        /// </summary>
        /// <param name="model">The model used in this solver.</param>
        /// <param name="solver">The instance of an OsiSolver, eg, OsiClpSolverInterface to be used.</param>
        /// <param name="name">The name for this solver.</param>
        public Solver(Model model, OsiSolverInterface solver, string name = null)
        {
            Ensure.NotNull(model, "model");
            Ensure.NotNull(solver, "solver");

            GutsOfConstructor(model, solver, name);
        }

        /// <summary>
        /// Initializes a new instance of the Solver class with the given name and model,
        /// and using a to be constructed instance of the given type derived from OsiSolverInterface.
        /// </summary>
        /// <param name="model">The model used in this solver.</param>
        /// <param name="osiSolverInterfaceType">The type derived from OsiSolverInterface to be used.</param>
        /// <param name="name">The name for this solver.</param>
        public Solver(Model model, Type osiSolverInterfaceType, string name = null)
        {
            Ensure.NotNull(model, "model");
            Ensure.Is<COIN.OsiSolverInterface>(osiSolverInterfaceType, "osiSolverInterfaceType");

            OsiSolverInterface solver = (OsiSolverInterface)osiSolverInterfaceType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
            GutsOfConstructor(model, solver, name);
        }
        
        /// <summary>
        /// Initializes a new instance of the Solver class with the given name and model,
        /// for using a to be constructed instance of the given type derived from OsiSolverInterface.
        /// </summary>
        /// <typeparam name="T">The type of OsiSolver to be used, derived from OsiSolverInterface.</typeparam>
        /// <param name="model">The model used in this solver.</param>
        /// <param name="name">The name for this solver.</param>
        /// <returns>The new instance of the Solver class.</returns>
        public static Solver New<T>(Model model, string name = null)
            where T : OsiSolverInterface
        {
            Ensure.NotNull(model, "model");

            return new Solver(model, typeof(T), name);
        }
        #endregion

        /// <summary>
        /// Gets or sets the amount of logging for the current solver
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
        /// Returns true iff there is at least one integer variable
        /// </summary>
        public bool IsMIP 
        { 
            get 
            {
                Generate();
                foreach (Variable var in variables)
                {
                    if (var.Type == VariableType.Integer) return true;
                }
                return false;
            } 
        }

        #endregion

        #region Static Properties
        /// <summary>
        /// Gets the Version number of this assembly
        /// </summary>
        public static Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

        /// <summary>
        /// Return all types that are derived from OsiSolverInterface in the SonnetWrapper.
        /// </summary>
        /// <returns>An array of Types within the SonnetWrapper that are derived from OsiSolverInterface.</returns>
        public static Type[] GetOsiSolverTypes()
        {
            Type[] types = System.Reflection.Assembly.GetAssembly(typeof(COIN.OsiSolverInterface)).GetTypes();
            List<Type> osiTypes = new List<Type>();
            foreach (Type type in types)
            {
                
                if (type.IsSubclassOf(typeof(COIN.OsiSolverInterface)) &&
                    !type.IsAbstract)
                {
                    osiTypes.Add(type);
                }
            }
            return osiTypes.ToArray();
        }
        #endregion

        /// <summary>
        /// Get the model of the current solver
        /// </summary>
        public Model Model
        {
            get { return this.model; }
        }

        /// <summary>
        /// Get the used OsiSolver
        /// </summary>
        public OsiSolverInterface OsiSolver
        {
            get { return this.solver; }
        }

        /// <summary>
        /// Get the full name of the type of OsiSolver.
        /// For example, "COIN.OsiClpSolverInterface"
        /// </summary>
        public string OsiSolverFullName
        {
            get { return this.solver.GetType().FullName; }
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
        /// Adds (a reference) the given constraint to the model
        /// </summary>
        /// <param name="con">The constraint to add to the model.</param>
        internal void Add(Constraint con)
        {
            Ensure.NotNull(con, "con");

            rawconstraints.Add(con);
        }

        #region Solve, Resolve, Maximise and Minimise Methods
        ///<summary>
        /// Maximises the model.
        /// This implicitly sets the objective sense to Maximise.
        /// The method uses branch and bound if model is MIP and LP otherwise.
        /// </summary>
        /// <param name="forceRelaxation">Force solving of the LP relaxation</param>
        public void Maximise(bool forceRelaxation = false)
        {
            ApplyObjectiveSense(ObjectiveSense.Maximise);
            if (!Generated) Solve(forceRelaxation);
            else Resolve(forceRelaxation);
        }

        ///<summary>
        /// Minimises the model.
        /// This implicitly sets the objective sense to Minimise.
        /// The method uses branch and bound if model is MIP and LP otherwise.
        /// </summary>
        /// <param name="forceRelaxation">Force solving of the LP relaxation.</param>
        public void Minimise(bool forceRelaxation = false)
        {
            ApplyObjectiveSense(ObjectiveSense.Minimise);
            if (!Generated) Solve(forceRelaxation);
            else Resolve(forceRelaxation);
        }

        /// <summary>
        /// Solve the model according to the ObjectiveSense settings.
        /// The method uses branch and bound if model is MIP and LP otherwise.
        /// </summary>
        /// <param name="forceRelaxation">Force solving of the LP relaxation.</param>
        public void Solve(bool forceRelaxation = false)
        {
            Solve(false, forceRelaxation);
        }

        /// <summary>
        /// Resolve the model according to the ObjectiveSense settings.
        /// The method uses branch and bound if model is MIP and LP otherwise.
        /// </summary>
        /// <param name="forceRelaxation">Force solving of the LP relaxation.</param>
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

                    if (solver is OsiCbcSolverInterface)
                    {
                        OsiCbcSolverInterface cbcSolver = (OsiCbcSolverInterface)solver;
                        if (cbcSolver.UseBranchAndBound())
                        {
                            cbcSolver.branchAndBound();
                        }
                        else
                        {
                            string[] cbcMainArgs = cbcSolver.GetCbcSolverArgs();
                            List<string> args = new List<string>();
                            args.Add("Sonnet");
                            args.AddRange(cbcMainArgs);
                            args.Add("-solve");
                            args.Add("-quit");

                            CbcSolver.CbcMain0(cbcSolver.getModelPtr());
                            CbcSolver.CbcMain1(args.ToArray(), cbcSolver.getModelPtr());
                        }
                    }
                    else
                    {
                        solver.branchAndBound();
                    }


                    AssignSolution(true);
                    if (AutoResetMIPSolve) ResetAfterMIPSolveInternal(); // mainly to reset bounds etc, but use AssignSolutionStatus because the Reset messes up the IsProvenOptimal etc!
                }
                else
                {
                    isSolving = true;
                    if (doResolve) solver.resolve();
                    else solver.initialSolve();
                    AssignSolution(false);
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

        /// <summary>
        /// Determine whether the current solution satisfies all constraints and variables bounds and types.
        /// </summary>
        /// <returns>True iff the current solution is feasible.</returns>
        public bool IsFeasible()
        {
            Generate();

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
            log.Debug(tmp.ToString());

            return feasible;
        }

        #region Get / Set WarmStart methods
        ///<summary>
        /// Gets the warm start object for the current problem
        ///</summary>
        public WarmStart GetWarmStart()
        {
            Generate();
            return WarmStart.NewWarmStart(solver);
        }

        ///<summary>
        /// Gets an empty warm start object for the current problem
        ///</summary>
        public WarmStart GetEmptyWarmStart()
        {
            Generate();
            return WarmStart.NewEmptyWarmStart(solver);
        }

        ///<summary>
        /// Set (apply) the given warm start object to the current problem
        ///</summary>
        public void SetWarmStart(WarmStart warmStart)
        {
            Ensure.NotNull(warmStart, "WarmStart");

            Generate();
            warmStart.ApplyWarmStart(solver);
        }
        #endregion;

        #region Reset / Save for MIP Solver methods
        /// <summary>
        /// Reset the bounds etc after a MIP solve (branch and bound).
        /// This is done automatically according to the AutoResetMIPSolve property.
        /// Note that this also reselt the results of solver.get.. functions.
        /// </summary>
        public void ResetAfterMIPSolve()
        {
            if (AutoResetMIPSolve)
            {
                throw new NotSupportedException("ResetAfterMIPSolve is automatically called");
            }

            Generate();
            ResetAfterMIPSolveInternal();
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

        /// <summary>
        /// Save the bounds etc before a MIP solve (branch and bound)
        /// This is done automatically according to the AutoResetMIPSolve property.
        /// </summary>
        public void SaveBeforeMIPSolve()
        {
            if (AutoResetMIPSolve)
            {
                throw new NotSupportedException("SaveBeforeMIPSolve is automatically called");
            }

            Generate();

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
        /// <param name="con">The constraint to generate.</param>
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
                    if (voffset == -1) throw new SonnetException("Trying to use variable that is not part of this model!");

                    columns[numberElements] = voffset;
                    element[numberElements] = c.coef;
                }

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
        /// <param name="obj">The objective to generate.</param>
        private void Generate(Objective obj)
        {
            Ensure.NotNull(obj, "objective");

            //this.objective.Unregister(this); // this should have already been done!

            // Registering the objective is important for changing coefficients: If we change coefs of an objective,
            // these changes have to be passed on to all registered models.
            obj.Assemble();
            obj.Register(this);
            obj.Assign(this, 0.0); // immediately also STORE!

            GenerateVariables(obj.Coefficients);

            if (Generated)
            {
                // now load the objective into the solver
                int n = variables.Count;	// the NEW number of variables
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

                // change all coefficients at the same time
                solver.setObjective(c);

                // Skip this: doesnt work as expected with max/min problems
                // The constant part goes in via the ObjOffset
                // solver.setDblParam(OsiObjOffset, obj->Constant);
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
        /// Generates (builds) the model to be solved. 
        /// This can be called explicitly, but is done automatically within a solve.
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
                    foreach (Constraint con in rawconstraints)
                    {
                        try
                        {
                            Generate(con);
                        }
                        catch (System.Exception e)
                        {
                            string message = string.Format("Error generating constraint {0}.", con.Name);
                            throw new SonnetException(message, e);
                        }
                    }

                    rawconstraints.Clear();
                }
                return;
            }
            #endregion

            #region Not yet generated
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

            foreach (Constraint con in rawconstraints)
            {
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
            }
            rawconstraints.Clear();

#if (SONNET_USE_SEMICONTVAR)
		    // now that all the regular constraints and variables are registered,
		    // register the additional constraints and helper variables for any semi-continuous variables
            foreach (Variable var in variables.Where(v => v is SemiContiniuousVariable))
            {
                SemiContinuousVariable scvar = (SemiContinuousVariable)var;
                double sclower = scvar.SemiContinuousLower;
                double upper = scvar.Upper;
                string name = scvar.Name + "SCHelper";

                Variable scvarHelper = new Variable(name, 0.0, 1.0, VariableType.Integer);

                Constraint con1 = new Constraint(name + "Con1", 1.0 * scvar, ConstraintType.LE, upper * scvarHelper);
                try
                {
                    Generate(con1);
                }
                catch (Exception ex)
                {
                    string message = string.Format("Error generating constraint {0}.", con1.Name);
                    throw new SonnetException(message, ex);
                }
                nz += con1.Coefficients.Count;

                Constraint con2 = new Constraint(name + "Con2", sclower * scvarHelper, ConstraintType.LE, 1.0 * scvar);
                try
                {
                    Generate(con2);
                }
                catch (Exception ex)
                {
                    string message = string.Format("Error generating constraint {0}.", con2.Name);
                    throw new SonnetException(message, ex);
                }
                nz += con2.Coefficients.Count;
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
                foreach (Constraint con in constraints)
                {
                    CoefVector coefs = con.Coefficients;
                    //double rhs = con.RhsConstant;

                    foreach (Coef coef in coefs)
                    {
                        int col = Offset(coef.var);
                        
                        if (coef.var.AssignedSolver != this) throw new SonnetException("Trying to use variable that is not part of this model!");
                        if (col < 0 || col >= n) throw new SonnetException("Variable offset has error value.");

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
                foreach (Constraint con in constraints)
                {
                    CoefVector coefs = con.Coefficients;
                    //double rhs = con.RhsConstant;
                    int row = Offset(con);

                    int ki = 0;
                    foreach (Coef coef in coefs)
                    {
                        int col = Offset(coef.var);

                        if (col < 0 || col >= n) throw new SonnetException("Variable offset has error value.");

                        Elm[Cst[col] + Clg[col]] = coef.coef;
                        Rnr[Cst[col] + Clg[col]] = row;
                        Clg[col]++;

                        ki++;
                    }
                }

                // Row bounds
                foreach (Constraint con in constraints)
                {
                    int row = Offset(con);
                    bu[row] = con.Upper;
                    bl[row] = con.Lower;

                    //double rhs = con.RhsConstant;
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

                log.DebugFormat("Ready to load the problem after {0}", (CoinUtils.CoinCpuTime() - genStart));

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
                    log.Debug("Using new CLP-specific matrix loading!");

                    OsiClpSolverInterface osiClp = (OsiClpSolverInterface)solver;
                    osiClp.LeanLoadProblem(n, m, nz, &Cst, &Clg, &Rnr, &Elm, &l, &u, &c, &bl, &bu);
                }
                else
#endif
                {
                    solver.loadProblemUnsafe(n, m, Cst, Rnr, Elm, l, u, c, bl, bu);

                    CoinUtils.DeleteArray(Elm);
                    CoinUtils.DeleteArray(Rnr);
                    CoinUtils.DeleteArray(Cst);
                    CoinUtils.DeleteArray(Clg);
                    CoinUtils.DeleteArray(c);
                    CoinUtils.DeleteArray(l);
                    CoinUtils.DeleteArray(u);
                    CoinUtils.DeleteArray(bl);
                    CoinUtils.DeleteArray(bu);
                }

                // Skip this: doesnt work as expected with max/min problems
                // the constant part goes in via the ObjOffset
                // solver.setDblParam(OsiObjOffset, cOffset);
            } // end unsafe

            log.DebugFormat("Problem fully loaded after {0}", (CoinUtils.CoinCpuTime() - genStart));

            foreach (Variable var in variables)
            {
                if (var.Type == VariableType.Integer)
                {
                    int col = Offset(var); //j.second;
                    solver.setInteger(col);
                }
            }

            foreach (Constraint con in constraints)
            {
                if (!con.Enabled)
                {
                    this.SetConstraintEnabled(con, false);
                }
            }

            StringBuilder tmp = new StringBuilder();
            tmp.AppendLine("Number of variables  : " + this.variables.Count);
            tmp.AppendLine("Number of constraints: " + this.constraints.Count);
            tmp.AppendLine("Number of elements   : " + solver.getNumElements());
            log.Debug(tmp.ToString()); // dont use ToStatisticsString() because that calls Generate..

            //static_cast<ModelEntity *>(objective).Unassign();

            // Full Names
            if (NameDiscipline > 0)
            {
                solver.setObjName(model.Objective.Name);

                foreach (Constraint con in constraints)
                {
                    int offset = Offset(con);
                    solver.setRowName(offset, con.Name);
                }
                log.DebugFormat("Done naming constraints after {0}", (CoinUtils.CoinCpuTime() - genStart));

                foreach (Variable var in variables)
                {
                    int offset = Offset(var);
                    solver.setColName(offset, var.Name);
                }

                log.DebugFormat("Done naming variables after {0}", (CoinUtils.CoinCpuTime() - genStart));
            }

            // Dump Hint Settings
            StringBuilder hintsMessage = new StringBuilder();
            foreach (OsiHintParam hintParam in Enum.GetValues(typeof(OsiHintParam)))
            {
                if (hintParam == OsiHintParam.OsiLastHintParam) continue;

                bool yesNo;
                OsiHintStrength hintStrength;

                solver.getHintParam(hintParam, out yesNo, out hintStrength);
                hintsMessage.AppendFormat("Hint: {0} : {1} at {2}\n", hintParam, yesNo, hintStrength);
            }
            if (hintsMessage.Length > 0) log.Debug(hintsMessage.ToString());

            System.GC.Collect();
            generated = true;

            //if (ProblemType == ProblemType.MILP) SaveBeforeMIPSolve();
            #endregion
        }

        /// <summary>
        /// Ungenerate the model.
        /// </summary>
        public void UnGenerate()
        {
            if (Generated)
            {
                generated = false;

                if (object.ReferenceEquals(null, objective)) throw new NullReferenceException("Ungenerate: A generated model must have a valid objective function");
                objective.Unregister(this);

                foreach (Constraint con in constraints)
                {
                    con.Unregister(this);
                    rawconstraints.Add(con);
                }
                
                // empty constraints
                constraints.Clear();

                foreach (Variable var in variables)
                {
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
        /// <param name="filename">The full file name to be used to export the model.</param>
        public void Export(string filename)
        {
            // All Public methods should call Generate to ensure any new constaints were added properly.
            Generate();

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
        /// <summary>
        /// Returns a string that represents the current solver.
        /// Similar to model.ToString().
        /// </summary>
        /// <returns>String representation of the current solver and model.</returns>
        public override string ToString()
        {
            // All Public methods should call Generate to ensure any new constaints were added properly.
            Generate();

            StringBuilder tmp = new StringBuilder();
            tmp.AppendLine(string.Format("Solver '{0}'", Name));
            tmp.Append(model.ToString(variables));
            return tmp.ToString();
        }

        /// <summary>
        /// Returns a string that represents the current solution of this solver.
        /// </summary>
        /// <returns>String representation of the current solution.</returns>
        public string ToSolutionString()
        {
            // All Public methods should call Generate to ensure any new constaints were added properly.
            Generate();

            StringBuilder tmp = new StringBuilder();
            tmp.AppendLine(string.Format("Solver '{0}'", Name));
            tmp.AppendLine(string.Format("Model '{0}'", model.Name));
            tmp.AppendLine(string.Concat("Model status: ", ((this.IsProvenOptimal) ? ("Optimal") : "not Optimal")));

            tmp.AppendLine("Objective: " + objective.Level());

            tmp.AppendLine("Variables:");
            foreach(Variable var in variables)
            {
                tmp.AppendLine(var.ToLevelString());
            }

            tmp.AppendLine("Constraints:");
            foreach(Constraint con in constraints)
            {
                tmp.AppendLine(con.ToLevelString());
            }

            tmp.AppendLine("End");
            return tmp.ToString();
        }
        
        /// <summary>
        /// Returns a string the contains statitics for the current model and solver.
        /// This includes number of variables, constraints, etc.
        /// </summary>
        /// <returns>String representation of statistics of the current solver.</returns>
        public string ToStatisticsString()
        {
            Generate();

            StringBuilder tmp = new StringBuilder();
            tmp.AppendLine("Statistics");
            tmp.AppendLine(string.Format("Solver '{0}'", Name));
            tmp.AppendLine(string.Format("Model '{0}'", model.Name));
  
            tmp.AppendLine(" Number of variables  : " + this.variables.Count);
            tmp.AppendLine(" Number of constraints: " + this.constraints.Count);
            tmp.AppendLine(" Number of elements   : " + solver.getNumElements());

            return tmp.ToString();
        }
        #endregion

        #region IsRegistered/Contains, for Tests
        /// <summary>
        /// Determine whether the given objective was registered 
        /// </summary>
        /// <param name="obj">The objective</param>
        /// <returns>True iff the given objective equals the current objective.</returns>
        internal bool IsRegistered(Objective obj)
        {
            Ensure.NotNull(obj, "objective");

            return this.objective.Equals(obj);
        }
        internal bool IsRegistered(Variable v)
        {
            Ensure.NotNull(v, "variable");

            foreach (Variable variable in variables)
            {
                if (object.Equals(variable, v)) return true;
            }

            return false;
        }
        /// <summary>
        /// For Testing only: Is this Constraint registered? Only generated constraints are generated?
        /// Not-yet generated constraints are not registered, since changes in the constraints do not need to be passed
        /// into the COIN solver yet.
        /// </summary>
        /// <param name="c">The constraint.</param>
        /// <returns>True iff one of the constraints Equals the given constraint.</returns>
        internal bool IsRegistered(Constraint c)
        {
            Ensure.NotNull(c, "constraint");

            foreach (Constraint constraint in constraints)
            {
                if (object.Equals(constraint, c)) return true;
            }

            return false;
        }
        /// <summary>
        /// For Testing only: IsRegistered OR in the (to-be) added list
        /// </summary>
        /// <param name="c">The constraint.</param>
        /// <returns>True iff the given constraint is registered, or Equals one of the raw constraints (not yet generated).</returns>
        internal bool Contains(Constraint c)
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

        //internal bool Contains(Variable v)
        //{
        //    //IsRegistered OR in the (to-be) added list 
        //    //DONT do this, since we'd have to check all the new constraints..
        //}

        #endregion


        /// <summary>
        /// Get the element index (offset) of the constraint in this model. An exception is thrown if no offset found.
        /// </summary>
        /// <param name="con">The constraint.</param>
        /// <returns>The integer offset of the given constraint in the current solver.</returns>
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
        /// <param name="var">The variable.</param>
        /// <returns>The integer offset of the given variable in the current solver.</returns>
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


        #region Obsolete/Deprecated methods
        // Using the Solver to enumerate the variables/constraints is not in line with the design.
        // These methods that expose the underlying solver are against the idea that the Solver should only
        // be used for solving, not retrieving stats of the Model
        // Also, in practise, enumerating variables (and even constraints) is not necessary. The user will keep these himself. 
        // Therefore, this is not (longer) supported.

        /// <summary>
        /// Gets the number of generated constraints.
        /// </summary>
        [Obsolete("Deprecated: use property of Model", true)]
        public int NumberOfConstraints 
        { 
            get 
            {
                Generate();
                return solver.getNumRows(); 
            } 
        }
        
        /// <summary>
        /// Gets the number of generated variables.
        /// </summary>
        [Obsolete("Deprecated: Not supported. Use property of OsiSolver", true)]
        public int NumberOfVariables 
        { 
            get 
            {
                Generate();
                return solver.getNumCols(); 
            } 
        }
        
        /// <summary>
        /// Gets the number of elements in the constraint matrix
        /// </summary>
        [Obsolete("Deprecated: Not supported. Use property of OsiSolver", true)]
        public int NumberOfElements 
        { 
            get 
            {
                Generate();
                return solver.getNumElements(); 
            } 
        }

        /// <summary>
        /// Gets the number of integer variables
        /// </summary>
        [Obsolete("Deprecated: Not supported. Use property of OsiSolver", true)]
        public int NumberOfIntegerVariables 
        { 
            get 
            {
                Generate();
                return solver.getNumIntegers(); 
            } 
        }
        
        /// <summary>
        /// Gets the generated constraints
        /// </summary>
        [Obsolete("Deprecated: use property of Model", true)]
        private IEnumerable<Constraint> Constraints
        {
            get { return constraints; }
        }

        /// <summary>
        /// Return the constraint, given its name in this model
        /// </summary>
        /// <param name="name">The name of the constraint to get.</param>
        /// <returns>The first constraint with Name that Equals the given name, or null if not found.</returns>
        [Obsolete("Deprecated: use property of Model", true)]
        private Constraint GetConstraint(string name)
        {
            foreach (Constraint con in constraints) if (con.Name.Equals(name)) return con;
            foreach (Constraint con in rawconstraints) if (con.Name.Equals(name)) return con;
            return null;
        }

        /// <summary>
        /// Gets the variables currenlty generated
        /// </summary>
        [Obsolete("Deprecated: Enumerating variables not supported.", true)]
        private IEnumerable<Variable> Variables
        {
            // we dont (bother to) store the constraint and variable names in a Map, so we just search
            // NOTE: since we dont EXplicitly add variables to any model, but rather this is done
            // implicitly when the constraints are generated, this means that we cannot find a variable by name
            // until the model is generated, and thus the list "variables" is built.
            // Of course, we could enforce that all variables be explicitly add, just like constraints, but 
            // I dont think this will be necessary.
            // Alternatively, we could also find the variable by going passed all the variables in the constraints....
            get { return variables; }
        }

        /// <summary>
        /// return the variable, given its element index in this model
        /// </summary>
        /// <param name="name">The name of the variable to get.</param>
        /// <returns>The first variable with Name that Equals the given name, or null if not found.</returns>
        [Obsolete("Deprecated: Enumerating variables not supported.", true)]
        private Variable GetVariable(string name)
        {
            foreach (Variable var in variables) if (var.Name.Equals(name)) return var;
            return null;
        }

        /// <summary>
        /// Deprecated: use property of Variable
        /// Looks up the offset, and retrieves value.
        /// </summary>
        /// <param name="v">The variable to return the value for.</param>
        /// <returns>The value of the variable in the current solution at the solver.</returns>
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
        /// Looks up the offset, and retrieves the reduced cost.
        /// </summary>
        /// <param name="v">The variable to return the reduced cost for.</param>
        /// <returns>The reduced cost of the variable in the current solution at the solver.</returns>
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
        /// Looks up the offset, and retreives the price.
        /// </summary>
        /// <param name="c">The constraint to return the price for.</param>
        /// <returns>The price of the given constraint in the current solution at the solver.</returns>
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

        /// <summary>
        /// Get and store the solution status (optimal, etc), and the solution values for the variables and constraints from the solver.
        /// These values are subsequently stored at the variables and constraints for later retrieval via variable.Value etc.
        /// Some values, like row prices, are not available for MIP.
        /// </summary>
        /// <param name="mipSolve">Latest solve was mip solve</param>
        private void AssignSolution(bool mipSolve)
        {
            AssignSolutionStatus(mipSolve);
            AssignVariableSolution(mipSolve);
            AssignConstraintSolution(mipSolve);
        }

        /// <summary>
        /// Get and store the solution status values (isProvenOptimal etc)
        /// Iteration Count is not available for MIP.
        /// </summary>
        /// <param name="mipSolve">Latest solve was mip solve</param>
        private void AssignSolutionStatus(bool mipSolve)
        {
            isAbandoned = solver.isAbandoned();
            isProvenOptimal = solver.isProvenOptimal();
            isProvenPrimalInfeasible = solver.isProvenPrimalInfeasible();
            isProvenDualInfeasible = solver.isProvenDualInfeasible();
            isPrimalObjectiveLimitReached = solver.isPrimalObjectiveLimitReached();
            isDualObjectiveLimitReached = solver.isDualObjectiveLimitReached();
            isIterationLimitReached = solver.isIterationLimitReached();
            if (mipSolve) iterationCount = 0; // doesnt work for OsiCpx
            else iterationCount = solver.getIterationCount();
        }


        /// <summary>
        /// Get and store the primal solution values to the variables and reduced cost
        /// Reduced Cost of columns not available for MIP.
        /// /// </summary>
        /// <param name="mipSolve">Latest solve was mip solve</param>
        private void AssignVariableSolution(bool mipSolve)
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
                double* reducedCost = null;
                if (!mipSolve) reducedCost = solver.getReducedCostUnsafe();

                for (int col = 0; col < variables.Count; col++)
                {
                    Variable var = variables[col];

                    var.Assign(this, col, values[col], mipSolve?0.0:reducedCost[col]);
#if (DEBUG)
                    if (IsProvenOptimal)
                    {
                        if (!values[col].IsBetween(var.Lower, var.Upper))
                        {
                            log.DebugFormat("Solution is optimal, but variable value {0} is outside of bounds [{1},{2}] ", var, var.Lower, var.Upper);
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
                    log.DebugFormat("Solution is optimal, but objective value {0} is not corrected for constant term (offset) {1}", objective.Value, objective.Level());
                }
            }
#endif
        }

        /// <summary>
        /// get and store the values of the primal constraints in the dual solution
        /// get and propagate the values of the primal constraints in the dual solution
        /// and the total LHS( or "middle") value of the constraints in the current (primal) solution, with all var. moved left
        /// Prices or rows not available for MIP.
        /// </summary>
        /// <param name="mipSolve">Latest solve was mip solve</param>
        private void AssignConstraintSolution(bool mipSolve)
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
                double* values = solver.getRowActivityUnsafe();
                double* prices = null;
                if (!mipSolve) prices = solver.getRowPriceUnsafe();

                for (int row = 0; row < constraints.Count; row++)
                {
                    Constraint con = constraints[row];
                    con.Assign(this, row, mipSolve?0.0:prices[row], values[row]);
                }
            }
        }
        #endregion //Assign Solution methods
        /// <summary>
        /// Is the solver busy Solving?
        /// </summary>
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

            int offset = Offset(var);
            solver.setColUpper(offset, upper);
        }

        internal void SetVariableLower(Variable var, double lower)
        {
            Ensure.NotNull(var, "variable");

            int offset = Offset(var);
            solver.setColLower(offset, lower);
        }
        
        internal void SetVariableBounds(Variable var, double lower, double upper)
        {
            Ensure.NotNull(var, "variable");

            int offset = Offset(var);
            solver.setColBounds(offset, lower, upper);
        }

        internal void SetVariableType(Variable var, VariableType type)
        {
            Ensure.NotNull(var, "variable");

            int offset = Offset(var);
            if (type == VariableType.Continuous) solver.setContinuous(offset);
            else if (type == VariableType.Integer) solver.setInteger(offset);
        }

        /// <summary>
        /// Set the name of the given variable within the solver (only) 
        /// </summary>
        /// <param name="var">The variable to set the name for.</param>
        /// <param name="name">The new name to set for this variable within the solver.</param>
        internal void SetVariableName(Variable var, string name)
        {
            Ensure.NotNull(var, "variable");
            Ensure.NotNullOrWhiteSpace(name, "name");

            int offset = Offset(var);
            solver.setColName(offset, name);
        }

        /// <summary>
        /// methods for changing the objective function
        /// </summary>
        /// <param name="var">The variable to set the objective coefficient for.</param>
        /// <param name="value">The new coefficient to set for this variable within the solver.</param>
        internal void SetObjectiveCoefficient(Variable var, double value)
        {
            Ensure.NotNull(var, "variable");

            int offset = Offset(var);
            solver.setObjCoeff(offset, value);
        }
        
        // methods for changing Range Constraints
        internal void SetCoefficient(RangeConstraint con, Variable var, double value)
        {
            Ensure.NotNull(con, "range constraint");
            Ensure.NotNull(var, "variable");

            int conOffset = Offset(con);
            int varOffset = Offset(var);

            //solver.setCoef(conOffset, varOffset, value);
            if (solver is OsiClpSolverInterface)
            {
                OsiClpSolverInterface osiClp = (OsiClpSolverInterface)solver;
                osiClp.getModelPtr().modifyCoefficient(conOffset, varOffset, value);
                return;
            }

            if (solver is OsiCbcSolverInterface)
            {
                OsiCbcSolverInterface osiCbc = (OsiCbcSolverInterface)solver;
                OsiSolverInterface osiReal = osiCbc.getRealSolverPtr();
                if (osiReal is OsiClpSolverInterface)
                {
                    OsiClpSolverInterface osiClp = (OsiClpSolverInterface)osiReal;
                    osiClp.getModelPtr().modifyCoefficient(conOffset, varOffset, value);
                    return;
                }
                else
                {
                    //.. nothing really..
                    throw new NotImplementedException("SetCoefficient is not implemented for '" + solver.GetType().Name + "' type of solver with real solver '" + osiReal.GetType().Name + "'.");
                }
            }

            throw new NotImplementedException("SetCoefficient is not implemented for '" + solver.GetType().Name + "' type of solver.");
        }

        internal virtual void SetConstraintUpper(RangeConstraint con, double upper)
        {
            Ensure.NotNull(con, "range constraint");

            int offset = Offset(con);
            //if (SolverType == SolverType.CpxSolver) solver.setRowBounds(offset, con.Lower, upper);
            solver.setRowUpper(offset, upper);
        }
        internal void SetConstraintLower(RangeConstraint con, double lower)
        {
            Ensure.NotNull(con, "range constraint");

            int offset = Offset(con);
            //if (SolverType == SolverType.CpxSolver) solver.setRowBounds(offset, lower, con.Upper);
            solver.setRowLower(offset, lower);
        }
        internal void SetConstraintBounds(RangeConstraint con, double lower, double upper)
        {
            Ensure.NotNull(con, "range constraint");

            int offset = Offset(con);
            solver.setRowBounds(offset, lower, upper);
        }
        internal void SetConstraintEnabled(Constraint con, bool enable)
        {
            Ensure.NotNull(con, "range constraint");

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
            Ensure.NotNullOrWhiteSpace(name, "name");

            if (NameDiscipline == 0) return;

            int offset = Offset(con);
            solver.setRowName(offset, name);
        }
        #endregion

        private void GutsOfConstructor(Model model, OsiSolverInterface solver, string name)
        {
            Ensure.NotNull(model, "model");
            Ensure.NotNull(solver, "solver");
            
            id = numberOfSolvers++;
            log.Info(InternalUtils.GetAssemblyInfo());

            this.model = model;
            this.solver = solver;

            if (name != null) Name = name;
            else Name = string.Format("{0}_{1}", solver.GetType().FullName, id);

            generated = false;

            model.Register(this);

            objective = model.Objective;
            variables = new List<Variable>();
            rawconstraints = new List<Constraint>(model.Constraints);
            constraints = new List<Constraint>();

            log.PassToSolver(solver);
            ApplyObjectiveSense(model.ObjectiveSense);

            log.InfoFormat("Solver {0} created with model {1}", Name, model.Name);
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

        private OsiSolverInterface solver;
        private Model model;

        #region IDisposable Members
        // See http://msdn.microsoft.com/en-us/library/system.idisposable.aspx

        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">Whether this method has been called by user's code.</param>
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

        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </summary>
        ~Solver()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion

    }
}
