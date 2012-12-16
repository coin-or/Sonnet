// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#ifndef CoefVector_H
#define CoefVector_H

#include <string>

namespace Sonnet
{
    struct Coef
    {
	public:
        Coef(Variable &aVar, double aCoef)
		{
			//Ensure.NotNull(aVar, "variable of coef");

            var = aVar;
            id = var.id;
            coef = aCoef;

			//#if (DEBUG)
			//	if (double.IsNaN(coef)) throw new SonnetException("The value of the coefficient of variable " + var.Name + " is not a number! (NaN)");
			//#endif
		}

		std::string ToString()
		{
            std::string tmp;
            //std::string varName = var.Name;

            //if (coef == 1.0) tmp.AppendFormat("{0}", varName);
            //else if (coef == -1.0) tmp.AppendFormat("- {0}", varName);
            //else if (coef < 0.0) tmp.AppendFormat("- {0} {1}", -coef, varName);
            //else tmp.AppendFormat("{0} {1}", coef, varName);

            //// note that a coef of -2.5 for x2 results in     "-2.5 x2", whereas in an expression
            //// we might prefer ".... - 2.5 x2 ..." 

            //return tmp.ToString();
		}

        bool Equals(Coef &rhs)
        {
            return id == rhs.id && coef == rhs.coef;
        }

        int CompareTo(Coef& coef)
        {
            return id.CompareTo(coef.id);
        }

        double Level()
        {
            return coef * (var.Value);
        }

        Coef & Multiply(double multiplier)
        {
            coef *= multiplier;
            return this;
        }
        
		Coef & Divide(double divider)
        {
            coef /= divider;
            return this;
        }

        Variable &var;
        int id;
        double coef;
    }

    class CoefVector : std::vector<Coef>
    {
	public:
        void Remove(int index)
        {
			//TODO
            //InternalUtils.Remove<Coef>(this, index);
        }

        std::string ToString()
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

#endif
