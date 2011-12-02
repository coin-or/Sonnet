// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    internal class MathExtension
    {
        public static bool IsInteger(double a)
        {
            return CompareDouble(System.Math.Round(a) - a, 0.0) == 0;
        }

        public static string DoubleToString(double a)
        {
            if (a <= -Infinity) return "-Inf";
            if (a >= Infinity) return "Inf";

            return a.ToString();
        }

        // return 0 if a and b are equal (relative to epsilon), or -1 if a < b - eps, or 1 if a > b + eps
        public static int CompareDouble(double a, double b)
        {
            double eps = MathExtension.Epsilon;
            // if either a or b is close to zero, then do absolute difference
            // otherwise (the 'if' here) do a relative comparison (relative to the absolute value of the highest of a and b)
            if (!UseAbsoluteComparisonOnly &&
                System.Math.Abs(a) >= Epsilon && System.Math.Abs(b) >= Epsilon)
                eps *= System.Math.Abs(System.Math.Max(a, b));

            if (a < b - eps)
                return -1;
            if (a > b + eps)
                return 1;
            return 0;
        }

        public const double Infinity = double.MaxValue;
        public const double Epsilon = 1e-5;
        public static bool UseAbsoluteComparisonOnly = false;
    }
    public class Utils
    {
        public static void Remove<T>(List<T> list, int index)
        {
            // This should be much faster, but without maintaining the (sorted?) order.
            // swap the last for i!!
            int n = list.Count;
            if (index < n - 1)
            {
                // somewhere in the middle of the list, then copy the last element into this position
                list[index] = list[n - 1];
                // and remove the last (copy)
                list.RemoveAt(n - 1);
            }
            else
            {
                //index == n - 1
                // if the last: then simply remove this one. Note that this does NOT copy elements.
                list.RemoveAt(index);
            }
        }
    }

    public class Ensure
    {
        public static void IsTrue(bool b)
        {
            if (!b) throw new ArgumentException();
        }
        public static void IsFalse(bool b)
        {
            if (b) throw new ArgumentException();
        }
        public static void Equals<T>(IEquatable<T> a, IEquatable<T> b)
        {
            if (!a.Equals(b)) throw new ArgumentException();
        }
        public static void NotNull(object obj)
        {
            if (object.ReferenceEquals(obj, null)) throw new ArgumentNullException();
        }

        public static void NotNull(object obj, string paramName)
        {
            if (object.ReferenceEquals(obj, null)) throw new ArgumentNullException(paramName);
        }

        //public static void NotNull(void obj, string paramName)
        //{
        //    if (void
        //}
    }

    internal struct Coef : IComparable<Coef>
    {
        public Coef(Variable aVar, double aCoef)
        {
            Ensure.NotNull(aVar, "variable of coef");

            var = aVar;
            id = var.id;
            coef = aCoef;

#if (DEBUG)
            if (double.IsNaN(coef)) throw new SonnetException("The value of the coefficient of variable " + var.Name + " is not a number! (NaN)");
#endif
        }

        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
            string varName = var.name;

            if (coef == 1.0) tmp.AppendFormat("{0}", varName);
            else if (coef == -1.0) tmp.AppendFormat("- {0}", varName);
            else if (coef < 0.0) tmp.AppendFormat("- {0} {1}", -coef, varName);
            else tmp.AppendFormat("{0} {1}", coef, varName);

            // note that a coef of -2.5 for x2 results in     "-2.5 x2", whereas in an expression
            // we might prefer ".... - 2.5 x2 ..." 

            return tmp.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Coef) return Equals((Coef)obj);
            return false;
        }

        public bool Equals(Coef rhs)
        {
            return id == rhs.id && coef == rhs.coef;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public int CompareTo(Coef coef)
        {
            return id.CompareTo(coef.id);
        }

        public double Level()
        {
            return coef * (var.Value);
        }

        public Coef Multiply(double multiplier)
        {
            coef *= multiplier;
            return this;
        }
        public Coef Divide(double divider)
        {
            coef /= divider;
            return this;
        }

        public Variable var;
        public int id;
        public double coef;

    }

    internal class CoefVector : List<Coef>
    {
        public void Remove(int index)
        {
            Utils.Remove<Coef>(this, index);
        }

        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj as CoefVector);
        //}
        //public bool Equals(CoefVector obj)
        //{
        //    if (object.ReferenceEquals(null, obj)) return false;

        //    if (coefs.Count != expr.coefs.Count) return false;

        //    int n = coefs.Count;
        //    for (int i = 0; i < n; i++)
        //    {
        //        if (!this[i].Equals(obj[i])) return false;
        //    }
        //    return true;
        //}

        //public override int GetHashCode()
        //{
        //    int hashCode = -int.MinValue;
        //    foreach (Coef coef in this)
        //    {
        //        hashCode += coef.GetHashCode();
        //    }
        //    return hashCode;
        //}
        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
            int cap = this.Capacity;
            int n = this.Count;
            for (int i = 0; i < n; i++)
            {
                Coef c = this[i];
                if (c.coef != 0.0)
                {
                    if (tmp.Length > 0)
                    {
                        tmp.Append(" ");
                        if (c.coef > 0.0) tmp.Append("+ ");
                    }

                    tmp.Append(c.ToString());
                }
            }
            return tmp.ToString();
        }
    }

    public class Expression : IDisposable
    {
        /// <summary>
        /// Constructor of empty expression (constant = 0)
        /// </summary>
        public Expression()
            : this(0.0)
        {
        }

        /// <summary>
        /// Constructor of new expression with only the given constrant
        /// </summary>
        /// <param name="constant">Constant to use</param>
        public Expression(double constant)
        {
            this.constant = constant;
            coefs = new CoefVector();
        }

        /// <summary>
        /// Constructor of new expression with one term: 1 * variable
        /// </summary>
        /// <param name="variable">Variable to use</param>
        public Expression(Variable variable)
            : this(0.0)
        {
            Ensure.NotNull(variable, "variable");

            coefs.Add(new Coef(variable, 1.0));
        }

        /// <summary>
        /// Constructor of new expression with one term: coef * variable
        /// </summary>
        /// <param name="coef">Multiplication coefficient</param>
        /// <param name="variable">Variable to use</param>
        public Expression(double coef, Variable variable)
            : this(0.0)
        {
            Ensure.NotNull(variable, "variable");

            if (coef != 0.0)
            {
                coefs.Add(new Coef(variable, coef));
            }
        }

        /// <summary>
        /// Constructor copies the given expression and multiplies all coefficients and constant with the given multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplier to use</param>
        /// <param name="expr">Expression to copy and multiply</param>
        public Expression(double multiplier, Expression expr)
            : this()
        {
            Ensure.NotNull(expr, "expr");
            expr.EnsureNotDisposedOrFinalized();

            if (multiplier == 0.0) return;

            int n = expr.coefs.Count;
            for (int i = 0; i < n; i++)
            {
                Coef c = expr.coefs[i]; // consider ref
                coefs.Add(new Coef(c.var, c.coef * multiplier));
            }

            constant = multiplier * expr.constant;
        }

        /// <summary>
        /// Constructor that copies the given expression.
        /// </summary>
        /// <param name="expr">Expression to copy</param>
        public Expression(Expression expr)
            : this()
        {
            Ensure.NotNull(expr, "expr");
            expr.EnsureNotDisposedOrFinalized();

            constant = expr.constant;
            coefs.AddRange(expr.coefs);
        }

        // Assignment operator cannot be overloaded in c#
        //public static Expression operator =(Expression rhs)
        //{
        //    Ensure.NotNull(rhs, "rhs");
        //
        //    if(object.ReferenceEquals(this, rhs)) return this;
        //    Clear();
        //    Add(rhs);
        //
        //    return this;
        //}

        /// <summary>
        /// Determines whether the specified Expression is equal to the current Expression.
        /// Note, this expression and the given expression are not necessarily assembled.
        /// Therefore, for example, the expression (x + x) not "Equals" (2 * x).
        /// This has nothing to do with equality (EQ) constraints.
        /// </summary>
        /// <param name="expr">The Expression to compare with the current Expression.</param>
        /// <returns>true iff the given expression has the same constant and all coefficients and variables as the current Expression.</returns>
        public bool Equals(Expression expr)
        {
            EnsureNotDisposedOrFinalized();
            if (object.ReferenceEquals(expr, null)) return false;
            if (object.ReferenceEquals(expr, this)) return true;

            expr.EnsureNotDisposedOrFinalized();

            //this.Assemble();
            //expression.Assemble();

            // coefs is a pointer to a CoefVector. CoefVector is derived from list, and list implements the "==" operator
            // However, the CoefVector's elements are Coefs, which implement "==" only by comparing the id of the variables of two coefs.
            if (this.constant != expr.constant) return false;

            if (this.coefs.Count != expr.coefs.Count) return false;

            return this.coefs.Equals(expr.coefs);
        }

        /// <summary>
        //  Determines whether the specified object is equal to the current Expression.
        /// </summary>
        /// <param name="obj">The object to compare with the current Expression.</param>
        /// <returns>True iff the given object is an Expression and is equal to the current Expression.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj as Expression);
        }

        /// <summary>
        /// Serves as a hash function for the current Expression, base on the constant, coefficients and variables.
        /// </summary>
        /// <returns>A hash code for the current Expression.</returns>
        public override int GetHashCode()
        {
            return this.constant.GetHashCode() ^ this.coefs.GetHashCode();
        }

        /// <summary>
        /// Returns a System.String that represents the current Expression.
        /// </summary>
        /// <returns>A System.String that represents the current Expression.</returns>
        public override string ToString()
        {
            EnsureNotDisposedOrFinalized();
            StringBuilder tmp = new StringBuilder();

            tmp.Append(coefs.ToString());
            // if there are coefs, but these are all 0.0, then coefs.size() will be > 0, but the string will still be empty
            //if (coefs.size() == 0) 
            if (tmp.Length == 0) tmp.Append(constant);
            else if (constant > 0.0)
            {
                tmp.Append(" + ");
                tmp.Append(constant);
            }
            else if (constant < 0.0)
            {
                tmp.Append(" - ");
                tmp.Append(Math.Abs(constant));
            }

            return tmp.ToString();
        }

        /// <summary>
        /// Adds the variable with the given coefficient to the current Expression.
        /// </summary>
        /// <param name="coef">The coefficient to be added.</param>
        /// <param name="variable">The Variable to be added with the coefficient.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(double coef, Variable variable)
        {
            EnsureNotDisposedOrFinalized();
            Ensure.NotNull(variable, "variable");

            if (coef != 0.0)
            {
                coefs.Add(new Coef(variable, coef));
            }

            return this;
        }

        /// <summary>
        /// Adds the given constant to the constant of the current Expression.
        /// </summary>
        /// <param name="constant">The constant value to be added.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(double constant)
        {
            EnsureNotDisposedOrFinalized();
            this.constant += constant;
            return this;
        }

        /// <summary>
        /// Adds the given variable with coefficient 1.0 to the current Expression.
        /// </summary>
        /// <param name="variable">The Variable to be added.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(Variable variable)
        {
            EnsureNotDisposedOrFinalized();
            Ensure.NotNull(variable, "variable");

            coefs.Add(new Coef(variable, 1.0));
            return this;
        }

        /// <summary>
        /// Adds the constant and a copy of the coefficients and variables of the given Expression to the current Expression.
        /// Cannot be used recursively.
        /// </summary>
        /// <param name="expr">The expression to be added.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(Expression expr)
        {
            EnsureNotDisposedOrFinalized();
            Ensure.NotNull(expr, "expr");

            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive additions not allowed.");

            coefs.AddRange(expr.coefs);
            constant += expr.constant;
            return this;
        }

        /// <summary>
        /// Adds the constant and a copy of the coefficients and variables of the given Expression, multiplied by the given factor
        /// to the current Expression.
        /// </summary>
        /// <param name="factor">The multiplication factor for the given Expression.</param>
        /// <param name="expr">The expression to be added.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(double factor, Expression expr)
        {
            EnsureNotDisposedOrFinalized();
            Ensure.NotNull(expr, "expr");

            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive additions not allowed.");

            if (factor != 0.0)
            {
                int n = expr.coefs.Count;
                for (int i = 0; i < n; i++)
                {
                    Coef c = expr.coefs[i]; // consider ref
                    coefs.Add(new Coef(c.var, factor * c.coef));
                }
                constant += factor * expr.constant;
            }
            return this;
        }

        /// <summary>
        /// Subtracts the given variable with the given coefficient from the current Expression.
        /// </summary>
        /// <param name="coef">The coefficient of the variable to be subtracted.</param>
        /// <param name="variable">The variable to be subtracted with the coefficient</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(double coef, Variable variable)
        {
            return this.Add(-coef, variable);
        }

        /// <summary>
        /// Subtracts the given expression multiplied by the given factor from the current Expression.
        /// </summary>
        /// <param name="factor">The multiplication factor for the given expression.</param>
        /// <param name="expr">The expression to be subtracted.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(double factor, Expression expr)
        {
            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive subtractions not allowed.");

            return this.Add(-factor, expr);
        }

        /// <summary>
        /// Subtracts the given constant from the constant of the current Expression.
        /// </summary>
        /// <param name="constant">The value to subtract.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(double constant)
        {
            return this.Add(-constant);
        }

        /// <summary>
        /// Subtracts the given variable (with coefficient 1.0) from the current Expression.
        /// </summary>
        /// <param name="var">The variable to subtract.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(Variable var)
        {
            return this.Add(-1.0, var);
        }

        /// <summary>
        /// Subtracts the given expression from the current Expression
        /// </summary>
        /// <param name="expr">The expression to subtract.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(Expression expr)
        {
            return this.Subtract(1.0, expr);
        }

        /// <summary>
        /// Multiplies the constant and all coefficients of the current Expression by the given multiplier.
        /// </summary>
        /// <param name="multiplier">The multiplier to be used.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Multiply(double multiplier)
        {
            if (multiplier == 0.0) Clear();
            else
            {
                int n = coefs.Count;
                for (int i = 0; i < n; i++)
                {
                    coefs[i] = coefs[i].Multiply(multiplier);
                }
                constant *= multiplier;
            }

            return this;
        }

        /// <summary>
        /// Divides the constant and all coefficients of the current Expression by the given divider.
        /// </summary>
        /// <param name="divider">The divider to be used.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Divide(double divider)
        {
            if (divider == 0.0) throw new DivideByZeroException();

            return this.Multiply(1.0 / divider);
        }

        internal void Clear()
        {
            EnsureNotDisposedOrFinalized();
            coefs.Clear();
            constant = 0.0;
        }

        /// <summary>
        /// Removes the given variable from the current Expression.
        /// This disrupts the order of coefficients.
        /// </summary>
        /// <param name="var">The variable to be removed.</param>
        /// <returns>The sum of all coefficients of the given variable before removing.</returns>
        protected double Remove(Variable var)
        {
            EnsureNotDisposedOrFinalized();
            Ensure.NotNull(var, "var");

            int aID = var.id;
            int n = coefs.Count;
            double coef = 0.0;

            for (int i = 0; i < n; )
            {
                Coef c = coefs[i]; // consider ref
                if (aID == c.id)
                {
                    coef += c.coef;
                    coefs.Remove(i);
                    n = coefs.Count; // needs to be updated!
                }
                else
                {
                    i++;
                }

            }
            return coef;
        }

        /// <summary>
        /// Determines the sum of all coefficients of the given variable.
        /// Same as Assemble(var);
        /// </summary>
        /// <param name="var">The variable involved.</param>
        /// <returns>The sum of all coefficients of the given variable.</returns>
        public double GetCoefficient(Variable var)
        {
            return Assemble(var);
        }

        /// <summary>
        /// Sets the overall coefficient of the given variable to the given value.
        /// </summary>
        /// <param name="var">The variable to be used.</param>
        /// <param name="value">The new coefficient for the given variable.</param>
        /// <returns>The sum of all *old* coefficients of the given variable.</returns>
        public double SetCoefficient(Variable var, double value)
        {
            double oldValue = Remove(var);
            this.Add(value, var);
            return oldValue;
        }

        /// <summary>
        /// Determines the sum of all coefficients of the given variable.
        /// </summary>
        /// <param name="var">The variable to be used.</param>
        /// <returns>The sum of all coefficients of the given variable.</returns>
        public double Assemble(Variable var)
        {
            EnsureNotDisposedOrFinalized();
            Ensure.NotNull(var, "var");

            int aID = var.id;
            double coef = 0.0;
            int n = coefs.Count;
            for (int i = 0; i < n; i++)
            {
                Coef c = coefs[i]; // consider ref
                if (aID == c.id) coef += c.coef;
            }
            return coef;
        }

        /// <summary>
        /// Assemble the current Expression.
        /// After Assembling, all variables will appear only once in the list of coefficients.
        /// </summary>
        public void Assemble()
        {
            EnsureNotDisposedOrFinalized();
            CoefVector assembled = new CoefVector();
            coefs.Sort();

            int n = coefs.Count;
            for (int i = 0; i < n; )
            {
                Coef newc = coefs[i]; // consider ref
                i++;

                while (i < n)
                {
                    Coef c = coefs[i];
                    if (newc.id != c.id) break;

                    newc.coef += c.coef;
                    i++;
                }

                assembled.Add(new Coef(newc.var, newc.coef));
            }

            coefs.Clear();
            coefs.AddRange(assembled);
        }

        /// <summary>
        /// Clears the current Expression and then adds the given expression.
        /// </summary>
        /// <param name="expr">The expression to be assigned.</param>
        /// <returns>The updated current Expression, equal to the given Expression.</returns>
        public Expression Assign(Expression expr)
        {
            // the assign erases any existing elements first, and then copies the aExp
            Clear();
            return this.Add(expr);
        }

        /// <summary>
        /// Clears the current Expression and sets the given constant.
        /// </summary>
        /// <param name="constant">The constant to be assigned.</param>
        /// <returns>The updated current Expression, equal to the given constant.</returns>
        public Expression Assign(double constant)
        {
            Clear();
            return this.Add(constant);
        }

        /// <summary>
        /// Gets the vector of coefficients and variables of the current Expression.
        /// </summary>
        internal CoefVector Coefficients
        {
            get
            {
                EnsureNotDisposedOrFinalized();

                return coefs;
            }
        }

        /// <summary>
        /// Gets the constant of the current Expression.
        /// </summary>
        public double Constant
        {
            get
            {
                EnsureNotDisposedOrFinalized();

                return constant;
            }
        }

        /// <summary>
        /// Returns the number of coefficients of the current Expression.
        /// </summary>
        public int NumberOfCoefficients
        {
            get
            {
                EnsureNotDisposedOrFinalized();

                return coefs.Count;
            }
        }

        /// <summary>
        /// Calculates constant plus the product of all coefficients and the Value in the current solution of their variables.
        /// </summary>
        /// <returns>The constant plus the product of all coefficients and the Value in the current solution of their variables.</returns>
        public double Level()
        {
            EnsureNotDisposedOrFinalized();

            int n = coefs.Count;
            double level = constant;
            foreach (Coef coef in coefs)
            {
                level += coef.Level();
            }

            return level;
        }
        #region Overloaded Operators
        // (?) Note that these overloaded operators use implicit conversion from variable and double to expression!
        
        /// <summary>
        /// Returns a new Constraint "lhs &lt;= rhs".
        /// The expressions are copied.
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="rhs">The rigt-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Expression lhs, Expression rhs)
        {
            return new Constraint(lhs, ConstraintType.LE, rhs);
        }

        /// <summary>
        /// Returns a new Constraint "lhs &lt;= val".
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Constraint operator <=(Expression lhs, double val)
        {
            return lhs <= new Expression(val);
        }

        public static Constraint operator <=(double val, Expression rhs)
        {
            return (new Expression(val) <= rhs); 
        }

        public static Constraint operator <=(Variable var, Expression rhs)
        {
            return (new Expression(var) <= rhs); 
        }

        public static Constraint operator <=(Expression lhs, Variable var)
        {
            return (lhs <= new Expression(var));
        }


        /// The >= operator
        public static Constraint operator >=(Expression lhs, Expression rhs)
        {
            return new Constraint(lhs, ConstraintType.GE, rhs);
        }

        public static Constraint operator >=(Expression lhs, double val)
        {
            return (lhs >= new Expression(val));
        }

        public static Constraint operator >=(double val, Expression rhs)
        {
            return (new Expression(val) >= rhs);
        }

        public static Constraint operator >=(Variable var, Expression rhs)
        {
            return (new Expression(var) >= rhs);
        }

        public static Constraint operator >=(Expression lhs, Variable var)
        {
            return (lhs >= new Expression(var));
        }

        /// The == operator, eg  lhs == rhs
        public static Constraint operator ==(Expression lhs, Expression rhs)
        {
            return new Constraint(lhs, ConstraintType.EQ, rhs);
        }

        public static Constraint operator !=(Expression lhs, Expression rhs)
        {
            throw new SonnetException("Cannot use != operator");
        }

        public static Constraint operator ==(Expression lhs, double val)
        {
            return (lhs == new Expression(val));
        }

        public static Constraint operator !=(Expression lhs, double val)
        {
            throw new SonnetException("Cannot use != operator");
        }

        public static Constraint operator ==(double val, Expression rhs)
        {
            return (new Expression(val) == rhs);
        }

        public static Constraint operator !=(double val, Expression rhs)
        {
            throw new SonnetException("Cannot use != operator");
        }

        public static Constraint operator ==(Variable var, Expression rhs)
        {
            return (new Expression(var) == rhs);
        }

        public static Constraint operator !=(Variable var, Expression rhs)
        {
            throw new SonnetException("Cannot use != operator");
        }

        public static Constraint operator ==(Expression lhs, Variable var)
        {
            return (lhs == new Expression(var));
        }

        public static Constraint operator !=(Expression lhs, Variable var)
        {
            throw new SonnetException("Cannot use != operator");
        }
        // Overloaded mathematical operators
        // The + operator
        public static Expression operator +(double val, Expression expr)
        {
            return new Expression(val).Add(expr);
        }

        public static Expression operator +(Variable var, Expression expr)
        {
            return (new Expression(var)).Add(expr);
        }

        public static Expression operator +(Expression expr, double coef)
        {
            return (new Expression(expr)).Add(coef);
        }

        public static Expression operator +(Expression expr, Variable var)
        {
            return (new Expression(expr)).Add(var);
        }

        //   returns copy of lhs + rhs  
        // so,   tmp = exp1 + exp2 + exp3   performs  
        //		((copy exp1, add exp2), copy result (exp1 and exp2), add exp3): n1 + n2 + n1 + n2 + n3!!
        // better: tmp.Add(exp1), tmp.Add(exp2), tmp.Add(exp3) : n1 + n2 + n3 !
        // which is equivalent to tmp.Add(exp1).Add(exp2).Add(exp3)
        public static Expression operator +(Expression expr1, Expression expr2)
        {
            return (new Expression(expr1)).Add(expr2);
        }

        // overloaded mathematical operators
        // the - operator
        public static Expression operator -(double coef, Expression expr)
        {
            return (new Expression(coef)).Subtract(expr);
        }

        public static Expression operator -(Variable var, Expression expr)
        {
            return (new Expression(var)).Subtract(expr);
        }

        public static Expression operator -(Expression expr, double coef)
        {
            return (new Expression(expr)).Subtract(coef);
        }

        public static Expression operator -(Expression expr, Variable var)
        {
            return (new Expression(expr)).Subtract(var);
        }

        public static Expression operator -(Expression expr1, Expression expr2)
        {
            return (new Expression(expr1)).Subtract(expr2);
        }

        // overloaded mathematical operators
        // the * operator
        public static Expression operator *(double coef, Expression expr)
        {
            return new Expression(coef, expr);
        }

        public static Expression operator *(Expression expr, double coef)
        {
            return new Expression(coef, expr);
        }

        // overloaded mathematical operators
        // the / operator
        public static Expression operator /(Expression expr, double coef)
        {
            return (new Expression(expr)).Divide(coef);
        }

        // Some mathematical functions
        // returns a new expression that is the sum of the given expressions
        public static Expression Sum(IEnumerable<Expression> expressions)
        {
            Expression sum = new Expression();
            foreach (Expression expr in expressions)
            {
                sum.Add(expr);
            }
            return sum;
        }

        // returns a new expression that is the sum of the given *expressions*
        public static Expression Sum(System.Collections.IEnumerable expressions)
        {
            Expression sum = new Expression();
            foreach (object obj in expressions)
            {
                // is this "obj" another Expression, or else a Variable?
                Expression expr = obj as Expression;
                if (!object.ReferenceEquals(null, expr))
                {
                    sum.Add(expr);
                }
                else
                {
                    Variable var = (Variable)obj;
                    sum.Add(var);
                }
            }
            return sum;
        }


        // returns a new expression that is the sum of the given variables
        public static Expression Sum(IEnumerable<Variable> variables)
        {
            Expression sum = new Expression();
            foreach (Variable var in variables)
            {
                sum.Add(var);
            }
            return sum;
        }

        //public static Expression ScalarProduct(double[] coefs, System.Collections.Generic.IList<Variable> variables)
        //{
        //            public static Expression ScalarProduct(double[] coefs, System.Collections.IList variables)

        //    }


        public static Expression ScalarProduct(double[] coefs, System.Collections.IList variables)
        {
            if (coefs.Length != variables.Count)
            {
                throw new SonnetException("For scalar product, the number of coefficients and variables must be the same.");
            }

            Expression sum = new Expression();
            int n = variables.Count;
            for (int i = 0; i < n; i++)
            {
                Variable variable = (Variable)variables[i];
                sum.Add(coefs[i], variable);
            }
            return sum;
        }

        public static Expression ScalarProduct(double[] coefs, Variable[] variables)
        {
            if (coefs.Length != variables.Length)
            {
                throw new SonnetException("For scalar product, the number of coefficients and variables must be the same.");
            }

            Expression sum = new Expression();
            int n = variables.Length;
            for (int i = 0; i < n; i++)
            {
                sum.Add(coefs[i], variables[i]);
            }
            return sum;
        }

        public static Expression ScalarProduct(IEnumerable<double> coefs, IEnumerable<Variable> vars)
        {
            Expression sum = new Expression();
            IEnumerator<Variable> varEnumerator = vars.GetEnumerator();
            IEnumerator<double> coefEnumerator = coefs.GetEnumerator();
            while (varEnumerator.MoveNext())
            {
                if (!coefEnumerator.MoveNext()) throw new SonnetException("For scalar product, the number of coefficients and variables must be the same.");

                Variable var = varEnumerator.Current;
                double coef = coefEnumerator.Current;

                sum.Add(coef, var);
            }

            if (coefEnumerator.MoveNext()) throw new SonnetException("For scalar product, the number of coefficients and variables must be the same.");
            return sum;
        }

        #endregion

        private CoefVector coefs;
        private double constant;
        #region IDisposable Members

        private void EnsureNotDisposedOrFinalized()
        {
            if (isDisposed) // || finalized > 0)
            {
                string message = "Cannot use Expression after it was disposed.";
                throw new ObjectDisposedException("Expression", message);
            }
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            if (!object.ReferenceEquals(coefs, null))
            {
                coefs.Clear();
                coefs.TrimExcess();
                coefs = null;
            }
        }

        #endregion
    }
}
