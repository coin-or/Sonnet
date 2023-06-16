// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License v2.0 (EPL-2.0).

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using System.IO;
using System.Reflection;

using COIN;

namespace Sonnet
{
    /// <summary>
    /// The Model class holds the objective and a collection of constraints.
    /// Unlike constraints, the variables are not explicitly added to the model.
    /// Rather, the variables of a model are only implied by variables use within the objective and constraints.
    /// This is also the reason why export functionality of the model is limited.
    /// </summary>
    public class Model : Named
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Model class with the given name.
        /// </summary>
        /// <param name="name">The name of this model.</param>
        public Model(string name = null)
            : base(name)
        {
            GutsOfConstructor(name);
        }
        #endregion

        /// <summary>
        /// Clears the objective and all constraints of this model.
        /// </summary>
        public void Clear()
        {
            if (!object.ReferenceEquals(objective, null)) objective.Clear();
            objective = new Objective("obj");

            if (!object.ReferenceEquals(constraints, null)) constraints.Clear();
        }

        #region ToString() methods
        /// <summary>
        /// Returns a System.String that represents the current Model, given the set of variables,
        /// This includes the name of the model, all variables, the objective and all constraints.
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>A string that represents the current Model.</returns>
        internal string ToString(IEnumerable<Variable> variables)
        {
            StringBuilder tmp = new StringBuilder();
            tmp.AppendLine("Model '" + Name + "'");

            foreach (Variable variable in variables)
            {
                tmp.AppendLine(variable.ToString());
            }

            tmp.Append("Objective: ");
            if (object.ReferenceEquals(null, objective))
            {
                tmp.AppendLine("No objective defined");
            }
            else
            {
                tmp.AppendLine(objective.ToString());
            }

            if (constraints.Count > 0)
            {
                tmp.AppendLine("Constraints:");
                foreach (Constraint constraint in Constraints)
                {
                    tmp.AppendLine(constraint.ToString());
                }
            }

            if (constraints.Count == 0)
            {
                tmp.AppendLine("Model does not contain any constraints!");
            }

            return tmp.ToString();
        }

        /// <summary>
        /// Returns a System.String that represents the current Model.
        /// This includes the name of the model, all variables, the objective and all constraints.
        /// Note, this method iterates over all constraints (!) to build the set of variables in use.
        /// </summary>
        /// <returns>A string that represents the current Model.</returns>
        public override string ToString()
        {
            return ToString(GetVariables());
        }
        #endregion

        #region Objective Property
        /// <summary>
        /// Gets or sets the objective of this model.
        /// Solvers are updated accordingly.
        /// Objective must be not-null.
        /// </summary>
        public Objective Objective
        {
            get { return objective; }
            set { ApplyObjective(value); }
        }

        private void ApplyObjective(Objective objective)
        {
            Ensure.NotNull(objective, "objective");

            // REGISTER WHEN GENERATING (ONLY)!
            this.objective = objective;
            foreach (Solver solver in solvers) solver.ApplyObjective(objective);
        }
        #endregion

        #region ObjectiveSense Property
        /// <summary>
        /// Gets or sets the objective sense (Max/Min) of this model.
        /// Solvers are updated accordingly.
        /// </summary>
        public ObjectiveSense ObjectiveSense
        {
            get { return objectiveSense; }
            set { ApplyObjectiveSense(value); }
        }

        private void ApplyObjectiveSense(ObjectiveSense sense)
        {
            objectiveSense = sense;
            foreach (Solver solver in solvers) solver.ApplyObjectiveSense(sense);
        }

        #endregion

        #region Public propeties

        /// <summary>
        /// Get the Infinity value
        /// </summary>
        public double Infinity
        {
            get { return MathUtils.Infinity; }
        }

        /// <summary>
        /// Gets the constraints of this model.
        /// Use model.Add(..) to add constraints.
        /// </summary>
        public IEnumerable<Constraint> Constraints
        {
            get { return constraints; }
        }

        /// <summary>
        /// Gets the number of constraints of this model.
        /// </summary>
        public int NumberOfConstraints
        {
            get { return constraints.Count; }
        }

        #endregion

        #region Add constraints methods
        /// <summary>
        /// Adds a reference to the given constraint to this model, and sets its name.
        /// </summary>
        /// <param name="con">The constraint to be added.</param>
        /// <param name="name">The name of the constraint within this model. Null existing con name.</param>
        /// <returns>The given constraint.</returns>
        public Constraint Add(string name, Constraint con)
        {
            Ensure.NotNull(con, "con");
            if (!string.IsNullOrEmpty(name)) con.Name = name;
            constraints.Add(con);

            foreach (Solver solver in solvers) solver.Add(con);
            return con;
        }

        /// <summary>
        /// Adds a reference to the given constraint to this model.
        /// </summary>
        /// <param name="con">The constraint to be added.</param>
        /// <returns>The given constraint.</returns>
        public Constraint Add(Constraint con)
        {
            return Add(null, con);
        }

        /// <summary>
        /// Adds references to the given constraints to this model.
        /// </summary>
        /// <param name="constraints">The constraints to be added.</param>
        public void Add(IEnumerable<Constraint> constraints)
        {
            Ensure.NotNull(constraints, "constraints");

            foreach (Constraint constraint in constraints) Add(constraint);
        }

        /// <summary>
        /// Determine whether this constraint was added to this model.
        /// </summary>
        /// <param name="c">The constraint to look for.</param>
        /// <returns>True iff this constraint is part of this model.</returns>
        public bool Contains(Constraint c)
        {
            // otherwise, check the constraints still to be added.
            foreach (Constraint constraint in constraints)
            {
                if (object.ReferenceEquals(constraint, c)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the first constraint with a Name that Equals the given name.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <returns>The first constraint with a Name that Equals the given name, or null if not found.</returns>
        public Constraint GetConstraint(string name)
        {
            return constraints.Find(con => con.Name.Equals(name));
        }
        #endregion

        #region Static Properties
        /// <summary>
        /// Gets the Version number of this assembly
        /// </summary>
        public static Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }
        #endregion

        #region New and Export methods

        //Let's move this to Solver, or move it out all together.
        // Like, Model.FromFile(..)
        
        /// <summary>
        /// Creates a new model from the given file.
        /// Supported file extensions: .mps only.
        /// </summary>
        /// <param name="fileName">The mps file to be imported.</param>
        /// <returns>The new model, or null if an error occurred.</returns>
        public static Model New(string fileName)
        {
            Variable[] variables;
            return Model.New(fileName, out variables);
        }

        /// <summary>
        /// Creates a new model from the given file.
        /// Supported file extensions: .mps only.
        /// </summary>
        /// <param name="fileName">The mps file to be imported.</param>
        /// <param name="variables">The full array of variables created for this new model.</param>
        /// <returns>The new model, or null if an error occurred.</returns>
        public static Model New(string fileName, out Variable[] variables)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName).ToLower();

            Model model = null;
            if (extension.Equals(".mps"))
            {
                #region New Model from .mps file
                string directoryName = Path.GetDirectoryName(fileName);
                if (directoryName.Length == 0) directoryName = ".";

                string fullPathWithoutExtension = Path.Combine(directoryName, fileNameWithoutExtension);

                CoinMpsIO m = new CoinMpsIO();
                log.PassToCoinMpsIO(m);

                m.setInfinity(MathUtils.Infinity);
                //m.passInMessageHandler(modelPtr_.messageHandler());
                //*m.messagesPointer()=modelPtr_.coinMessages();

                int numberErrors = m.readMps(fullPathWithoutExtension);
                if (numberErrors != 0)
                {
                    string message = string.Format("Errors occurred when reading the mps file '{0}'.", fileName);
                    SonnetLog.Default.Error(message);
                    throw new SonnetException(message);
                }

                // set objective function offest
                // setDblParam(OsiObjOffset,m.objectiveOffset()); // WHAT IS THIS??

                model = NewHelper(out variables, m.isInteger, m.columnName, m.rowName,
                    m.getColLower(), m.getColUpper(), m.getObjectiveName(), m.getObjCoefficients(),
                    m.getNumCols(), m.getNumRows(), m.getRowSense(), m.getMatrixByRow(), m.getRowLower(), m.getRowUpper());
                
                model.Name = fileNameWithoutExtension;
                #endregion
            }
            else if (extension.Equals(".lp"))
            {
                #region New Model from .lp file
                CoinLpIO m = new CoinLpIO();
                log.PassToCoinLpIO(m);

                m.setInfinity(MathUtils.Infinity);
                // If the original problem is
                // a maximization problem, the objective function is immediadtly 
                // flipped to get a minimization problem.  
                m.readLp(fileName);
                
                model = NewHelper(out variables, m.isInteger, m.columnName, m.rowName, 
                    m.getColLower(), m.getColUpper(), m.getObjName(), m.getObjCoefficients(), 
                    m.getNumCols(), m.getNumRows(), m.getRowSense(), m.getMatrixByRow(), m.getRowLower(), m.getRowUpper());

                model.Name = fileNameWithoutExtension;
                #endregion
            }
            else
            {
                string message = string.Format("Cannot import file {0} : unknown extension '{1}'.", fileName, extension);
                SonnetLog.Default.Error(message);
                throw new SonnetException(message);
            }

            return model;
        }

        private static Model NewHelper(out Variable[] variables, Func<int, bool> isIntegerFunc, Func<int, string> columnNameFunc, Func<int, string> rowNameFunc,
            double[] colLower, double[] colUpper, string objName, double[] objCoefs, int numberVariables, int numberConstraints, char[] rowSenses, CoinPackedMatrix rowMatrix, double[] rowLowers, double[] rowUppers)
        {
            Model model = new Model();
            variables = new Variable[numberVariables];

            Expression objExpr = new Expression();

            int i = 0;
            for (i = 0; i < numberVariables; i++)
            {
                Variable var = new Variable();
                variables[i] = var;

                double lower = colLower[i];
                double upper = colUpper[i];
                bool isInteger = isIntegerFunc.Invoke(i);

                string name = columnNameFunc.Invoke(i);
                if (!string.IsNullOrEmpty(name)) var.Name = name;
                else var.Name = string.Concat("VAR", i);

                var.Lower = lower;
                var.Upper = upper;
                var.Type = (isInteger) ? VariableType.Integer : VariableType.Continuous;

                objExpr.Add(objCoefs[i], var);
            }

            model.Objective = new Objective(objName, objExpr);
            model.ObjectiveSense = ObjectiveSense.Minimise;
            // NOTE: MPS DOESNT STORE MAXIMIZATION OR MINIMIZATION!
            // bUT LP always returns Minimization (and transforms objective accordingly if original is max)

            for (int j = 0; j < numberConstraints; j++)
            {
                Expression expr = new Expression();
                CoinShallowPackedVector vector = rowMatrix.getVector(j); // I guess..
                int nElements = vector.getNumElements();
                int[] indices = vector.getIndices();
                double[] elements = vector.getElements();

                for (int e = 0; e < nElements; e++)
                {
                    int index = indices[e];
                    Variable var = variables[index];
                    double coef = elements[e];

                    expr.Add(coef, var);
                }

                double lower = rowLowers[j];
                double upper = rowUppers[j];

                string name = rowNameFunc.Invoke(j);
                string conName;
                if (!string.IsNullOrEmpty(name)) conName = name;
                else conName = string.Concat("CON", i);

                switch (rowSenses[j])
                {
                    case 'L': //<= constraint and rhs()[i] == rowupper()[i]
                        {
                            ConstraintType type = ConstraintType.LE;
                            Expression upperExpr = new Expression(upper);
                            Constraint con = new Constraint(conName, expr, type, upperExpr);
                            upperExpr.Clear();
                            model.Add(con);
                            break;
                        }
                    case 'E': //=  constraint
                        {
                            ConstraintType type = ConstraintType.EQ;
                            Expression upperExpr = new Expression(upper);
                            Constraint con = new Constraint(conName, expr, type, upperExpr);
                            upperExpr.Clear();
                            model.Add(con);
                            break;
                        }
                    case 'G': //>= constraint and rhs()[i] == rowlower()[i]
                        {
                            ConstraintType type = ConstraintType.GE;
                            Expression lowerExpr = new Expression(lower);
                            Constraint con = new Constraint(conName, expr, type, lowerExpr);
                            lowerExpr.Clear();
                            model.Add(con);
                            break;
                        }
                    case 'R': //ranged constraint
                        {
                            RangeConstraint con = new RangeConstraint(conName, lower, expr, upper);
                            model.Add(con);
                            break;
                        }
                    case 'N': //free constraint
                        {
                            RangeConstraint con = new RangeConstraint(conName, lower, expr, upper);
                            con.Enabled = false;
                            model.Add(con);
                            break;
                        }
                    default:
                        break;
                }
            }
            return model;
        }

        /// <summary>
        /// Exports this model to file.
        /// Support file extensions: .sonnet only.
        /// This method simply calls Model.ToString() and writes the output to file.
        /// To export to different formats, use the Solver.
        /// </summary>
        /// <param name="filename">The sonnet file to be exported to.</param>
        public void Export(string filename)
        {
            string directoryName = System.IO.Path.GetDirectoryName(filename);
            if (directoryName.Length == 0) directoryName = ".";

            string fullPathWithoutExtension = string.Concat(directoryName, System.IO.Path.DirectorySeparatorChar, System.IO.Path.GetFileNameWithoutExtension(filename));
            string extension = System.IO.Path.GetExtension(filename);

            if (extension.Equals(".sonnet"))
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
        #endregion

        #region Helper methods
        internal void Register(Solver solver)
        {
            solvers.Add(solver);
        }

        internal void Unregister(Solver solver)
        {
            solvers.Remove(solver);
        }

        //bool Contains(Variable v);		// IsRegistered OR in the (to-be) added list -> NOT do this, since we'd have to check all the new constraints 

        /// <summary>
        /// Generate a full list of all used variable from the objective and constraints.
        /// This is very expensive to call.
        /// </summary>
        /// <returns>The list of variables used in the objective and constraints.</returns>
        private IEnumerable<Variable> GetVariables()
        {
            Dictionary<int, Variable> variables = new Dictionary<int, Variable>();
            if (!object.ReferenceEquals(null, objective))
            {
                foreach (Coef c in objective.Coefficients) variables[c.var.id] = c.var;
            }

            foreach (Constraint constraint in Constraints)
            {
                foreach (Coef c in constraint.Coefficients) variables[c.var.id] = c.var;
                foreach (Coef c in constraint.RhsCoefficients) variables[c.var.id] = c.var;
            }

            return variables.Values;
        }

        private void GutsOfConstructor(string name)
        {
            id = numberOfModels++;

            objective = new Objective("obj");
            objectiveSense = ObjectiveSense.Minimise;
            constraints = new List<Constraint>();

            if (name != null) Name = name;
            else Name = string.Format("Model_{0}", id);
        }
        #endregion

        private static SonnetLog log = SonnetLog.Default;
        private static int numberOfModels = 0;

        private Objective objective;
        private ObjectiveSense objectiveSense;
        private List<Constraint> constraints;
        private List<Solver> solvers = new List<Solver>();
    }
}
