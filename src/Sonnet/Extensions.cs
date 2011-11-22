// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sonnet
{
    public static class Extensions
    {
        // Some mathematical functions
        // returns a new expression that is the sum of the given expressions
        public static Expression Sum(this IEnumerable<Expression> expressions)
        {
            return Expression.Sum(expressions);
        }

        // returns a new expression that is the sum of the given *expressions*
        public static Expression Sum(this System.Collections.IEnumerable expressions)
        {
            return Expression.Sum(expressions);
        }


        // returns a new expression that is the sum of the given variables
        public static Expression Sum(this IEnumerable<Variable> variables)
        {
            return Expression.Sum(variables);
        }

        public static Expression ScalarProduct(this System.Collections.Generic.IList<Variable> variables, double[] coefs)
        {
            return Expression.ScalarProduct(coefs, variables);
        }

        public static Expression ScalarProduct(this Variable[] variables, double[] coefs)
        {
            return Expression.ScalarProduct(coefs, variables);
        }

        public static Expression ScalarProduct(IEnumerable<Variable> vars, IEnumerable<double> coefs)
        {
            return Expression.ScalarProduct(coefs, vars);
        }

    }
}
