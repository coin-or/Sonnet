// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// Specifies the types of variables: Continuous and Integer.
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// Continuous values
        /// </summary>
        Continuous,
        /// <summary>
        /// Integer values
        /// </summary>
        Integer
    }

    /// <summary>
    /// The Variable class represents variables in expressions which 
    /// are use in objectives and constraints.
    /// Each variable has a name, lower bound and upper bound.
    /// In addition, each variable can take either continuous or integer values. This is
    /// specified as the Type of the variable.
    /// Variables are not explicitly added to models. The variables used within a model are
    /// defined by the constraints and objective of a model.
    /// New values for properties (Lower, Upper, etc) are automatically updated across all solvers that use this variable.
    /// </summary>
    public class Variable : ModelEntity
    {
        /// <summary>
        /// Initializes a new instance of the Variable class with a default name,
        /// lower bound of 0, upper bound of +inf and which can take continuous values.
        /// </summary>
        public Variable()
            :this(null, 0.0, MathUtils.Infinity, VariableType.Continuous)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class of the given type, with a default name,
        /// lower bound of 0 and upper bound of +inf.
        /// </summary>
        /// <param name="type">The type of the new variable.</param>
        public Variable(VariableType type)
            :this(null, 0.0, MathUtils.Infinity, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class with the given name,
        /// lower bound of 0, upper bound of +inf and which can take continuous values.
        /// </summary>
        /// <param name="name">The name of the new variable.</param>
        public Variable(string name)
            :this(name, 0.0, MathUtils.Infinity, VariableType.Continuous)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class with a default name,
        /// and which can take continuous values between the given lower and upper bounds.
        /// </summary>
        /// <param name="lower">The lower bound of the new variable.</param>
        /// <param name="upper">The upper bound of the new variable.</param>
        public Variable(double lower, double upper)
            :this(null, lower, upper, VariableType.Continuous)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class of the given type, with a default name,
        /// and which can take continuous values between the given lower and upper bounds.
        /// </summary>
        /// <param name="lower">The lower bound of the new variable.</param>
        /// <param name="upper">The upper bound of the new variable.</param>
        /// <param name="type">The type of the new variable.</param>
        public Variable(double lower, double upper, VariableType type)
            :this(null, lower, upper, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class of the given type, with the given name,
        /// lower bound of 0 and upper bound of +inf.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public Variable(string name, VariableType type)
            :this(name, 0.0, MathUtils.Infinity, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class with the given name,
        /// and which can take continuous values between the given lower and upper bounds.
        /// </summary>
        /// <param name="name">The name of the new variable.</param>
        /// <param name="lower">The lower bound of the new variable.</param>
        /// <param name="upper">The upper bound of the new variable.</param>
        public Variable(string name, double lower, double upper)
            :this(name, lower, upper, VariableType.Continuous)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Variable class of the given type, with the given name,
        /// and which can take values between the given lower and upper bounds.
        /// </summary>
        /// <param name="name">The name of the new variable.</param>
        /// <param name="lower">The lower bound of the new variable.</param>
        /// <param name="upper">The upper bound of the new variable.</param>
        /// <param name="type">The type of the new variable.</param>
        public Variable(string name, double lower, double upper, VariableType type)
        {
            this.lower = lower;
            this.upper = upper;
            this.type = type;

            this.frozen = 0;
            this.id = numberOfVariables++;

            if (name != null) Name = name;
            else Name = string.Format("Var[{0}]", id);
        }

        /// <summary>
        /// Returns an array of the given size filled with new Variables constructed with a default name,
        /// lower bound of 0, upper bound of +inf and which can take continuous values.
        /// </summary>
        /// <param name="size">The number of new variables.</param>
        /// <returns>Array of variables of the given size.</returns>
        public static Variable[] NewVariables(int size)
        {
            Variable[] tmp = new Variable[size];
            for (int i = 0, n = tmp.Length; i < n; i++)
            {
                tmp[i] = new Variable();
            }
            return tmp;
        }

        /// <summary>
        /// Returns an array of the given size filled with new Variables which can take continuous values,
        /// constructed with a default name, which can take values between the given lower and upper bounds.
        /// </summary>
        /// <param name="size">The number of new variables.</param>
        /// <param name="lower">The lower bound of the new variables.</param>
        /// <param name="upper">The upper bound of the new variables.</param>
        /// <returns>Array of variables of the given size.</returns>
        public static Variable[] NewVariables(int size, double lower, double upper)
        {
            Variable[] tmp = new Variable[size];
            for (int i = 0, n = tmp.Length; i < n; i++)
            {
                tmp[i] = new Variable(lower, upper);
            }
            return tmp;
        }

        /// <summary>
        /// Returns an array of the given size filled with new Variables of the given type,
        /// constructed with a default name, which can take values between the given lower and upper bounds.
        /// </summary>
        /// <param name="size">The number of new variables.</param>
        /// <param name="lower">The lower bound of the new variables.</param>
        /// <param name="upper">The upper bound of the new variables.</param>
        /// <param name="type">The type of the new variables.</param>
        /// <returns>Array of variables of the given size.</returns>
        public static Variable[] NewVariables(int size, double lower, double upper, VariableType type)
        {
            Variable[] tmp = new Variable[size];
            for (int i = 0, n = tmp.Length; i < n; i++)
            {
                tmp[i] = new Variable(lower, upper, type);
            }
            return tmp;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current variable.</param>
        /// <returns>true if obj has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            // to prevent warning CS0660
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for this variable, based only on the id.
        /// </summary>
        /// <returns>A hash code for the current variable.</returns>
        public override int GetHashCode()
        {
            // to prevent warning CS0661
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current variable.
        /// </summary>
        /// <returns>A string that represents the current variable.</returns>
        public override string ToString()
        {
            return string.Format("{0} : {1} : [{2}, {3}]", Name, type, lower.ToDoubleString(), upper.ToDoubleString());
        }

        /// <summary>
        /// Returns a string that represent the current variable and its value 
        /// and reduced cost.
        /// </summary>
        /// <returns>A string representation of this instance using its value and reduced cost.</returns>
        public string ToLevelString()
        {
            return string.Format("{0} = {1}   ( {2} )", this, value, reducedCost);
        }

        /// <summary>
        /// Get or set the upper bound of this variable.
        /// </summary>
        public double Upper
        {
            get { return upper; }
            set
            {
                if (upper.CompareToEps(value) != 0)
                {
                    upper = value;
                    foreach(Solver solver in solvers) solver.SetVariableUpper(this, upper);
                }
            }
        }

        /// <summary>
        /// Get or set the lower bound value for this variable.
        /// </summary>
        public double Lower
        {
            get { return lower; }
            set
            { 
                if (lower.CompareToEps(value) != 0)
                {
                    lower = value;
                    foreach(Solver solver in solvers) solver.SetVariableLower(this, lower);
                }
            }
        }

        /// <summary>
        /// Get or set the type (Continuous, Integer) of this variable.
        /// </summary>
        public VariableType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    foreach(Solver solver in solvers) solver.SetVariableType(this, type);
                }
            }
        }

        /// <summary>
        /// Get or set the name of this constraint.
        /// </summary>
        public override string Name
        {
            set
            {
                if (!name.Equals(value))
                {
                    name = value;
                    foreach(Solver solver in solvers) solver.SetVariableName(this, name);
                }
            }
        }

        /// <summary>
        /// Return whether the value of this variable was frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return frozen > 0; }
        }

        /// <summary>
        /// Freeze the current value of this variable.
        /// In effect, the lower and upper bounds are both set to the current value.
        /// </summary>
        /// <returns>True if this variable was not yet frozen beforem, and false otherwise.</returns>
        public bool Freeze()
        {
            frozen++;

            if (frozen == 1)
            {
                // otherwise, this variable was already frozen..

                // Now, really do the Freezing:
                // Adjust the upper and lower bound *in the model only* to this value
                double value = Value;
                foreach(Solver solver in solvers) solver.SetVariableBounds(this, value, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempt to unfreeze this variable.
        /// If Freeze() is called multiple times, then UnFreeze() must be called at least as many times.
        /// Only at the last call is the variable value unfrozen.
        /// </summary>
        /// <returns>True if the variable value was unfrozen; false otherwise.</returns>
        public bool UnFreeze()
        {
            if (frozen > 0)
            {
                frozen--;

                if (frozen == 0)
                {
                    // Now, really do the UnFreezing:
                    // adjust the upper and lower bounds to their original values

                    foreach(Solver solver in solvers) solver.SetVariableBounds(this, Lower, Upper);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get or set the value of this variable in the current solution.
        /// </summary>
        public double Value
        {
            get
            {
#if (DEBUG)
                if (!Assigned)
                {
                    throw new SonnetException("Variable has no assigned model!");
                }
#endif
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Get the reduced cost of this variable in the current solution.
        /// </summary>
        public double ReducedCost
        {
            get { return reducedCost; }
        }

        /// <summary>
        /// Test whether this the current value of this variable is within the bounds and is integer if applicable.
        /// </summary>
        /// <returns>True if bound and type are satisfied.</returns>
        public bool IsFeasible()
        {
            if (!value.IsBetween(lower, upper)) return false;

            if (type == VariableType.Integer && !value.IsInteger()) return false;

            return true;
        }

        /// <summary>
        /// Assigns the current solution of the given solver to this variable.
        /// This includes the given offset, the value and reduced cost.
        /// A variable can be assigned to at most one solver at a time.
        /// This method is called after the solver finished solving.
        /// </summary>
        /// <param name="solver">The solver to be assigned</param>
        /// <param name="offset">The offset of this variable in the array of variables of the solver.</param>
        /// <param name="value">The value of this variable in the current solution.</param>
        /// <param name="reducedCost">The reduced cost of this variable in the current solution.</param>
        internal virtual void Assign(Solver solver, int offset, double value, double reducedCost)
        {
            base.Assign(solver, offset);
            this.value = value;
            this.reducedCost = reducedCost;
        }

        #region Overloaded Operators
        #region Operator <=
        /// <summary>
        /// Creates a new constraint "c &lt;= x"
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(double c, Variable x)
        {
            return c <= (new Expression(x));
        }

        /// <summary>
        /// Creates a new constraint "x &lt;= c"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Variable x, double c)
        {
            return (new Expression(x)) <= c;
        }

        /// <summary>
        /// Creates a new constraint "x &lt;= y"
        /// </summary>
        /// <param name="x">The left-hand side variable.</param>
        /// <param name="y">The right-hand side variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Variable x, Variable y)
        {
            return new Expression(x) <= y;
        }
        #endregion

        #region Operator >=
        /// <summary>
        /// Creates a new constraint "c &gt;= x"
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(double c, Variable x)
        {
            return c >= new Expression(x);
        }

        /// <summary>
        /// Creates a new constraint "x &gt;= c"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(Variable x, double c)
        {
            return new Expression(x) >= c;
        }

        /// <summary>
        /// Creates a new constraint "x &gt;= y"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="y">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(Variable x, Variable y)
        {
            return new Expression(x) >= y;
        }
        #endregion

        #region Operator == and !=
        /// <summary>
        /// Creates a new constraint "c == x"
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(double c, Variable x)
        {
            return c == new Expression(x);
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(double c, Variable x)
        {
            throw new SonnetException("Cannot use != operator");
        }

        /// <summary>
        /// Creates a new constraint "x == c"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(Variable x, double c)
        {
            return new Expression(x) == c;
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(Variable x, double c)
        {
            throw new SonnetException("Cannot use != operator");
        }
        
        /// <summary>
        /// Creates a new constraint "x == y"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="y">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(Variable x, Variable y)
        {
            return new Expression(x) == new Expression(y);
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="y">The variable.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(Variable x, Variable y)
        {
            throw new SonnetException("Cannot use != operator");
        }
        #endregion

        #region Operator +
        // Because these operators have only non-expression arguments, they cannot be implemented in expression.

        /// <summary>
        /// Creates a new expression "c + x"
        /// </summary>
        /// <param name="c">The constrant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(double c, Variable x)
        {
            return (new Expression(c)).Add(x);
        }

        /// <summary>
        /// Creates a new expression "x + c"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(Variable x, double c)
        {
            return (new Expression(x)).Add(c);
        }

        /// <summary>
        /// Creates a new expression "x + y"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="y">The variable.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(Variable x, Variable y)
        {
            return (new Expression(x)).Add(y);
        }
        #endregion

        #region Operator -
        /// <summary>
        /// Creates a new expression "c - x"
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(double c, Variable x)
        {
            return (new Expression(c)).Subtract(x);
        }

        /// <summary>
        /// Creates a new expression "x - c"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Variable x, double c)
        {
            return (new Expression(x)).Subtract(c);
        }

        /// <summary>
        /// Creates a new expression "x - y"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="y">The variable.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Variable x, Variable y)
        {
            return (new Expression(x)).Subtract(y);
        }
        #endregion
        
        /// <summary>
        /// Creates a new expression "c * x"
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(double c, Variable x)
        {
            return new Expression(c, x);
        }

        /// <summary>
        /// Creates a new expression "c * x"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(Variable x, double c)
        {
            return new Expression(c, x);
        }

        /// <summary>
        /// Creates a new expression "(1/c) * x"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator /(Variable x, double c)
        {
            return (new Expression(x)).Divide(c);
        }
        #endregion

        /// <summary>
        /// Counts the global number of variables. Mainly used for id.
        /// </summary>
        protected static int numberOfVariables = 0;

        private int frozen;
        private double value;
        private double reducedCost;

        /// <summary>
        /// The upper bound value.
        /// </summary>
        protected double upper;
        
        /// <summary>
        /// The lower bound value.
        /// </summary>
        protected double lower;

        /// <summary>
        /// The type of the current variable.
        /// </summary>
        protected VariableType type;
    }
}
