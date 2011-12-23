// Copyright (C) 2011, Jan-Willem Goossens 
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
            InternalUtils.Remove<Coef>(this, index);
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
}