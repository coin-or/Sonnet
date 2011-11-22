// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    public enum ConstraintType { LE, GE, EQ };

    public class Constraint : ModelEntity, IDisposable
    {
        public Constraint(string name, Constraint con)
            : base()
        {
            Ensure.NotNull(con, "constraint");

            GutsOfConstructor(name, con.expr, con.type, con.rhs);
        }

        public Constraint(string name, Expression expr, ConstraintType type, Expression rhs)
        {
            Ensure.NotNull(expr, "expression");
            Ensure.NotNull(rhs, "rhs expression");

            GutsOfConstructor(name, expr, type, rhs);
        }

        public Constraint(Expression expr, ConstraintType type, Expression rhs)
            : this(null, expr, type, rhs)
        {
        }

        public Constraint(Constraint con)
            : this(null, con)
        {
        }

        public static char GetOsiConstraintType(ConstraintType constraintType)
        {
            switch (constraintType)
            {
                case ConstraintType.EQ: return 'E';
                case ConstraintType.LE: return 'L';
                case ConstraintType.GE: return 'G';
                default: throw new SonnetException("Unknown constraint type " + constraintType);
            }
        }

        public bool IsFeasible()
        {
            EnsureNotDisposedOrFinalized();

            return MathExtension.CompareDouble(Slack(), 0.0) >= 0;
        }

        public override string ToString()
        {
            EnsureNotDisposedOrFinalized();

            StringBuilder tmp = new StringBuilder();
            string typeString = "";

            switch (this.type)
            {
                case ConstraintType.LE:
                    typeString = "<=";
                    break;
                case ConstraintType.EQ:
                    typeString = "==";
                    break;
                case ConstraintType.GE:
                    typeString = ">=";
                    break;
            }
            tmp.AppendFormat("{0} : ", Name);
            string exprString;
            //if (expr == nullptr && lean) exprString = "LEAN!";
            //else
            exprString = expr.ToString();

            tmp.AppendFormat("{0} {1} {2}",
                exprString,
                typeString,
                rhs.ToString());
            return tmp.ToString();
        }

        public virtual string ToLevelString()
        {
            EnsureNotDisposedOrFinalized();

            StringBuilder tmp = new StringBuilder();
            string typeString;
            switch (type)
            {
                case ConstraintType.LE:
                    typeString = "<=";
                    break;
                case ConstraintType.EQ:
                    typeString = "==";
                    break;
                case ConstraintType.GE:
                    typeString = ">=";
                    break;
                default:
                    throw new SonnetException("Unknown constraint type!");
            }

            string exprLevel;
            //if (expr == nullptr && lean) exprLevel = "LEAN!";
            //else 
            exprLevel = expr.Level().ToString();

            tmp.AppendFormat("{0} : ", Name);
            tmp.AppendFormat("{0} {1} {2}",
                exprLevel,
                typeString,
                rhs.Level());
            tmp.AppendFormat("  ( {0} )", price);
            return tmp.ToString();

        }

        //   a <= b <= c    
        // is    Constraint(a <= b) <= c, 
        // where c = upper, and a = con->expr, and b = con->rhs
        public static RangeConstraint operator <=(Constraint con, double upper)
        {
            con.EnsureNotDisposedOrFinalized();

            if (con.expr.NumberOfCoefficients > 0) // not constant!
            {
                throw new SonnetException("Range constraints must have constant lower and upper bounds!");
            }
            if (con.type != ConstraintType.LE)
            {
                throw new SonnetException("Range constraints can only be <= constraints");
            }

            return new RangeConstraint(con.expr.Constant, con.rhs, upper);
        }

        public static RangeConstraint operator >=(Constraint con, double rhs)
        {
            throw new SonnetException("Range constraints can only be <= constraints");
        }

        public virtual ConstraintType Type
        {
            get
            {
                EnsureNotDisposedOrFinalized();

                return type;
            }
        }
        public override string Name
        {
            set
            {
                EnsureNotDisposedOrFinalized();

                if (!name.Equals(value))
                {
                    name = value;
                    foreach (Solver solver in solvers) solver.SetConstraintName(this, name);
                }
            }
        }

        public double Price
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                return price;
            }
        }

        public double Value
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                return value;
            }
        }

        public bool Enabled
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                return enabled;
            }
            set
            {
                EnsureNotDisposedOrFinalized();
                if (value != enabled)
                {
                    enabled = value;
                    foreach (Solver solver in solvers) solver.SetConstraintEnabled(this, value);
                }
            }
        }

        public double Level()
        {
            EnsureNotDisposedOrFinalized();
            return expr.Level();
        }

        public virtual double Slack()
        {
            EnsureNotDisposedOrFinalized();

            //the slack values returned consist of the right-hand side minus the row activity level
            switch (type)
            {
                case ConstraintType.EQ:
                case ConstraintType.LE:
                    return rhs.Level() - Level();
                case ConstraintType.GE:
                    return Level() - rhs.Level();
            }

            throw new SonnetException("Unknown constraint type");
        }

        internal virtual void Assemble()
        {
            EnsureNotDisposedOrFinalized();

            expr.Subtract(rhs);
            rhs.Clear();

            double constant = expr.Constant;
            if (constant != 0.0)
            {
                expr.Subtract(constant);
                rhs.Subtract(constant);
            }
            expr.Assemble();

        }

        internal CoefVector Coefficients { get { EnsureNotDisposedOrFinalized(); return expr.Coefficients; } }
        internal CoefVector RhsCoefficients { get { EnsureNotDisposedOrFinalized(); return rhs.Coefficients; } }
        internal double RhsConstant { get { EnsureNotDisposedOrFinalized(); return rhs.Constant; } }
        public virtual double Lower
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                switch (this.type)
                {
                    case ConstraintType.EQ: return RhsConstant;
                    case ConstraintType.LE: return double.MinValue;
                    case ConstraintType.GE: return RhsConstant;
                    default: throw new SonnetException("Unknown constraint type");
                }
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public virtual double Upper
        {
            get
            {
                EnsureNotDisposedOrFinalized();
                switch (this.type)
                {
                    case ConstraintType.EQ: return RhsConstant;
                    case ConstraintType.LE: return RhsConstant;
                    case ConstraintType.GE: return double.MaxValue;
                    default: throw new SonnetException("Unknown constraint type");
                }

            }
            set
            {
                throw new NotSupportedException();
            }
        }

        internal void Assign(Solver solver, int offset, double price, double value)
        {
            EnsureNotDisposedOrFinalized();

            base.Assign(solver, offset);
            this.price = price;
            this.value = value;
        }

        protected static int numberOfConstraints = 0;
        protected Expression expr;
        protected Expression rhs;

        private void GutsOfConstructor(string name, Expression expr, ConstraintType type, Expression rhs)
        {
            id = numberOfConstraints++;

            this.expr = new Expression(expr);
            this.rhs = new Expression(rhs);
            this.type = type;
            this.enabled = true;

            if (name != null) Name = name;
            else Name = string.Format("Con[{0}]", id);
        }

        private ConstraintType type;
        private double price;
        private double value;
        private int disposed;
        private bool enabled;

        private void EnsureNotDisposedOrFinalized()
        {
            if (disposed > 0)// || finalized > 0)
            {
                string message = string.Format("Cannot use Constraint after it was disposed {0} times.", disposed);
                throw new ObjectDisposedException("Constraint", message);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (disposed > 0) return;

            if (!object.ReferenceEquals(expr, null))
            {
                expr.Dispose();
                expr = null;
            }

            if (!object.ReferenceEquals(rhs, null))
            {
                rhs.Dispose();
                rhs = null;
            }

            disposed++;
        }

        #endregion
    }

    public class RangeConstraint : Constraint
    {
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
                if (rhs.Constant < MathExtension.Infinity) rhs.Subtract(constant);
                // if the lower (lhs) is not already minus Infinity, then subtract the constant
                if (lower > -MathExtension.Infinity) lower -= constant;
            }
            expr.Assemble();
        }

        private double lower;
    }
}
