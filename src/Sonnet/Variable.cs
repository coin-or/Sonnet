// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    public enum VariableType
    {
        Continuous,
        Integer
    }

    public class Variable : ModelEntity
    {
        public Variable()
        {
            GutsOfConstructor(0.0, double.MaxValue, VariableType.Continuous, null);
        }

        public Variable(VariableType type)
        {
            GutsOfConstructor(0.0, double.MaxValue, type, null);
        }

        public Variable(string name)
        {
            GutsOfConstructor(0.0, double.MaxValue, VariableType.Continuous, name);
        }

        public Variable(double lower, double upper)
        {
            GutsOfConstructor(lower, upper, VariableType.Continuous, null);
        }

        public Variable(double lower, double upper, VariableType type)
        {
            GutsOfConstructor(lower, upper, type, null);
        }

        public Variable(string name, VariableType type)
        {
            GutsOfConstructor(0.0, double.MaxValue, type, null);
        }

        public Variable(string name, double lower, double upper)
        {
            GutsOfConstructor(lower, upper, VariableType.Continuous, name);
        }

        public Variable(string name, double lower, double upper, VariableType type)
        {
            GutsOfConstructor(lower, upper, type, name);
        }

        public static Variable[] NewVariables(int size)
        {
            Variable[] tmp = new Variable[size];
            for (int i = 0, n = tmp.Length; i < n; i++)
            {
                tmp[i] = new Variable();
            }
            return tmp;
        }

        public static Variable[] NewVariables(int size, double lower, double upper)
        {
            Variable[] tmp = new Variable[size];
            for (int i = 0, n = tmp.Length; i < n; i++)
            {
                tmp[i] = new Variable(lower, upper);
            }
            return tmp;
        }

        public static Variable[] NewVariables(int size, double lower, double upper, VariableType type)
        {
            Variable[] tmp = new Variable[size];
            for (int i = 0, n = tmp.Length; i < n; i++)
            {
                tmp[i] = new Variable(lower, upper, type);
            }
            return tmp;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} : [{2}, {3}]", Name, type, MathExtension.DoubleToString(lower), MathExtension.DoubleToString(upper));
        }

        public string ToLevelString()
        {
            return string.Format("{0} = {1}   ( {2} )", this, value, reducedCost);
        }

        public double Upper
        {
            get { return upper; }
            set
            {
                if (MathExtension.CompareDouble(upper, value) != 0)
                {
                    upper = value;
                    foreach(Solver solver in solvers) solver.SetVariableUpper(this, upper);
                }
            }
        }

        public double Lower
        {
            get { return lower; }
            set
            {
                if (MathExtension.CompareDouble(lower, value) != 0)
                {
                    lower = value;
                    foreach(Solver solver in solvers) solver.SetVariableLower(this, lower);
                }
            }
        }

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

        public bool IsFrozen
        {
            get { return frozen > 0; }
        }

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

        public double ReducedCost
        {
            get { return reducedCost; }
        }

        public bool IsFeasible()
        {
            if (MathExtension.CompareDouble(lower, value) > 0 && MathExtension.CompareDouble(value, upper) > 0) return false;

            if (type == VariableType.Integer && !MathExtension.IsInteger(value)) return false;

            return true;
        }

        internal virtual void Assign(Solver solver, int offset, double value, double reducedCost)
        {
            base.Assign(solver, offset);
            this.value = value;
            this.reducedCost = reducedCost;
        }

        protected double upper;
        protected double lower;
        protected VariableType type;
        protected static int numberOfVariables = 0;

        private void GutsOfConstructor(double lower, double upper, VariableType type, string name)
        {
            this.lower = lower;
            this.upper = upper;
            this.type = type;

            this.frozen = 0;
            this.id = numberOfVariables++;

            if (name != null) Name = name;
            else Name = string.Format("Var[{0}]", id);
        }

        private int frozen;
        private double value;
        private double reducedCost;

        #region Overloaded Operators

        public static Constraint operator <=(double val, Variable var)
        {
            return val <= (new Expression(var));// removed gcnew: this is put into a new constraint, which copies the exprs.
        }

        public static Constraint operator <=(Variable var, double val)
        {
            return (new Expression(var)) <= val;
        }

        public static Constraint operator <=(Variable var1, Variable var2)
        {
            return new Expression(var1) <= new Expression(var2);
        }

        public static Constraint operator >=(double val, Variable var)
        {
            return val >= new Expression(var);
        }

        public static Constraint operator >=(Variable var, double val)
        {
            return new Expression(var) >= val;
        }

        public static Constraint operator >=(Variable var1, Variable var2)
        {
            return new Expression(var1) >= new Expression(var2);
        }

        public static Constraint operator ==(double val, Variable var)
        {
            return val == new Expression(var);
        }

        public static Constraint operator !=(double val, Variable var)
        {
            throw new SonnetException("Cannot use != operator");
        }

        public static Constraint operator ==(Variable var, double val)
        {
            return new Expression(var) == val;
        }

        public static Constraint operator !=(Variable var, double val )
        {
            throw new SonnetException("Cannot use != operator");
        }
        
        public static Constraint operator ==(Variable var1, Variable var2)
        {
            return new Expression(var1) == new Expression(var2);
        }

        public static Constraint operator !=(Variable var1, Variable var2)
        {
            throw new SonnetException("Cannot use != operator");
        }

        // Overloaded mathematical operators.
        // Because these operators have only non-expression arguments, they cannot be implemented in expression
        // The + operator
        public static Expression operator +(double val, Variable var)
        {
            return (new Expression(val)).Add(var);
        }

        public static Expression operator +(Variable var, double coef)
        {
            return (new Expression(var)).Add(coef);
        }

        public static Expression operator +(Variable var1, Variable var2)
        {
            return (new Expression(var1)).Add(var2);
        }

        // The - operator
        public static Expression operator -(double coef, Variable var)
        {
            return (new Expression(coef)).Subtract(var);
        }

        public static Expression operator -(Variable var, double coef)
        {
            return (new Expression(var)).Subtract(coef);
        }

        public static Expression operator -(Variable var1, Variable var2)
        {
            return (new Expression(var1)).Subtract(var2);
        }

        // The * operator
        public static Expression operator *(double coef, Variable var)
        {
            return new Expression(coef, var);
        }

        public static Expression operator *(Variable var, double coef)
        {
            return new Expression(coef, var);
        }

        // The - operator
        public static Expression operator /(Variable var, double coef)
        {
            return (new Expression(var)).Divide(coef);
        }

        #endregion

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
