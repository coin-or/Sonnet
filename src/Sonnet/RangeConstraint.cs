// Copyright (C) 2011, Jan-Willem Goossens 
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
    ///  l &lt;= expression &lt;= u
    /// where l and u are constants.
    /// RangeConstraints can change coefficients of individual variables.
    /// </summary>
    public class RangeConstraint : Constraint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="expr"></param>
        /// <param name="upper"></param>
        public RangeConstraint(double lower, Expression expr, double upper)
            : base(expr, ConstraintType.LE, new Expression(upper))
        {
            this.lower = lower;
            Name = string.Format("RangeCon[{0}]", id);
        }

        public RangeConstraint(string name, RangeConstraint rangeConstraint)
            : base(name, rangeConstraint)
        {
            this.lower = rangeConstraint.lower;
        }

        public RangeConstraint(string name, double lhs, Expression expr, double rhs)
            : base(name, expr, ConstraintType.LE, new Expression(rhs))
        {
            this.lower = lhs;
        }

        public override string ToString()
        {
            if (Type != ConstraintType.LE) throw new SonnetException("Range constraints must be of type <= .");

            return string.Format("{0} : {1} <= {2} <= {3}", Name, Lower, expr.ToString(), Upper);
        }

        public override string ToLevelString()
        {
            if (Type != ConstraintType.LE) throw new SonnetException("Range constraints must be of type <= .");

            return string.Format("{0} : {1} <= {2} <= {3}  ( {4} )", Name, Lower, expr.Level(), Upper, Price);
        }

        public override double Lower
        {
            get { return this.lower; }
            set
            {
                this.lower = value;
                foreach (Solver solver in solvers) solver.SetConstraintLower(this, value);
            }
        }

        public override double Upper
        {
            get { return rhs.Constant; }
            set
            {
                rhs.Assign(value);
                foreach (Solver solver in solvers) solver.SetConstraintUpper(this, value);
            }
        }

        public void SetBounds(double lower, double upper)
        {
            rhs.Assign(upper);
            this.lower = lower;

            foreach (Solver solver in solvers) solver.SetConstraintBounds(this, lower, upper);
        }

        public double GetCoefficient(Variable var)
        {
            return expr.GetCoefficient(var);
        }

#if (CONSTRAINT_SET_COEF)
        // Change the coefficient of variable var to value. Returns the old coefficient of this variable.
        public double SetCoefficient(Variable var, double value)
        {
            Ensure.NotNull(var, "variable");

            double oldValue = expr.SetCoefficient(var, value);
            foreach (Solver solver in solvers) solver.SetCoefficient(this, var, value);

            return oldValue;
        }
#endif
        public override double Slack() // for a feasible solution, the Slack is non-negative!
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
