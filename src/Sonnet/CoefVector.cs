// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    internal struct Coef : IComparable<Coef>
    {
        public Coef(Variable aVar, double aCoef)
        {
            Ensure.NotNull(aVar, "variable of coef");
            Ensure.IsFalse(double.IsNaN(aCoef), $"The value of the coefficient of variable {aVar.Name} is not a number! (NaN)");

            var = aVar;
            id = var.id;
            coef = aCoef;
        }

        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
            string varName = var.Name;

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
            if (obj is Coef rhs) return Equals(rhs);
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

        public int CompareTo(Coef other)
        {
            return id.CompareTo(other.id);
        }

        public static bool operator ==(Coef left, Coef right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator >(Coef left, Coef right)
        {
            return left.CompareTo(right) > 0; // check this!
        }
        public static bool operator >=(Coef left, Coef right)
        {
            return left.CompareTo(right) >= 0; // check this!
        }

        public static bool operator <(Coef left, Coef right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(Coef left, Coef right)
        {
            return left.CompareTo(right) <= 0; // check this!
        }

        public static bool operator != (Coef left, Coef right)
        {
            return !(left == right);
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
        public readonly int id;
        public double coef;
    }

    internal struct QuadCoef : IComparable<QuadCoef>
    {
        public QuadCoef(Variable var1, Variable var2)
            : this(var1, var2, 1.0)
        {
        }

        public QuadCoef(Variable var1, Variable var2, double coef)
        {
            Ensure.NotNull(var1, "variable1 of coef");
            Ensure.NotNull(var2, "variable2 of coef");
            Ensure.IsFalse(double.IsNaN(coef), "coef is not a number (NaN)"); 

            if (var1.id <= var2.id)
            {
                this.var1 = var1;
                this.id1 = var1.id;
                this.var2 = var2;
                this.id2 = var2.id;
            }
            else
            {
                this.var1 = var2;
                this.id1 = var2.id;
                this.var2 = var1;
                this.id2 = var1.id;
            }
            
            this.coef = coef;
        }

        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
            string varsName;
            if (id1 == id2) varsName = var1.Name + "^2";
            else varsName = var1.Name + var2.Name;

            if (coef == 1.0) tmp.AppendFormat("{0}", varsName);
            else if (coef == -1.0) tmp.AppendFormat("- {0}", varsName);
            else if (coef < 0.0) tmp.AppendFormat("- {0} {1}", -coef, varsName);
            else tmp.AppendFormat("{0} {1}", coef, varsName);

            // note that a coef of -2.5 for x2 results in     "-2.5 x2", whereas in an expression
            // we might prefer ".... - 2.5 x2 ..." 

            return tmp.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is QuadCoef rhs) return Equals(rhs);
            return false;
        }

        public bool Equals(QuadCoef rhs)
        {
            return id1 == rhs.id1 && id2 == rhs.id2 && coef == rhs.coef;
        }

        public bool EqualsVariables(QuadCoef rhs)
        {
            return id1 == rhs.id1 && id2 == rhs.id2;
        }

        public override int GetHashCode()
        {
            return id1 ^ id2;
        }

        public int CompareTo(QuadCoef other)
        {
            int id1compare = id1.CompareTo(other.id1);
            if (id1compare != 0) return id1compare;
            return id2.CompareTo(other.id2);
        }

        public static bool operator ==(QuadCoef left, QuadCoef right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator >(QuadCoef left, QuadCoef right)
        {
            return left.CompareTo(right) > 0; // check this!
        }

        public static bool operator >=(QuadCoef left, QuadCoef right)
        {
            return left.CompareTo(right) >= 0; // check this!
        }

        public static bool operator <(QuadCoef left, QuadCoef right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(QuadCoef left, QuadCoef right)
        {
            return left.CompareTo(right) <= 0; // check this!
        }

        public static bool operator !=(QuadCoef left, QuadCoef right)
        {
            return !(left == right);
        }

        public double Level()
        {
            return coef * var1.Value * var2.Value;
        }

        public QuadCoef Multiply(double multiplier)
        {
            coef *= multiplier;
            return this;
        }
        public QuadCoef Divide(double divider)
        {
            coef /= divider;
            return this;
        }

        public Variable var1;
        public readonly int id1;
        public Variable var2;
        public readonly int id2;
        public double coef;
    }

    internal class CoefVector : List<Coef>
    {
        public void Remove(int index)
        {
            InternalUtils.Remove<Coef>(this, index);
        }

        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
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

    internal class QuadCoefVector : List<QuadCoef>
    {
        public void Remove(int index)
        {
            InternalUtils.Remove<QuadCoef>(this, index);
        }
        
        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
            int n = this.Count;
            for (int i = 0; i < n; i++)
            {
                QuadCoef c = this[i];
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
}