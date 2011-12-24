// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// Provides a set of static methods for creating expressions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns a new expression that is the sum of the given expressions.
        /// </summary>
        /// <param name="expressions">The expressions to be summed up.</param>
        /// <returns>New expression that is the sum of the given expressions.</returns>
        public static Expression Sum(this IEnumerable<Expression> expressions)
        {
            return Expression.Sum(expressions);
        }

        /// <summary>
        /// Computes an expression which is the sum of the sequence of Variables that are obtained
        /// by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that is used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element which returns a Variable.</param>
        /// <returns>The sum of the projected variables.</returns>
        public static Expression Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Variable> selector)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(selector, "selector");

            return source.Select(selector).Sum();
        }

        /// <summary>
        /// Computes an expression which is the sum of the sequence of Expressions that are obtained
        /// by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that is used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element which returns an Expression.</param>
        /// <returns>The sum of the projected variables.</returns>
        public static Expression Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Expression> selector)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(selector, "selector");

            return source.Select(selector).Sum();
        }

        /// <summary>
        /// Returns a new expression that is the sum of the given variables.
        /// </summary>
        /// <typeparam name="TKey">This type is ignored.</typeparam>
        /// <param name="dictionary">The dictionary with variables to be summed up.</param>
        /// <returns>New expression that is the sum of the given variables.</returns>
        public static Expression Sum<TKey>(this System.Collections.Generic.IDictionary<TKey, Variable> dictionary)
        {
            return Expression.Sum(dictionary.Values);
        }

        /// <summary>
        /// Returns a new expression that is the sum of the given expressions.
        /// </summary>
        /// <param name="expressions">The expressions to be summed up.</param>
        /// <returns>New expression that is the sum of the given expressions.</returns>
        [Obsolete("Deprecated: Use type-safe alternative", true)]
        public static Expression Sum(this System.Collections.IEnumerable expressions)
        {
            return Expression.Sum((IEnumerable<Expression>) expressions);
        }

        /// <summary>
        /// Returns a new expression that is the sum of the given variables.
        /// </summary>
        /// <param name="variables">The variables to be summed up.</param>
        /// <returns>New expression that is the sum of the given variables.</returns>
        public static Expression Sum(this IEnumerable<Variable> variables)
        {
            return Expression.Sum(variables);
        }

        /// <summary>
        /// Returns an expression that is the scalar product of the coefficients and variables: 
        ///   sum_i { coefs_i * variables_i }
        /// Note that the number of coefficients and variables must be equal.
        /// </summary>
        /// <param name="variables">The variables</param>
        /// <param name="coefs">The array of coefficients</param>
        /// <returns>New expression that is the scalar product of the coefficients and variables.</returns>
        public static Expression ScalarProduct(this System.Collections.Generic.IList<Variable> variables, double[] coefs)
        {
            return Expression.ScalarProduct(coefs, variables);
        }

        /// <summary>
        /// Returns an expression that is the scalar product of the coefficients and variables: 
        ///   sum_i { coefs_i * variables_i }
        /// Note that the number of coefficients and variables must be equal.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="coefs">The array of coefficients.</param>
        /// <returns>New expression that is the scalar product of the coefficients and variables.</returns>
        public static Expression ScalarProduct(this Variable[] variables, double[] coefs)
        {
            return Expression.ScalarProduct(coefs, variables);
        }

        /// <summary>
        /// Returns an expression that is the scalar product of the coefficients and variables: 
        ///   sum_i { coefs_i * variables_i }
        /// Note that the number of coefficients and variables must be equal.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="coefs">The array of coefficients.</param>
        /// <returns>New expression that is the scalar product of the coefficients and variables.</returns>
        public static Expression ScalarProduct(IEnumerable<Variable> variables, IEnumerable<double> coefs)
        {
            return Expression.ScalarProduct(coefs, variables);
        }

    }
}
