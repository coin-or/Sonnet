// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// The class RangeConstraint respresents a special type of constraints.
    /// By definition, a range constraint can be formulated as
    ///  lower &lt;= expression &lt;= upper
    /// where l and u are constants.
    /// RangeConstraints can change coefficients of individual variables.
    /// </summary>
    public class RangeConstraint : Constraint
    {
        /// <summary>
        /// Initializes a new instance of the RangeConstraint class based on a copy of the given constraint.
        /// A default name is used.
        /// </summary>
        /// <param name="rangeConstraint">The range constraint to be copied.</param>
        public RangeConstraint(RangeConstraint rangeConstraint)
            : base(null, rangeConstraint)
        {
            this.lower = rangeConstraint.lower;
        }

        /// <summary>
        /// Initializes a new instance of the RangeConstraint class based on a copy of the given constraint,
        /// but with the given name.
        /// </summary>
        /// <param name="name">The name for the new constraint.</param>
        /// <param name="rangeConstraint">The range constraint to be copied.</param>
        public RangeConstraint(string name, RangeConstraint rangeConstraint)
            : base(name, rangeConstraint)
        {
            this.lower = rangeConstraint.lower;
        }

        /// <summary>
        /// Initializes a new instance of the RangeConstraint class with the default name, 
        /// and of the form      lower &lt;= expr &lt;= upper
        /// The given expression is copied, and therefore are no longer needed after this constructor is called.
        /// </summary>
        /// <param name="lower">The left-hand side constant of the range constraint.</param>
        /// <param name="expr">The middle expression of the constraint.</param>
        /// <param name="upper">The right-hand side constant of the range consrtaint.</param>
        public RangeConstraint(double lower, Expression expr, double upper)
            : base(null, expr, ConstraintType.LE, new Expression(upper))
        {
            this.lower = lower;
        }

        /// <summary>
        /// Initializes a new instance of the RangeConstraint class with the given name, 
        /// and of the form      lower &lt;= expr &lt;= upper
        /// The given expression is copied, and therefore are no longer needed after this constructor is called.
        /// </summary>
        /// <param name="name">The name for the new constraint.</param>
        /// <param name="lower">The left-hand side constant of the range constraint.</param>
        /// <param name="expr">The middle expression of the constraint.</param>
        /// <param name="upper">The right-hand side constant of the range consrtaint.</param>
        public RangeConstraint(string name, double lower, Expression expr, double upper)
            : base(name, expr, ConstraintType.LE, new Expression(upper))
        {
            this.lower = lower;
        }

        /// <summary>
        /// Returns a System.String that represents the current RangeConstraint.
        /// </summary>
        /// <returns>A System.String that represents the current RangeConstraint.</returns>
        public override string ToString()
        {
            Ensure.IsTrue(Type == ConstraintType.LE, "Range constraints must be of type <= .");

            return string.Format("{0} : {1} <= {2} <= {3}", Name, Lower, expr.ToString(), Upper);
        }

        /// <summary>
        /// Returns a string that represents the value of this instance using the Level of the expressions.
        /// </summary>
        /// <returns>A string representation of this instance using the Level.</returns>
        public override string ToLevelString()
        {
            Ensure.IsTrue(Type == ConstraintType.LE, "Range constraints must be of type <= .");

            return string.Format("{0} : {1} <= {2} <= {3}  ( {4} )", Name, Lower, expr.Level(), Upper, Price);
        }

        /// <summary>
        /// Gets or sets the constant lower (left-hand-side) bound of this range constraint. 
        /// Note that range constaints can only be '&lt;=' constraints.
        /// </summary>
        public override double Lower
        {
            get { return this.lower; }
            set
            {
                this.lower = value;
                foreach (Solver solver in solvers) solver.SetConstraintLower(this, value);
            }
        }

        /// <summary>
        /// Gets or sets the constant upper (right-hand-side) bound of this range constraint.
        /// Note that range constaints can only be '&lt;=' constraints.
        /// </summary>
        public override double Upper
        {
            get { return rhs.Constant; }
            set
            {
                rhs.Assign(value);
                foreach (Solver solver in solvers) solver.SetConstraintUpper(this, value);
            }
        }

        /// <summary>
        /// Sets both the constant lower and upper bound of this range constraint.
        /// Note that range constaints can only be '&lt;=' constraints.
        /// </summary>
        /// <param name="lower">The lower bound.</param>
        /// <param name="upper">The upper bound.</param>
        public void SetBounds(double lower, double upper)
        {
            rhs.Assign(upper);
            this.lower = lower;

            foreach (Solver solver in solvers) solver.SetConstraintBounds(this, lower, upper);
        }

        /// <summary>
        /// Retrieve the overall coefficient of the given variable in the expression part of this range constraint.
        /// </summary>
        /// <param name="var">The variable for which to retrieve the coefficient.</param>
        /// <returns>The overall coefficient of the given variable.</returns>
        public double GetCoefficient(Variable var)
        {
            return expr.GetCoefficient(var);
        }

        /// <summary>
        /// Change the coefficient of variable var to value. Returns the old coefficient of this variable.
        /// </summary>
        /// <param name="var">The variable for which to set the coefficient.</param>
        /// <param name="value">The new value</param>
        /// <returns>The old value</returns>
        public double SetCoefficient(Variable var, double value)
        {
            Ensure.NotNull(var, "variable");

            double oldValue = expr.SetCoefficient(var, value);
            foreach (Solver solver in solvers) solver.SetCoefficient(this, var, value);

            return oldValue;
        }

        /// <summary>
        /// Determines the slack compared to the upper bound (only) of this range constraint in the current solution. 
        /// The slack should be non-negative in a feasible solution.
        /// </summary>
        /// <returns>The slack compared to the upper bound of this constraint in the current solution.</returns>
        public override double Slack() 
        {
            return rhs.Level() - Level();
        }

        internal override void Assemble()
        {
            // for RangeConstraints, the rhs (upper) and lhs (lower) are always constant.
            double constant = expr.Constant;
            if (constant != 0.0)
            {
                expr.Subtract(constant);
                // if the rhs->Constant is not already Infinity, then subtract the constant
                if (rhs.Constant < MathUtils.Infinity) rhs.Subtract(constant);
                // if the lower (lhs) is not already minus Infinity, then subtract the constant
                if (lower > -MathUtils.Infinity) lower -= constant;
            }
            expr.Assemble();
        }

        private double lower;
    }
}
