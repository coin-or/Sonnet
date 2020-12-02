// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// The class Expression is an important building block in the building of a Model.
    /// An Expression consists of a constant (double), an array of variables with their coefficients (the linear part)
    /// and an array of variable pairs and their coefficients (the quadratic part).
    /// Many overloaded operators exists for expressions. In principle, these operators 
    /// return *new* objects (Expressions or Constraints), with all coefficients copied.
    /// On the other hand, methods exist for manipulating the current expression.
    /// For example, the overloaded + operator returns copy of lhs + rhs:
    ///   tmp = exp1 + exp2 + exp3;
    /// performs  
    ///		((copy exp1, add exp2), copy result (exp1 and exp2), add exp3)
    /// thus performing n1 + n2 + n1 + n2 + n3 operations in total.
    /// More efficient is
    ///   tmp.Add(exp1); tmp.Add(exp2); tmp.Add(exp3);
    /// or equivalently 
    ///   tmp.Add(exp1).Add(exp2).Add(exp3);
    /// which performs only n1 + n2 + n3 operations.
    /// </summary>
    public sealed class Expression 
    {
        #region Constructors
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
            quadCoefs = new QuadCoefVector();
        }

        /// <summary>
        /// Constructor of new expression with one term: 1 * variable
        /// </summary>
        /// <param name="variable">Variable to use</param>
        public Expression(Variable variable)
            : this(1.0, variable)
        {
        }

        /// <summary>
        /// Constructor of new expression with one term: 1 * variable1 * variable2
        /// </summary>
        /// <param name="variable1">Variable1 to use</param>
        /// <param name="variable2">Variable2 to use</param>
        public Expression(Variable variable1, Variable variable2)
            : this(1.0, variable1, variable2)
        {
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
        /// Constructor of new quadratic expression with one term: coef * variable1 * variable2
        /// </summary>
        /// <param name="coef">Multiplication coefficient</param>
        /// <param name="variable1">Variable1 to use</param>
        /// <param name="variable2">Variable2 to use</param>
        public Expression(double coef, Variable variable1, Variable variable2)
            : this(0.0)
        {
            Ensure.NotNull(variable1, "variable1");
            Ensure.NotNull(variable2, "variable2");

            if (coef != 0.0)
            {
                quadCoefs.Add(new QuadCoef(variable1, variable2, coef));
            }
        }

        /// <summary>
        /// Constructor that copies the given expression.
        /// </summary>
        /// <param name="expr">Expression to copy</param>
        public Expression(Expression expr)
            : this()
        {
            Ensure.NotNull(expr, "expr");

            this.Add(expr);
        }
        #endregion // Constructors

        /// <summary>
        /// Removes all coefficients from this expression and set the constant to 0.
        /// </summary>
        public void Clear()
        {
            if (!object.ReferenceEquals(coefs, null))
            {
                coefs.Clear();
                coefs.TrimExcess();

                quadCoefs.Clear();
                quadCoefs.TrimExcess();
            }
            constant = 0.0;
        }


        // Assignment operator cannot be overloaded in c#
        //public static Expression operator =(Expression rhs)

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
            if (object.ReferenceEquals(expr, null)) return false;
            if (object.ReferenceEquals(expr, this)) return true;

            // coefs is a pointer to a CoefVector. CoefVector is derived from list, and list implements the "==" operator
            // However, the CoefVector's elements are Coefs, which implement "==" only by comparing the id of the variables of two coefs.
            if (this.constant != expr.constant) return false;

            if (this.coefs.Count != expr.coefs.Count) return false;

            if (this.quadCoefs.Count != expr.quadCoefs.Count) return false;

            if (!this.coefs.Equals(expr.coefs)) return false;

            if (!this.quadCoefs.Equals(expr.quadCoefs)) return false;

            return true;
        }

        /// <summary>
        ///  Determines whether the specified object is equal to the current Expression.
        /// </summary>
        /// <param name="obj">The object to compare with the current Expression.</param>
        /// <returns>True iff the given object is an Expression and is equal to the current Expression.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Expression);
        }

        /// <summary>
        /// Serves as a hash function for the current Expression, base on the constant, coefficients and variables.
        /// </summary>
        /// <returns>A hash code for the current Expression.</returns>
        public override int GetHashCode()
        {
            // don't use constant.GetHashCode() because it's not readonly
            return this.coefs.GetHashCode() ^ this.quadCoefs.GetHashCode();
        }

        /// <summary>
        /// Returns a System.String that represents the current Expression.
        /// </summary>
        /// <returns>A System.String that represents the current Expression.</returns>
        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();

            tmp.Append(coefs.ToString());

            if (IsQuadratic)
            {
                if (tmp.Length == 0) tmp.Append(quadCoefs.ToString());
                else if (quadCoefs[0].coef > 0.0)
                {
                    tmp.Append(" + ");
                    tmp.Append(quadCoefs.ToString());
                }
                else if (quadCoefs[0].coef < 0.0)
                {
                    tmp.Append(" ");
                    tmp.Append(quadCoefs.ToString());
                }
            }

            // if there are coefs, but these are all 0.0, then coefs.size() will be > 0, but the string will still be empty
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
            Ensure.NotNull(variable, "variable");

            if (coef != 0.0)
            {
                coefs.Add(new Coef(variable, coef));
            }

            return this;
        }

        /// <summary>
        /// Adds the given quadratic term coef * var1 * var2 to the current Expression.
        /// </summary>
        /// <param name="coef">The coefficient to be used.</param>
        /// <param name="var1">The var1 to be added with the coefficient.</param>
        /// <param name="var2">The var2 to be added with the coefficient.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(double coef, Variable var1, Variable var2)
        {
            Ensure.NotNull(var1, "variable1");
            Ensure.NotNull(var2, "variable2");

            if (coef != 0.0)
            {
                quadCoefs.Add(new QuadCoef(var1, var2, coef));
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
            return Add(1.0, variable);
        }

        /// <summary>
        /// Adds the constant and a copy of the coefficients and variables of the given Expression to the current Expression.
        /// Cannot be used recursively.
        /// </summary>
        /// <param name="expr">The expression to be added.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(Expression expr)
        {
            Ensure.NotNull(expr, "expr");

            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive additions not allowed.");

            coefs.AddRange(expr.coefs);
            quadCoefs.AddRange(expr.quadCoefs);
            constant += expr.constant;
            return this;
        }

        /// <summary>
        /// Adds (multiplier * expr) to the current Expression.
        /// </summary>
        /// <param name="multiplier">The multiplier for the given Expression.</param>
        /// <param name="expr">The expression to be added.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Add(double multiplier, Expression expr)
        {
            Ensure.NotNull(expr, "expr");

            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive additions not allowed.");

            if (multiplier != 0.0)
            {
                int n = expr.coefs.Count;
                for (int i = 0; i < n; i++)
                {
                    Coef c = expr.coefs[i]; // consider ref
                    coefs.Add(new Coef(c.var, multiplier * c.coef));
                }
                int qn = expr.quadCoefs.Count;
                for (int i = 0; i < qn; i++)
                {
                    QuadCoef c = expr.quadCoefs[i]; // consider ref
                    quadCoefs.Add(new QuadCoef(c.var1, c.var2, multiplier * c.coef));
                }
                constant += multiplier * expr.constant;
            }
            return this;
        }


        /// <summary>
        /// Adds to the current expresion the multiplication of coef * var * expr
        /// The given expr cannot be quadratic, since then var * expr would not be quadratic.
        /// </summary>
        /// <param name="coef"></param>
        /// <param name="var"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public Expression Add(double coef, Variable var, Expression expr)
        {
            Ensure.NotNull(var, "var");
            Ensure.NotNull(expr, "expr");
            Ensure.IsFalse(expr.IsQuadratic, "Cannot multiply expression because the result would not be quadratic");

            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive additions not allowed.");

            if (coef != 0.0)
            {
                // c + b1x1 + b2x2 + q1x1x1 + q2x1x2 + coef * var * (b + a1x1 +a2x2)
                // constant is unchanged

                // linear coefs get added term coef * b * var
                coefs.Add(new Coef(var, coef * expr.constant));
                
                // quad coefs get added terms (coef * a1) * var * x1 + (coef * a2) * var * x2
                // loop over all linear coefs of expr 
                int qn = expr.coefs.Count;
                for (int i = 0; i < qn; i++)
                {
                    Coef c = expr.coefs[i];
                    this.quadCoefs.Add(new QuadCoef(var, c.var, coef * c.coef));
                }
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
        /// Subtracts the quadratic term variable1 times variable2 with the given coefficient from the current Expression.
        /// </summary>
        /// <param name="coef">The coefficient of the variable to be subtracted.</param>
        /// <param name="variable1">The variable1 to be subtracted with the coefficient</param>
        /// <param name="variable2">The variable2 to be subtracted with the coefficient</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(double coef, Variable variable1, Variable variable2)
        {
            return this.Add(-coef, variable1, variable2);
        }

        /// <summary>
        /// Subtracts the given expression multiplied by the given multiplier from the current Expression.
        /// </summary>
        /// <param name="multiplier">The multiplier for the given expression.</param>
        /// <param name="expr">The expression to be subtracted.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(double multiplier, Expression expr)
        {
            if (object.ReferenceEquals(this, expr)) throw new SonnetException("Recursive subtractions not allowed.");

            return this.Add(-multiplier, expr);
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
        /// Subtracts the given quadratic term (var1 * var2) from the current Expression.
        /// </summary>
        /// <param name="var1">The variable1 to subtract.</param>
        /// <param name="var2">The variable2 to subtract.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Subtract(Variable var1, Variable var2)
        {
            return this.Add(-1.0, var1, var2);
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
                for (int i = 0, n = coefs.Count; i < n; i++)
                {
                    coefs[i] = coefs[i].Multiply(multiplier);
                }

                for (int i = 0, qn = quadCoefs.Count; i < qn; i++)
                {
                    quadCoefs[i] = quadCoefs[i].Multiply(multiplier);
                }
                constant *= multiplier;
            }

            return this;
        }

        /// <summary>
        /// Multiplies the current expression with the given variable.
        /// Same as Multiply(1.0, var)
        /// The current expression cannot already be quadratic.
        /// </summary>
        /// <param name="var">The variable to multiply with</param>
        /// <returns>The updated current Expression</returns>
        public Expression Multiply(Variable var)
        {
            return this.Multiply(1.0, var);
        }

        /// <summary>
        /// Multiplies the current expression with the given coef * variable.
        /// This expression cannot already be quadratic.
        /// The given expression cannot be the current expression.
        /// </summary>
        /// <param name="coef">The coefficient to multiply with</param>
        /// <param name="var">The variable to multiply with</param>
        /// <returns>The updated current Expression (this)</returns>
        public Expression Multiply(double coef, Variable var)
        {
            Ensure.NotNull(var, "var");
            Ensure.IsFalse(this.IsQuadratic, "Cannot multiply expression because the result would not be quadratic");

            // All linear coefs become quad coefs
            int n = this.coefs.Count;
            for (int i = 0; i < n; i++)
            {
                Coef c = this.coefs[i]; // consider ref
                quadCoefs.Add(new QuadCoef(var, c.var, coef * c.coef));
            }

            // then clear coefs and add as only coef the old constant * var
            this.coefs.Clear();
            if (this.constant != 0.0) coefs.Add(new Coef(var, coef * this.constant));
            
            // lastly, no remaining constant.
            this.constant = 0.0;

            return this;
        }

        /// <summary>
        /// Multiplies the current expression with the given expression
        /// The resulting expression cannot be more than quadratic.
        /// The expression can be the the same as current (for Squared)
        /// </summary>
        /// <param name="expr">The expression to be multiplied with</param>
        /// <returns>The updated current Expression (this)</returns>
        public Expression Multiply(Expression expr)
        {
            string message = "Cannot multiply expression because the result would not be quadratic";
            if (this.NumberOfQuadCoefficients > 0)
            {
                // if the current expression is already quadratic, then the expr to multiply with has to be just a constant
                Ensure.IsFalse(expr.NumberOfCoefficients > 0 || expr.NumberOfQuadCoefficients > 0, message);

                this.Multiply(expr.constant);
                return this;
            }
            else if (this.NumberOfCoefficients > 0)
            {
                // if the current expression is not quadratic but has linear terms, then the expression to multiply with cannot be quadratic
                Ensure.IsFalse(expr.NumberOfQuadCoefficients > 0, message);

                // this(c + a1x1 + a2x2)   *    (d + b1x1 + b2x2)
                // 
                // c * (d + b1x1 + b2x2) + a1x1 * (d + b1x1 + b2x2) + a2x2 * ( d + b1x1 + b2x2) 
                Expression result = new Expression();
                result.Add(this.constant, expr);

                int n = this.coefs.Count;
                for (int i = 0; i < n; i++)
                {
                    Coef c = this.coefs[i];
                    result.Add(c.coef, c.var, expr);
                }

                this.Clear();
                this.Add(result);
                return this;
            }
            else
            {
                // if the current expression is just a constant, then multiply by expression
                // copy the current constant, clear the current expression and build using Add with multiplier
                double multiplier = this.constant;
                if (object.ReferenceEquals(this, expr))
                {
                    Expression result = new Expression();
                    result.Add(multiplier, expr);

                    this.Clear();
                    this.Add(result);
                }
                else
                {
                    this.Clear();
                    this.Add(multiplier, expr);
                }
                return this;
            }
        }

        /// <summary>
        /// Divides the constant and all coefficients of the current Expression by the given divider.
        /// </summary>
        /// <param name="divider">The divider to be used. Must be non-zero.</param>
        /// <returns>The updated current Expression.</returns>
        public Expression Divide(double divider)
        {
            if (divider == 0.0) throw new DivideByZeroException();

            return this.Multiply(1.0 / divider);
        }
        /// <summary>
        /// Returns the expression * expression for the current expression.
        /// The resulting expression cannot be more than quadratic.
        /// </summary>
        /// <returns>The updated current expression (this).</returns>
        public Expression Squared()
        {
            return this.Multiply(this);
        }

        /// <summary>
        /// Removes all linear terms of the given variable from the current Expression.
        /// Note: This method reorders the coefficients.
        /// </summary>
        /// <param name="var">The variable to be removed.</param>
        /// <returns>The sum of all coefficients of the given variable before removing.</returns>
        private double Remove(Variable var)
        {
            Ensure.NotNull(var, "var");

            int aID = var.id;
            double coef = 0.0;

            for (int i = 0, n = coefs.Count; i < n; )
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
        /// Removes all quadratic terms of the given variables from the current Expression.
        /// Note: This method reorders the coefficients.
        /// </summary>
        /// <param name="var1">The variable1 to be removed.</param>
        /// <param name="var2">The variable2 to be removed.</param>
        /// <returns>The sum of all coefficients of the given variables terms before removing.</returns>
        private double Remove(Variable var1, Variable var2)
        {
            Ensure.NotNull(var1, "var1");
            Ensure.NotNull(var2, "var2");

            QuadCoef qc = new QuadCoef(var1, var2, 1.0);
            double coef = 0.0;

            for (int i = 0, qn = quadCoefs.Count; i < qn;)
            {
                QuadCoef c = quadCoefs[i]; // consider ref
                if (qc.EqualsVariables(c))
                {
                    coef += c.coef;
                    quadCoefs.Remove(i);
                    qn = quadCoefs.Count; // needs to be updated!
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
        /// Determines the sum of all coefficients of the given pair of variable.
        /// Same as Assemble(var1, var2);
        /// </summary>
        /// <param name="var1">The variable1 involved.</param>
        /// <param name="var2">The variable2 involved.</param>
        /// <returns>The sum of all coefficients of the given variable.</returns>
        public double GetCoefficient(Variable var1, Variable var2)
        {
            return Assemble(var1, var2);
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
        /// Sets the overall coefficient of the given pair of variables to the given value.
        /// </summary>
        /// <param name="var1">The variable1 to be used.</param>
        /// <param name="var2">The variable2 to be used.</param>
        /// <param name="value">The new coefficient for the given variables.</param>
        /// <returns>The sum of all *old* coefficients of the given variables.</returns>
        public double SetCoefficient(Variable var1, Variable var2, double value)
        {
            double oldValue = Remove(var1, var2);
            this.Add(value, var1, var2);
            return oldValue;
        }

        /// <summary>
        /// Determines the sum of all coefficients of the given variable.
        /// </summary>
        /// <param name="var">The variable to be used.</param>
        /// <returns>The sum of all coefficients of the given variable.</returns>
        public double Assemble(Variable var)
        {
            Ensure.NotNull(var, "var");

            int aID = var.id;
            double coef = 0.0;
            for (int i = 0, n = coefs.Count; i < n; i++)
            {
                Coef c = coefs[i]; // consider ref
                if (aID == c.id) coef += c.coef;
            }
            return coef;
        }

        /// <summary>
        /// Determines the sum of all coefficients of the given variables.
        /// </summary>
        /// <param name="var1">The variable1 to be used.</param>
        /// <param name="var2">The variable2 to be used.</param>
        /// <returns>The sum of all coefficients of the given variables.</returns>
        public double Assemble(Variable var1, Variable var2)
        {
            Ensure.NotNull(var1, "var1");
            Ensure.NotNull(var2, "var2");

            QuadCoef qc = new QuadCoef(var1, var2);

            double coef = 0.0;
            for (int i = 0, qn = quadCoefs.Count; i < qn; i++)
            {
                QuadCoef c = quadCoefs[i]; // consider ref
                if (qc.EqualsVariables(c)) coef += c.coef;
            }
            return coef;
        }
        /// <summary>
        /// Assemble the current Expression.
        /// After Assembling, all variables will appear only once in the list of coefficients.
        /// </summary>
        public void Assemble()
        {
            CoefVector assembled = new CoefVector();
            coefs.Sort();

            for (int i = 0, n = coefs.Count; i < n; )
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

            QuadCoefVector quadAssembled = new QuadCoefVector();
            quadCoefs.Sort();

            for (int i = 0, qn = quadCoefs.Count; i < qn;)
            {
                QuadCoef newc = quadCoefs[i]; // consider ref
                i++;

                while (i < qn)
                {
                    QuadCoef c = quadCoefs[i];
                    //if the newc.id != c.id then break
                    if (!newc.EqualsVariables(c)) break;

                    newc.coef += c.coef;
                    i++;
                }

                quadAssembled.Add(new QuadCoef(newc.var1, newc.var2, newc.coef));
            }

            quadCoefs.Clear();
            quadCoefs.AddRange(quadAssembled);
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
            get { return coefs; }
        }

        /// <summary>
        /// Gets the vector of coefficients and variables of the current Expression.
        /// </summary>
        internal QuadCoefVector QuadCoefficients
        {
            get { return quadCoefs; }
        }

        /// <summary>
        /// Gets the constant of the current Expression.
        /// </summary>
        public double Constant
        {
            get { return constant; }
        }

        /// <summary>
        /// Returns the number of linear coefficients of the current Expression.
        /// </summary>
        public int NumberOfCoefficients
        {
            get { return coefs.Count; }
        }

        /// <summary>
        /// Returns the number of quadratic coefficients of the current Expression.
        /// </summary>
        public int NumberOfQuadCoefficients
        {
            get { return quadCoefs.Count; }
        }

        /// <summary>
        /// Returns true iff there are one or more quadritic terms in the current Expression.
        /// </summary>
        public bool IsQuadratic
        {
            get { return quadCoefs.Count > 0; }
        }
        /// <summary>
        /// Calculates constant plus the product of all coefficients and the Value in the current solution of their variables.
        /// </summary>
        /// <returns>The constant plus the product of all coefficients and the Value in the current solution of their variables.</returns>
        public double Level()
        {
            int n = coefs.Count;
            double level = constant;
            foreach (Coef coef in coefs)
            {
                level += coef.Level();
            }

            foreach (QuadCoef quadcoef in quadCoefs)
            {
                level += quadcoef.Level();
            }

            return level;
        }
        #region Overloaded operators
        // (?) Note that these overloaded operators use implicit conversion from variable and double to expression!

        #region Operator <=
        /// <summary>
        /// Creates a new Constraint "lhs &lt;= rhs".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="rhs">The rigt-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Expression lhs, Expression rhs)
        {
            return new Constraint(lhs, ConstraintType.LE, rhs);
        }

        /// <summary>
        /// Creates a new Constraint "lhs &lt;= c".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Expression lhs, double c)
        {
            return lhs <= new Expression(c);
        }

        /// <summary>
        /// Creates a new Constraint "c &lt;= rhs".
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(double c, Expression rhs)
        {
            return (new Expression(c) <= rhs);
        }

        /// <summary>
        /// Creates a new Constraint "x &lt;= rhs".
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Variable x, Expression rhs)
        {
            return (new Expression(x) <= rhs);
        }

        /// <summary>
        /// Creates a new Constraint "lhs &lt;= x".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator <=(Expression lhs, Variable x)
        {
            return (lhs <= new Expression(x));
        }
        #endregion

        /// <summary>
        /// Creates a new Constraint "lhs &gt;= rhs".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(Expression lhs, Expression rhs)
        {
            return new Constraint(lhs, ConstraintType.GE, rhs);
        }

        /// <summary>
        /// Creates a new Constraint "lhs &gt;= c".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(Expression lhs, double c)
        {
            return (lhs >= new Expression(c));
        }

        /// <summary>
        /// Creates a new Constraint "c &gt;= rhs".
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(double c, Expression rhs)
        {
            return (new Expression(c) >= rhs);
        }

        /// <summary>
        /// Creates a new Constraint "x &gt;= rhs".
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(Variable x, Expression rhs)
        {
            return (new Expression(x) >= rhs);
        }

        /// <summary>
        /// Creates a new Constraint "lhs &gt;= x".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator >=(Expression lhs, Variable x)
        {
            return (lhs >= new Expression(x));
        }

        #region Operator ==
        /// <summary>
        /// Creates a new Constraint "lhs == rhs".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(Expression lhs, Expression rhs)
        {
            return new Constraint(lhs, ConstraintType.EQ, rhs);
        }

        /// <summary>
        /// Creates a new Constraint "lhs == c".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="c">The constant.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(Expression lhs, double c)
        {
            return (lhs == new Expression(c));
        }

        /// <summary>
        /// Creates a new Constraint "c == rhs".
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(double c, Expression rhs)
        {
            return (new Expression(c) == rhs);
        }

        /// <summary>
        /// Creates a new Constraint "x == rhs".
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="rhs">The right-hand-side expression of the new constraint.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(Variable x, Expression rhs)
        {
            return (new Expression(x) == rhs);
        }

        /// <summary>
        /// Creates a new Constraint "lhs == x".
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="x">The variable.</param>
        /// <returns>The new constraint.</returns>
        public static Constraint operator ==(Expression lhs, Variable x)
        {
            return (lhs == new Expression(x));
        }
        #endregion

        #region Operator !=
        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="lhs">The left-hand-side expression.</param>
        /// <param name="rhs">The right-hand-side expression.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(Expression lhs, Expression rhs)
        {
            Ensure.NotSupported();
            return null;
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="lhs">The left-hand-side expression.</param>
        /// <param name="c">The constant.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(Expression lhs, double c)
        {
            Ensure.NotSupported();
            return null;
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="rhs">The right-hand-side expression.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(double c, Expression rhs)
        {
            Ensure.NotSupported();
            return null;
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="rhs">The right-hand-side expression.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(Variable x, Expression rhs)
        {
            Ensure.NotSupported();
            return null;
        }

        /// <summary>
        /// Not supported. Added to prevent error CS0216.
        /// </summary>
        /// <param name="lhs">The left-hand-side expression of the new constraint.</param>
        /// <param name="x">The variable.</param>
        /// <returns>NotSupportedException</returns>
        public static Constraint operator !=(Expression lhs, Variable x)
        {
            Ensure.NotSupported();
            return null;
        }
        #endregion

        #region Operator +
        /// <summary>
        /// Creates a new Expression set to "c + expr"
        /// </summary>
        /// <param name="c">The constant.</param>
        /// <param name="expr">The expression</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(double c, Expression expr)
        {
            return new Expression(c).Add(expr);
        }

        /// <summary>
        /// Creates a new Expression set to "x + expr"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="expr">The expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(Variable x, Expression expr)
        {
            return (new Expression(x)).Add(expr);
        }

        /// <summary>
        /// Creates a new Expression set to "expr + c"
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="c">The constant to be added.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(Expression expr, double c)
        {
            return (new Expression(expr)).Add(c);
        }

        /// <summary>
        /// Creates a new Expression set to "expr + x"
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="x">The variable to be added.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(Expression expr, Variable x)
        {
            return (new Expression(expr)).Add(x);
        }

        /// <summary>
        /// Creates a new Expression set to "expr1 + expr2"
        /// </summary>
        /// <param name="expr1">The left expression</param>
        /// <param name="expr2">The right expression</param>
        /// <returns>The new expression.</returns>
        public static Expression operator +(Expression expr1, Expression expr2)
        {
            return (new Expression(expr1)).Add(expr2);
        }
        #endregion

        #region Operator -
        /// <summary>
        /// Creates a new Expression set to "- expr"
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Expression expr)
        {
            return (new Expression()).Subtract(expr);
        }

        /// <summary>
        /// Creates a new Expression set to "c - expr"
        /// </summary>
        /// <param name="c">The constant to be used.</param>
        /// <param name="expr">The expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(double c, Expression expr)
        {
            return (new Expression(c)).Subtract(expr);
        }

        /// <summary>
        /// Creates a new Expression set to "x - expr"
        /// </summary>
        /// <param name="x">The variable.</param>
        /// <param name="expr">The expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Variable x, Expression expr)
        {
            return (new Expression(x)).Subtract(expr);
        }

        /// <summary>
        /// Creates a new Expression set to "expr - c"
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="c">The constant to be subtracted.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Expression expr, double c)
        {
            return (new Expression(expr)).Subtract(c);
        }

        /// <summary>
        /// Creates a new Expression set to "expr - x"
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="x">The variable to be subtracted.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Expression expr, Variable x)
        {
            return (new Expression(expr)).Subtract(x);
        }

        /// <summary>
        /// Creates a new Expression set to "expr1 - expr2"
        /// </summary>
        /// <param name="expr1">The left expression.</param>
        /// <param name="expr2">The right expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator -(Expression expr1, Expression expr2)
        {
            return (new Expression(expr1)).Subtract(expr2);
        }
        #endregion

        #region Operator * for constants
        /// <summary>
        /// Creates a new expression set to "f * expr".
        /// The multiplier f is applied to the constant and coefficients in the expression.
        /// </summary>
        /// <param name="f">The multiplier.</param>
        /// <param name="expr">The expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(double f, Expression expr)
        {
            return (new Expression()).Add(f, expr);
        }

        /// <summary>
        /// Creates a new expression set to "expr * f" ( = "f * expr").
        /// The multiplier f is applied to the constant and coefficients in the expression.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="f">The multiplier.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(Expression expr, double f)
        {
            return (new Expression()).Add(f, expr);
        }

        /// <summary>
        /// Creates a new expression set to "expr * var" ( = "var * expr").
        /// The given expression must not be quadratic.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="var">The variable.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(Expression expr, Variable var)
        {
            return (new Expression(var)).Multiply(expr);
        }

        /// <summary>
        /// Creates a new expression set to "expr * var" ( = "var * expr").
        /// The given expression must not be quadratic.
        /// </summary>
        /// <param name="var">The variable.</param>
        /// <param name="expr">The expression.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(Variable var, Expression expr)
        {
            return (new Expression(var)).Multiply(expr);
        }

        /// <summary>
        /// Creates a new expression set to "expr1 * expr2".
        /// The given expressions must not be quadratic.
        /// </summary>
        /// <param name="expr1">The expression1.</param>
        /// <param name="expr2">The expression2.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator *(Expression expr1, Expression expr2)
        {
            return (new Expression(expr1)).Multiply(expr2);
        }
        #endregion

        #region Operator / for constant
        /// <summary>
        /// Creates a new expression set to "expr / d".,
        /// The constant and coefficients of the given expression are all divided by d.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <param name="d">The divider to be used. Must be non-zero.</param>
        /// <returns>The new expression.</returns>
        public static Expression operator /(Expression expr, double d)
        {
            return (new Expression(expr)).Divide(d);
        }
        #endregion

        /// <summary>
        /// Returns a new expression that is the sum of the given expressions
        /// </summary>
        /// <param name="expressions">The expressions to be summed up.</param>
        /// <returns>A new expression that is the sum of the given expressions.</returns>
        public static Expression Sum(IEnumerable<Expression> expressions)
        {
            Expression sum = new Expression();
            foreach (Expression expr in expressions)
            {
                sum.Add(expr);
            }
            return sum;
        }

        /// <summary>
        /// Returns a new expression that is the sum of the given variables.
        /// </summary>
        /// <param name="variables">The variables to be summed up.</param>
        /// <returns>A new expression that is the sum of the given variables.</returns>
        public static Expression Sum(IEnumerable<Variable> variables)
        {
            Expression sum = new Expression();
            foreach (Variable var in variables)
            {
                sum.Add(var);
            }
            return sum;
        }

        /// <summary>
        /// Returns an expression that is the scalar product of the array of coefficients and array of variables: 
        ///   sum_i { coefs_i * variables_i }
        /// Note that the number of coefficients and variables must be equal.
        /// </summary>
        /// <param name="coefs">The coefficients to be used.</param>
        /// <param name="variables">The variables to be used.</param>
        /// <returns>The new expression.</returns>
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

        /// <summary>
        /// Returns an expression that is the scalar product of the coefficients and variables: 
        ///   sum_i { coefs_i * variables_i }
        /// Note that the number of coefficients and variables must be equal.
        /// </summary>
        /// <param name="coefs">The coefficients to be used.</param>
        /// <param name="variables">The variables to be used.</param>
        /// <returns>The new expression.</returns>
        public static Expression ScalarProduct(IEnumerable<double> coefs, IEnumerable<Variable> variables)
        {
            Expression sum = new Expression();
            IEnumerator<Variable> varEnumerator = variables.GetEnumerator();
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

        private readonly CoefVector coefs;
        private readonly QuadCoefVector quadCoefs;
        private double constant;
    }
}