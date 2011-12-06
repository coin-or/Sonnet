// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

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
    public class Model : Named
    {
#if (DYNAMIC_LOADING)
        static Model()
        {
            // Addapted after http://scottbilas.com/blog/automatically-choose-32-or-64-bit-mixed-mode-dlls/

            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(Path.Combine(assemblyDir, "SonnetWrapper.proxy.dll"))
                || !File.Exists(Path.Combine(assemblyDir, "SonnetWrapper.x86.dll"))
                || !File.Exists(Path.Combine(assemblyDir, "SonnetWrapper.x64.dll")))
            {
                throw new InvalidOperationException("Found SonnetWrapper.proxy.dll which cannot exist. "
                    + "Must instead have SonnetWrapper.x86.dll and SonnetWrapper.x64.dll. Check your build settings.");
            }

            AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
            {
                if (e.Name.StartsWith("SonnetWrapper.proxy,", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = Path.Combine(assemblyDir,
                        string.Format("SonnetWrapper.{0}.dll", (IntPtr.Size == 4) ? "x86" : "x64"));
                    return Assembly.LoadFile(fileName);
                }
                return null;
            };
        }
#endif
        #region Constructors
        public Model()
            : this(string.Empty)
        {
        }

        public Model(string name)
            : base(name)
        {
            GutsOfConstructor();
        }
        #endregion

        public void Clear()
        {
            if (!object.ReferenceEquals(objective, null)) objective.Clear();
            objective = new Objective("obj");

            if (!object.ReferenceEquals(constraints, null)) constraints.Clear();
        }

        #region ToString() methods
        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();

            IEnumerable<Variable> variables = GetVariables();
            foreach (Variable variable in variables)
            {
                tmp.Append(variable.ToString());
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

            if (constraints.Count > 0)
            {
                tmp.Append("Constraints:\n");
                foreach (Constraint constraint in Constraints)
                {
                    tmp.Append(constraint.ToString());
                    tmp.Append("\n");
                }
            }

            if (constraints.Count == 0)
            {
                tmp.Append("Model does not contain any constraints!");
            }

            return tmp.ToString();
        }
        #endregion

        #region Objective Property
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
        /// Gets the constraints
        /// </summary>
        public IEnumerable<Constraint> Constraints
        {
            get { return constraints; }
        }

        /// <summary>
        /// Gets the number of constraints.
        /// </summary>
        public int NumberOfConstraints
        {
            get { return constraints.Count; }
        }
        
        #endregion

        #region Add constraints methods
        /// <summary>
        /// Adds a reference to the given constraint with the given name to this model.
        /// </summary>
        /// <param name="con">The constraint to be added.</param>
        /// <param name="name">The name of the constraint within this model.</param>
        /// <returns>The given constraint.</returns>
        public Constraint Add(Constraint con, string name = "")
        {
            Ensure.NotNull(con, "con");
            if (!string.IsNullOrEmpty(name)) con.Name = name;
            constraints.Add(con);

            foreach(Solver solver in solvers) solver.Add(con);
            return con;
        }

        /// <summary>
        /// Adds references to the given constraints to this model.
        /// </summary>
        /// <param name="constraints">The constraints to be added.</param>
        public void Add(System.Collections.IEnumerable constraints)
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
        /// return the constraint, given its element index in this model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        #region Import and Export methods
        /// <summary>
        /// Imports the model in the given file into this model.
        /// Only MPS files are supported.
        /// </summary>
        /// <param name="fileName">The mps file to be imported.</param>
        /// <returns>True if the file was successfully imported.</returns>
        public bool ImportModel(string fileName)
        {
            string directoryName = System.IO.Path.GetDirectoryName(fileName);
            if (directoryName.Length == 0) directoryName = ".";

            string fullPathWithoutExtension = string.Concat(directoryName, (System.IO.Path.DirectorySeparatorChar), System.IO.Path.GetFileNameWithoutExtension(fileName));
            string extension = System.IO.Path.GetExtension(fileName);
            if (extension.Equals(".mps"))
            {
                CoinMpsIO m = new CoinMpsIO();
                log.PassToCoinMpsIO(m);

                m.setInfinity(Infinity);
                //m.passInMessageHandler(modelPtr_.messageHandler());
                //*m.messagesPointer()=modelPtr_.coinMessages();

                int numberErrors = m.readMps(fullPathWithoutExtension);
                if (numberErrors != 0) return false;

                // set objective function offest
                // setDblParam(OsiObjOffset,m.objectiveOffset()); // WHAT IS THIS??

                double[] colLower = m.getColLower();
                double[] colUpper = m.getColUpper();
                string objName = m.getObjectiveName();
                double[] objCoefs = m.getObjCoefficients();
                Expression objExpr = new Expression();

                int numberVariables = m.getNumCols();

                Variable[] rawVariables = new Variable[numberVariables];
                int i = 0;
                for (i = 0; i < numberVariables; i++)
                {
                    Variable var = new Variable();
                    rawVariables[i] = var;

                    double lower = colLower[i];
                    double upper = colUpper[i];
                    bool isInteger = m.isInteger(i);

                    string name = m.columnName(i);
                    if (!string.IsNullOrEmpty(name)) var.Name = name;
                    else var.Name = string.Concat("VAR", i);

                    var.Lower = lower;
                    var.Upper = upper;
                    var.Type = (isInteger) ? VariableType.Integer : VariableType.Continuous;

                    objExpr.Add(objCoefs[i], var);
                }

                Objective objective = new Objective(objName, objExpr);
                Objective = objective;
                ObjectiveSense = ObjectiveSense.Minimise;
                // NOTE: MPS DOESNT STORE MAXIMIZATION OR MINIMIZATION!

                int numberConstraints = m.getNumRows();
                char[] rowSenses = m.getRowSense();
                CoinPackedMatrix rowMatrix = m.getMatrixByRow();
                double[] rowLowers = m.getRowLower();
                double[] rowUppers = m.getRowUpper();

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
                        Variable var = rawVariables[index];
                        double coef = elements[e];

                        expr.Add(coef, var);
                    }

                    double lower = rowLowers[j];
                    double upper = rowUppers[j];

                    string name = m.rowName(j);
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
                                this.Add(con);
                                break;
                            }
                        case 'E': //=  constraint
                            {
                                ConstraintType type = ConstraintType.EQ;
                                Expression upperExpr = new Expression(upper);
                                Constraint con = new Constraint(conName, expr, type, upperExpr);
                                upperExpr.Clear();
                                this.Add(con);
                                break;
                            }
                        case 'G': //>= constraint and rhs()[i] == rowlower()[i]
                            {
                                ConstraintType type = ConstraintType.GE;
                                Expression lowerExpr = new Expression(lower);
                                Constraint con = new Constraint(conName, expr, type, lowerExpr);
                                lowerExpr.Clear();
                                this.Add(con);
                                break;
                            }
                        case 'R': //ranged constraint
                            {
                                RangeConstraint con = new RangeConstraint(conName, lower, expr, upper);
                                this.Add(con);
                                break;
                            }
                        case 'N': //free constraint
                            {
                                RangeConstraint con = new RangeConstraint(conName, lower, expr, upper);
                                con.Enabled = false;
                                this.Add(con);
                                break;
                            }
                        default:
                            break;
                    }
                }

                return true;
            }
            else
            {
                throw new SonnetException(string.Concat("Cannot import file ", fileName, " : unknown extension '", extension, "'."));
            }
        }

        /// <summary>
        /// Exports the model in SONNET format.
        /// To export to different formats, use the Solver.
        /// </summary>
        /// <param name="filename">The sonnet file to be exported to.</param>
        public void ExportModel(string filename)
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


        private void GutsOfConstructor()
        {
            Utils.DumpAssemblyInfo();

            id = numberOfModels++;

            objective = new Objective("obj");
            objectiveSense = ObjectiveSense.Minimise;
            constraints = new List<Constraint>();
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
