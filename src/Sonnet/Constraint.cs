// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// Specifies the types of constraints: less-or-equal (LE), greater-or-equal (GE) or equal (EQ)
    /// </summary>
    public enum ConstraintType { 
        /// <summary>
        /// Less-or-equal
        /// </summary>
        LE, 
        /// <summary>
        /// Greater-or-equal
        /// </summary>
        GE, 
        /// <summary>
        /// Equal
        /// </summary>
        EQ 
    };

    /// <summary>
    /// Class Constraints that can be added to one or more models.
    /// By definition, a constraint has a left-hand side, a type and a right-hand side.
    /// </summary>
    public class Constraint : ModelEntity
    {
        /// <summary>
        /// Initializes a new instance of the Constraint class based on a copy of the given constraint.
        /// A default name is used.
        /// </summary>
        /// <param name="con">The constraint to be copied.</param>
        public Constraint(Constraint con)
            : this(null, con)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Constraint class based on a copy of the given constraint,
        /// but with the given name.
        /// </summary>
        /// <param name="name">The name for the new constraint.</param>
        /// <param name="con">The constraint to be copied.</param>
        public Constraint(string name, Constraint con)
            :base(name)
        {
            Ensure.NotNull(con, "constraint");

            GutsOfConstructor(name, con.expr, con.type, con.rhs);
        }

        /// <summary>
        /// Initializes a new instance of the Constraint class with the default name, 
        /// of the given type with the given left and right-hand side expressions.
        /// The given expressions are copied, and therefore are no longer needed after this constructor is called.
        /// </summary>
        /// <param name="lhs">The left-hand side expression of the constraint.</param>
        /// <param name="type">The type of the constraint.</param>
        /// <param name="rhs">The right-hand side expression of the consrtaint.</param>
        public Constraint(Expression lhs, ConstraintType type, Expression rhs)
            : this(null, lhs, type, rhs)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Constraint class with the given name, 
        /// of the given type with the given left and right-hand side expressions.
        /// The given expressions are copied, and therefore are no longer needed after this constructor is called.
        /// </summary>
        /// <param name="name">The name for the new constraint.</param>
        /// <param name="lhs">The left-hand side expression of the constraint.</param>
        /// <param name="type">The type of the constraint.</param>
        /// <param name="rhs">The right-hand side expression of the consrtaint.</param>
        public Constraint(string name, Expression lhs, ConstraintType type, Expression rhs)
            : base(name)
        {
            Ensure.NotNull(lhs, "expression");
            Ensure.NotNull(rhs, "rhs expression");

            GutsOfConstructor(name, lhs, type, rhs);
        }

        /// <summary>
        /// Clear this constraint by clearing its expressions.
        /// </summary>
        public void Clear()
        {
            if (!object.ReferenceEquals(expr, null))
            {
                expr.Clear();
            }

            if (!object.ReferenceEquals(rhs, null))
            {
                rhs.Clear();
            }
        }

        #region ToString() methods
        /// <summary>
        /// Returns a System.String that represents the current Constraint.
        /// </summary>
        /// <returns>A System.String that represents the current Constraint.</returns>
        public override string ToString()
        {
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

        /// <summary>
        /// Returns a string that represents the value of this instance using the Level of the expressions.
        /// </summary>
        /// <returns>A string representation of this instance using the Level.</returns>
        public virtual string ToLevelString()
        {
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
        #endregion

        #region Overloaded operators
        //   a <= b <= c    
        // is    Constraint(a <= b) <= c, 
        // where c = upper, and a = con->expr, and b = con->rhs

        /// <summary>
        /// Creates a new RangeConstraint "l &lt;= expr &lt;= u"
        /// where l is the left-hand side constant of the given constraint,
        /// expr is the right-hand side of the constraint and u is the given upper bound.
        /// Note that this can be used as
        ///   RangeConstraint rcon = 2 &lt;= x + y &lt;= 10;
        /// because (2 &lt;= x + y) is first made into a constraint.
        /// </summary>
        /// <param name="con">The &lt;= constraint that will be used for its right-hand side expression. The left-hand side expressin must not contain any variables.</param>
        /// <param name="upper">The upper bound of the new range constraint.</param>
        /// <returns>The new range constraint.</returns>
        public static RangeConstraint operator <=(Constraint con, double upper)
        {
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

        /// <summary>
        /// Throws an exception: Range constraints can only be &lt;= constraints.
        /// </summary>
        /// <param name="con">The constraint.</param>
        /// <param name="rhs">The bound.</param>
        /// <returns>A SonnetException</returns>
        public static RangeConstraint operator >=(Constraint con, double rhs)
        {
            throw new SonnetException("Range constraints can only be <= constraints");
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the type of this constraint.
        /// </summary>
        public virtual ConstraintType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets or sets the name of this constraint.
        /// </summary>
        public override string Name
        {
            set
            {
                if (!Name.Equals(value))
                {
                    base.Name = value;
                    foreach (Solver solver in solvers) solver.SetConstraintName(this, Name);
                }
            }
        }

        /// <summary>
        /// Gets the price of this constraint in the current solution.
        /// </summary>
        public double Price
        {
            get { return price; }
        }

        /// <summary>
        /// Gets the value of this constraint in the current solution.
        /// </summary>
        public double Value
        {
            get { return value; }
        }

        /// <summary>
        /// Gets or sets whether this constraint is enabled (enforced).
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    foreach (Solver solver in solvers) solver.SetConstraintEnabled(this, value);
                }
            }
        }
        
        /// <summary>
        /// Gets the lower bound of this constraint. For EQ and GE constraint, this is the right-hand side constant.
        /// For LE constraints, this is -inf.
        /// Only RangeConstraints can Set the lower bound.
        /// </summary>
        public virtual double Lower
        {
            get
            {
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

        /// <summary>
        /// Gets the upper bound of this constraint. For EQ and LE constraint, this is the right-hand side constant.
        /// For GE constraints, this is +inf.
        /// Only RangeConstraints can Set the upper bound.
        /// </summary>
        public virtual double Upper
        {
            get
            {
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

        #endregion
        /// <summary>
        /// Determines the level of the left-hand side expression of this constraint in the current solution.
        /// </summary>
        /// <returns>The level of the left-hand side expression of this constraint in the current solution.</returns>
        public double Level()
        {
            return expr.Level();
        }

        /// <summary>
        /// Determines whether the current solution is feasible wrt this constraint by checking whether the slack is non-negative.
        /// </summary>
        /// <returns>True if this constraint is satisfied; false otherwise.</returns>
        public bool IsFeasible()
        {
            return !Slack().IsNegative();
        }

        /// <summary>
        /// Determines the slack of this constraint in the current solution. Regardless of the type of constraint, the slack should 
        /// be non-negative in a feasible solution.
        /// </summary>
        /// <returns>The slack of this constraint in the current solution.</returns>
        public virtual double Slack()
        {
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

        internal CoefVector Coefficients { get { return expr.Coefficients; } }
        internal CoefVector RhsCoefficients { get { return rhs.Coefficients; } }
        internal double RhsConstant { get { return rhs.Constant; } }

        internal virtual void Assemble()
        {
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

        internal void Assign(Solver solver, int offset, double price, double value)
        {
            base.Assign(solver, offset);
            this.price = price;
            this.value = value;
        }

        private void GutsOfConstructor(string name, Expression expr, ConstraintType type, Expression rhs)
        {
            Ensure.IsFalse(expr.IsQuadratic, "Quadratic constraints lhs are not supported");
            Ensure.IsFalse(rhs.IsQuadratic, "Quadratic constraints rhs are not supported");

            id = numberOfConstraints++;

            this.expr = new Expression(expr);
            this.rhs = new Expression(rhs);
            this.type = type;
            this.enabled = true;

            if (name != null) Name = name;
            else Name = string.Format("Con_{0}", id);
        }

        /// <summary>
        /// Counts the global number of constraints. Mainly used for id.
        /// </summary>
        protected static int numberOfConstraints = 0;

        private ConstraintType type;
        private double price;
        private double value;
        private bool enabled;

        /// <summary>
        /// The (left-hand side) expression of this constraint.
        /// </summary>
        
        protected Expression expr;
        /// <summary>
        /// The right-hand side expression of this constraint.
        /// </summary>
        protected Expression rhs;
    }
}
