// Copyright (C) Jan-Willem Goossens 
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
        // The following extension are all lazily executed, so only when the returned set is enumerated!
        // If the resulting set is not enumerated, then nothing is done!!
        #region Lazy Sets of Constraints -- Define ForAll projections from source set to constraints
        /// <summary>
        /// Returns a lazy enumerable full of constraints generated from the source set.
        /// Example: lengths.ForAll(i => x[i] >= -M * (1 - n[i]))
        /// Example: lengths.ForAll(i => new Constraint(x[i] >= -M * (1 - n[i])) { Name = "Limit_" + i });  // copies the constraint unnecessarily
        /// Example: lengths.ForAll(i => (x[i] >= -M * (1 - n[i])).WithName("Limit_" + i)); // doesnt copy the constraint, but uglier
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that is used.</param>
        /// <param name="selector">A transform function to apply to each element and which returns a Constraint.</param>
        /// <returns>An enumerable full of constraints generated from the source set.</returns>
        public static IEnumerable<Constraint> ForAll<TSource>(this IEnumerable<TSource> source, Func<TSource, Constraint> selector)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(selector, "selector");

            return source.Select(selector);
        }

        /// <summary>
        /// Returns a lazy (!) enumerable full of constraints generated from the source set, each with the name as "name_" + i.ToString()
        /// Example: model.Add( lengths.ForAll("limit", i => x[i] >= -M * (1 - n[i])) );
        /// For nested loops, provide inner loops with name = "".
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that is used.</param>
        /// <param name="name">The base name to be used for the constraints. Use "" to concat index "_i"</param>
        /// <param name="selector">A transform function to apply to each element and which returns a Constraint.</param>
        /// <returns>An enumerable full of constraints generated from the source set with set names.</returns>
        public static IEnumerable<Constraint> ForAll<TSource>(this IEnumerable<TSource> source, string name, Func<TSource, Constraint> selector)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(selector, "selector");

            Func<TSource, Constraint> nameSelector = (i =>
            {
                Constraint c = selector(i);
                if (c.Name.StartsWith("Con_")) c.Name = name + "_" + i.ToString();
                else c.Name = name + "_" + i.ToString() + c.Name;
                return c;
            });

            return ForAll(source, nameSelector);
        }


        /// <summary>
        /// Returns a lazy (!) enumerable full of constraints genereted from the source set, for all but the inner-most of compounded ForAlls.
        /// Example: model.Add( products.Where(p => p.IsCar).ForAll(p => lengths.ForAll(l => x[p] >= y[j])) );
        /// Example: model.Add( products.ForAll(p => lengths.ForAll(l => colors.ForAll(c => x[p] >= y[j] + z[c]))) );
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that is used.</param>
        /// <param name="selector">A transform function to apply to each element and which returns a set of Constraints.</param>
        /// <returns>An enumerable full of constraints generated from the source set with set names.</returns>
        public static IEnumerable<Constraint> ForAll<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<Constraint>> selector)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(selector, "selector");

            return source.SelectMany(selector);
        }

        /// <summary>
        /// Returns a lazy (!) enumerable full of constraints genereted from the source set, each with the name as "name_" + i.ToString().
        /// Set name at outer-most ForAll, and use "" for innner ForAlls to concat the indexes for Constraint names to "MinProdAndColor_p_l_c", etc.
        /// Example: model.Add( products.Where(p => p.IsCar).ForAll("MinProd", p => lengths.ForAll("", l => x[p] >= y[j])) );
        /// Example: model.Add( products.ForAll("MinProdAndColor", p => lengths.ForAll("", l => colors.ForAll("", c => x[p] >= y[j] + z[c]))) );
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="name">The base name to be used for the constraints. Use "" to concat index "_i"</param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<Constraint> ForAll<TSource>(this IEnumerable<TSource> source, string name, Func<TSource, IEnumerable<Constraint>> selector)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(name, "name");
            Ensure.NotNull(selector, "selector");

            Func<TSource, IEnumerable<Constraint>> nameSelector = (i =>
            {
                IEnumerable<Constraint> constraints = selector(i);
                return constraints.Select(c =>
                {
                    if (c.Name.StartsWith("Con_")) c.Name = name + "_" + i.ToString();
                    else c.Name = name + "_" + i.ToString() + c.Name;
                    return c;
                });
            });

            return source.ForAll(nameSelector);
        }
        #endregion

        /// <summary>
        /// Returns a dictionary from the source elements to the keys.
        /// Example: Dictionary &lt;string, Variable&gt; x = products.ToMap(p => new Variable() { Name = "x_" + p.ToString(); });
        /// Example: Dictionary &lt;string, Dictionary &lt;int, Variable&gt;&gt; y = products.ToMap(p => colors.ToMap(c => new Variable() { Name = "y_" + p + "," + c})); }
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">Enumerable of source element</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <returns>Dictionary from the source elements to the keys.</returns>
        public static Dictionary<TSource, TKey> ToMap<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            Dictionary<TSource, TKey> dict = new Dictionary<TSource, TKey>();
            foreach(var i in source)
            {
                dict.Add(i, keySelector(i));
            }
            return dict;
        }

        /// <summary>
        /// For each item in the source enumerable, perform the given action.
        /// Example: products.ForAllDo(p => Console.WriteLine("Product: " + p));
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">Enumerable of source element</param>
        /// <param name="action">The action to be performed for each.</param>
        public static void ForAllDo<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            Ensure.NotNull(source, "source");
            Ensure.NotNull(action, "action");

            foreach (var s in source) action(s);
        }

        /// <summary>
        /// Return the source object but now with Name set.
        /// This allows for immediate name assignment, such as Constraint con = (x &lt;= 10).WithName("hello");
        /// </summary>
        /// <typeparam name="TSource">The type of source</typeparam>
        /// <param name="source">The source element, derived of Named</param>
        /// <param name="name">The name to be set to the source.</param>
        /// <returns>The source object</returns>
        public static TSource WithName<TSource>(this TSource source, string name)
            where TSource : Named
        {
            source.Name = name;
            return source;
        }

        /// <summary>
        /// Returns a string with on each line one element
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The source set.</param>
        /// <returns>The string.</returns>
        public static string ToItemString<TSource>(this IEnumerable<TSource> source)
        {
            StringBuilder tmp = new StringBuilder();
            return source.ToItemString(tmp).ToString();
        }

        /// <summary>
        /// Returns a string with on each line one element
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The source set.</param>
        /// <param name="builder">A given StringBuilder object to append to.</param>
        /// <returns>The builder object</returns>
        public static StringBuilder ToItemString<TSource>(this IEnumerable<TSource> source, StringBuilder builder)
        {
            foreach (var s in source)
            {
                var enumerable = s as System.Collections.IEnumerable;
                if (enumerable != null) enumerable.ToItemString(builder);
                else builder.AppendLine(s.ToString());
            }
            return builder;
        }

        private static StringBuilder ToItemString(this System.Collections.IEnumerable source, StringBuilder builder)
        {
            foreach (var s in source)
            {
                var enumerable = s as System.Collections.IEnumerable;
                if (enumerable != null) enumerable.ToItemString(builder);
                else builder.AppendLine(s.ToString());
            }
            return builder;
        }

        /// <summary>
        /// Returns the first variable from the enumerable for which the Name strign Equals the given name. 
        /// This is case sensitive.
        /// </summary>
        /// <param name="variables">The variables to search.</param>
        /// <param name="name">The name of the variable to look for.</param>
        /// <returns>The first variable from the enumerable for which the Name strign Equals the given name.</returns>
        public static Variable GetVariable(this IEnumerable<Variable> variables, string name)
        {
            Ensure.NotNull(variables, "variables");
            Ensure.NotNull(name, "name");
            return variables.First(v => string.Equals(v.Name, name));
        }

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
        /// Example:  Expression expr = Days.Sum(d => d.x) + Methods.Sum(m => m.y);
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
        /// Example:  Expression expr = Methods.Sum(m => m.Cost * m.y + 25.0);
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

        /// <summary>
        /// Gets the current bound for MIP.
        /// If the current solution is Optimal, then Bound equals objective Value.
        /// If not yet optimal, then Bound is best relaxation bound of all nodes left on the search tree.
        /// This value is not available for all solvers.
        /// </summary>
        /// <param name="solver">The solver. Only available for OsiCbc.</param>
        /// <returns>The current bound</returns>
        internal static double Bound(this COIN.OsiSolverInterface solver)
        {
            Ensure.NotNull(solver, "solver");

            if (solver.isProvenOptimal())
            {
                return solver.getObjValue();
            }

            // not optimal, so depends per solver
            if (solver is COIN.OsiCbcSolverInterface osiCbc)
            {
                return osiCbc.Model.getBestPossibleObjValue();
            }

            throw new NotImplementedException($"Bound is not implemented for OsiSolverInterface type {solver.GetType()}");
        }
    }
}
