// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// Specifies the types of objective sense: Maximise or Minimise.
    /// </summary>
    public enum ObjectiveSense
    {
        /// <summary>
        /// Maximise the value of the given objective function.
        /// </summary>
        Maximise = -1,
        /// <summary>
        /// Minimise the value of the given objective function.
        /// </summary>
        Minimise = 1
    }

    /// <summary>
    /// The Objective class is an expression that can be assigned to one or more 
    /// models. 
    /// The expression is copied, and can not be modified, except by modifying individual coefficients.
    /// The Objective itself does not specify whether its value will be maximise or minimised.
    /// The direction of the optimisation is set at the Model.
    /// The constant in the given expression is taken into account.
    /// </summary>
    public class Objective : ModelEntity
    {
        /// <summary>
        /// Initializes a new instance of the Objective class with the given 
        /// name and an empty expression. 
        /// </summary>
        /// <param name="name">The given name.</param>
        public Objective(string name = null)
            : this(name, new Expression())
        {
        }

        /// <summary>
        /// Initializes a new instance of the Objective class
        /// that contains elements copied from the specified expression.
        /// </summary>
        /// <param name="expr">The expression whose elements are copied to the new objective.</param>
        public Objective(Expression expr)
            : this(null, expr)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Objective class with the given name
        /// that contains elements copied from the specified expression.
        /// </summary>
        /// <param name="name">The name for the new objective.</param>
        /// <param name="expr">The expression whose elements are copied to the new objective.</param>
        public Objective(string name, Expression expr)
            : base(name)
        {
            Ensure.NotNull(expr, "expression");

            GutsOfConstructor(name, expr);
        }

        /// <summary>
        /// Implicitly initializes a new instance of the Objective class
        /// that contains elements copied from the specified expression.
        /// For example: 
        /// Variable x = new Variable();
        /// Expression expression = 2 * x;
        /// Objective obj = expression;
        /// Objective obj2 = 3 * x;
        /// </summary>
        /// <param name="expr">The expression whose elements are copied to the new objective.</param>
        /// <returns>The new objective.</returns>
        public static implicit operator Objective(Expression expr)
        {
            return new Objective(expr);
        }

        /// <summary>
        /// Implicitly initializes a new instance of the Objective class
        /// that contains only the given variable.
        /// For example: 
        /// Variable x = new Variable();
        /// Objective obj = x;
        /// // Objective obj = 1.0 * x; // same thing.
        /// </summary>
        /// <param name="x">The variable that makes up the new objective.</param>
        /// <returns>The new objective.</returns>
        public static implicit operator Objective(Variable x)
        {
            return new Objective(1.0 * x);
        }


        /// <summary>
        /// Returns a copy of the expression of the given obj.
        /// Usage:
        /// Expression ex = 10.0 + (Expression)obj;
        /// </summary>
        /// <param name="obj">The objective to be used</param>
        public static explicit operator Expression(Objective obj)
        {
            return obj.Expression();
        }

        /// <summary>
        /// Returns a copy of the expression of this objective.
        /// We must not allow external access to the internal expression to prevent changes.
        /// Therefore, also no implicit conversion to internal expression.
        /// </summary>
        /// <returns>A new expression</returns>
        public Expression Expression()
        {
            return new Expression(this.expression);
        }

        /// <summary>
        /// Removes all elements from the Objective.
        /// </summary>
        public void Clear()
        {
            if (!(expression is null))
            {
                expression.Clear();
            }
        }

        /// <summary>
        /// Converts the value of this instance to a System.String.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            return string.Format("Objective {0} : {1}", Name, expression.ToString());
        }

        /// <summary>
        /// Converts the value of this instance to a System.String using the Level of the expression
        /// of this objective.
        /// </summary>
        /// <returns>A string representation of this instance using the Level.</returns>
        public virtual string ToLevelString()
        {
            return string.Format("Objective {0} : {1}", Name, expression.Level());
        }

        /// <summary>
        /// Gets the current Value of this objective.
        /// This value is set (AssignSolution) after the optimisation.
        /// </summary>
        public double Value
        {
            get { return value; }
        }

        /// <summary>
        /// Calculates the level of this objective, i.e. fill in the values of the variable in expression of this objective. This should be equal to the Value.
        /// See Level of the Expression class.
        /// </summary>
        /// <returns>The level.</returns>
        public double Level()
        {
            Ensure.IsTrue(Assigned, "No solver not set!");
            return expression.Level();
        }

        /// <summary>
        /// Gets the current Bound of this objective for MIP.
        /// If the current solution is Optimal, then Objective Bound equals Objective Value.
        /// If not yet optimal, then Bound is best relaxation bound of all nodes left on the search tree.
        /// This value is set (AssignSolution) after the optimisation.
        /// This value is not available for all solvers.
        /// </summary>
        public double Bound 
        { 
            get { return bound; } 
        }

        /// <summary>
        /// Returns the coefficient of the given variable in this objective.
        /// </summary>
        /// <param name="var">The variable whose coefficient to return.</param>
        /// <returns>The coefficient of the given variable.</returns>
        public double GetCoefficient(Variable var)
        {
            return expression.GetCoefficient(var);
        }

        /// <summary>
        /// Sets the coefficients of the given variable in this objective.
        /// All solvers that use this objective are updated.
        /// </summary>
        /// <param name="var">The variable whose coefficient is to be set.</param>
        /// <param name="coef">The new coefficient.</param>
        /// <returns>The previous coefficients of the given variable in this objective.</returns>
        public double SetCoefficient(Variable var, double coef)
        {
            double oldValue = expression.SetCoefficient(var, coef);
            foreach (Solver solver in solvers) solver.SetObjectiveCoefficient(var, coef);
            return oldValue;
        }

        /// <summary>
        /// Assemble this objective by assembling its expression.
        /// </summary>
        internal void Assemble()
        {
            expression.Assemble();
        }
        
        /// <summary>
        /// Assigns the given solver, and set the Value and Bound of this objective.
        /// </summary>
        /// <param name="solver">The solver to be assigned.</param>
        /// <param name="value">The new Value of this objective.</param>
        /// <param name="bound">The new Bound of this objective.</param>
        internal void Assign(Solver solver, double value, double bound)
        {
            base.Assign(solver, -1);
            this.value = value;
            this.bound = bound;
        }

        /// <summary>
        /// Returns the coefficients of (the expression of) this objective.
        /// </summary>
        internal CoefVector Coefficients
        {
            get { return expression.Coefficients; }
        }

        /// <summary>
        /// Returns the quadratic coefficients of (the expression of) this objective
        /// </summary>
        internal QuadCoefVector QuadCoefficients
        {
            get { return expression.QuadCoefficients; }
        }

        /// <summary>
        /// Returns the constant of (the expression of) this objective.
        /// </summary>
        internal double Constant
        {
            get { return expression.Constant; }
        }

        /// <summary>
        /// Returns whether or not (the expression of) this objective has a quadratic term.
        /// </summary>
        public bool IsQuadratic
        {
            get { return expression.IsQuadratic; }
        }

        /// <summary>
        /// Initializes this objective.
        /// </summary>
        /// <param name="name">The name to be used for this objective.</param>
        /// <param name="expr">The expression to be used for this objective.</param>
        private void GutsOfConstructor(string name, Expression expr)
        {
            this.id = NextId();

            if (!(expr is null)) this.expression = new Expression(expr);
            else this.expression = new Expression();

            if (name != null) Name = name;
            else Name = string.Format("Obj_{0}", id);
        }

        /// <summary>
        /// Used for assigning this.id as numberOf++
        /// </summary>
        /// <returns>numberOf++</returns>
        private static int NextId()
        {
            return numberOfObjectives++;
        }

        private static int numberOfObjectives = 0;
        private Expression expression;
        private double value;
        private double bound;
    }
}
