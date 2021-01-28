// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Sonnet
{
    /// <summary>
    /// This class implements various static methods
    /// </summary>
    internal static class MathUtils
    {
        public const double Infinity = double.MaxValue;
        public const double Epsilon = 1e-5;
        /// <summary>
        /// Compares this double to the given value.
        /// Returns -1 if b is larger, 1 if b is smaller, and 0 otherwise.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double.</param>
        /// <param name="b">The given double.</param>
        /// <returns>-1 if this double is smaller than b - eps,
        /// 1 if a is larget than b + eps, and 0 otherwise.
        /// </returns>
        public static int CompareToEps(this double a, double b)
        {
            double eps = Epsilon;
            // if either a or b is close to zero, then do absolute difference
            // otherwise (the 'if' here) do a relative comparison (relative to the absolute value of the highest of a and b)
            if (System.Math.Abs(a) >= Epsilon && System.Math.Abs(b) >= Epsilon)
                eps *= System.Math.Abs(System.Math.Max(a, b));

            if (a < b - eps)
                return -1;
            if (a > b + eps)
                return 1;
            return 0;
        }

        /// <summary>
        /// Determines whether this double is positive.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double</param>
        /// <returns>True iff this double is larger than zero.</returns>
        public static bool IsPositive(this double a)
        {
            return a.CompareToEps(0.0) > 0;
        }

        /// <summary>
        /// Determines whether this double is negative.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double</param>
        /// <returns>True iff this double is smaller than zero.</returns>
        public static bool IsNegative(this double a)
        {
            return a.CompareToEps(0.0) < 0;
        }

        /// <summary>
        /// Determines whether this double is equal to or between the given two bounds.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="x">This double.</param>
        /// <param name="l">the lowwer bound.</param>
        /// <param name="u">The upper bound.</param>
        /// <returns>True iff this double is equal to or between the two bounds.</returns>
        public static bool IsBetween(this double x, double l, double u)
        {
            if (x.CompareToEps(l) >= 0 && x.CompareToEps(u) <= 0) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this double is equal to zero.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double.</param>
        /// <returns>True iff this double is equal to zero.</returns>
        public static bool IsZero(this double a)
        {
            return a.CompareToEps(0.0) == 0;
        }

        /// <summary>
        /// Determines whether this double is integer.
        /// This method uses Utils.Epsilon.
        /// </summary>
        /// <param name="a">This double</param>
        /// <returns>true if this double is integer, and false otherwise.</returns>
        public static bool IsInteger(this double a)
        {
            return (System.Math.Round(a) - a).IsZero();
        }

        /// <summary>
        /// Creates a string representation of this double.
        /// Uses "-Inf" and "Inf" whenever applicatable.
        /// </summary>
        /// <param name="a">This double.</param>
        /// <returns>"-Inf" if a is less than or equal to Utils.Infinity, "Inf" is larger or equal, and 
        /// regular a.ToString() otherwise.</returns>
        public static string ToDoubleString(this double a)
        {
            if (a <= -Infinity) return "-Inf";
            if (a >= Infinity) return "Inf";

            return a.ToString();
        }
    }

    /// <summary>
    /// This static class contains extension methods.
    /// </summary>
    public static class Utils
    {
        private static readonly Dictionary<COIN.OsiCbcSolverInterface, string[]> cbcSolverArgs = new Dictionary<COIN.OsiCbcSolverInterface, string[]>();
        
        /// <summary>
        /// Returns the array of arguments to be used when solving using an instance of OsiCbcSolverInterface.
        /// Returns an empty array if no arguments were found.
        /// See also Sonnet.Solve(..)
        /// </summary>
        /// <param name="solver">The OsiCbcSolverInterface instance.</param>
        /// <returns>The arguments for CbcMain1(..).</returns>
        public static string[] GetCbcSolverArgs(this COIN.OsiCbcSolverInterface solver)
        {
            if (cbcSolverArgs.TryGetValue(solver, out string[] result)) return result;
            else return new string[0];
        }

        /// <summary>
        /// Sets the array of arguments to be used when solving using an instance of OsiCbcSolverInterface.
        /// See also Sonnet.Solve(..)
        /// </summary>
        /// <param name="solver">The OsiCbcSolverInterface instance.</param>
        /// <param name="args">The arguments for CbcMain1(..).</param>
        public static void SetCbcSolverArgs(this COIN.OsiCbcSolverInterface solver, params string []args)
        {
            cbcSolverArgs[solver] = args;
        }

    }

    internal static class InternalUtils
    {
        public static string GetAssemblyInfo()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Assembly information:");
            message.AppendLine("File path: " + System.Reflection.Assembly.GetExecutingAssembly().Location);
            message.AppendLine("File date: " + System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString());
            message.AppendLine(System.Reflection.Assembly.GetExecutingAssembly().FullName);
            message.AppendLine("Framework: " + Environment.Version.ToString());
            message.AppendLine("Assembly runtime version: " + System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion);

            System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.GetPEKind(out System.Reflection.PortableExecutableKinds portableExecutableKinds, out System.Reflection.ImageFileMachine imageFileMachine);
            message.AppendLine("Portable executable kinds: " + portableExecutableKinds.ToString());
            message.AppendLine("Image file machine: " + imageFileMachine.ToString());

            return message.ToString();
        }

        public static char GetOsiConstraintType(this ConstraintType constraintType)
        {
            switch (constraintType)
            {
                case ConstraintType.EQ: return 'E';
                case ConstraintType.LE: return 'L';
                case ConstraintType.GE: return 'G';
                default: throw new SonnetException("Unknown constraint type " + constraintType);
            }
        }

        /// <summary>
        /// Returns true iff exactly one argument is set for this solver using SetCbcMainArgs, 
        /// and that argument is "-branchAndBound".
        /// </summary>
        /// <param name="solver"></param>
        /// <returns></returns>
        public static bool UseBranchAndBound(this COIN.OsiCbcSolverInterface solver)
        {
            string[] args = solver.GetCbcSolverArgs();
            if (args.Length == 1 && string.Equals(args[0], "-branchAndBound", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return false;

        }

        public static void Remove<T>(this List<T> list, int index)
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

    /// <summary>
    /// The Ensure class contains various tests that will throw an exception if a condition is not met.
    /// For example:
    ///     Ensure.IsTrue(a == 5);
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Throws an ArgumentException if b is not true.
        /// </summary>
        /// <param name="b">The boolean value to be tested.</param>
        /// <param name="message">The message.</param>
        public static void IsTrue(bool b, string message = null)
        {
            if (!b)
            {
                if (message != null) throw new ArgumentException(message);
                else throw new ArgumentException("The value is not true");
            }
        }

        /// <summary>
        /// Throws an ArgumentException if b is not false.
        /// </summary>
        /// <param name="b">The boolean value to be tested.</param>
        /// <param name="message">The message.</param>
        public static void IsFalse(bool b, string message = null)
        {
            if (b)
            {
                if (message != null) throw new ArgumentException(message);
                else throw new ArgumentException("The value is not false");     
            }
        }

        /// <summary>
        /// Throws and ArgumentOutOfRangeException is the given type is not derived from the generic type.
        /// Uses derived.IsSubclassOf.
        /// </summary>
        /// <typeparam name="Base">The base type</typeparam>
        /// <param name="derived">The derived type</param>
        /// <param name="paramName">The given parameter name to be reported.</param>
        public static void Is<Base>(Type derived, string paramName = null)
        {
            if (derived is null)
            {
                string message = $"Given type argument is null but should be derived from type {typeof(Base).Name}";
                if (paramName is null) throw new ArgumentNullException(message);
                else throw new ArgumentNullException(paramName, message);                  
            }
            if (!derived.IsSubclassOf(typeof(Base)))
            {
                string message = string.Format("Type {0} is not derived from type {1}", derived.Name, typeof(Base).Name);
                if (paramName != null) throw new ArgumentOutOfRangeException(paramName, message);
                else throw new ArgumentOutOfRangeException(message, (Exception)null);
            }
        }
        /// <summary>
        /// Throws an ArgumentException if a and b are not equal.
        /// </summary>
        /// <typeparam name="T">The type of objects.</typeparam>
        /// <param name="a">The object to compare.</param>
        /// <param name="b">The object to compare to.</param>
        public static void Equals<T>(IEquatable<T> a, IEquatable<T> b)
        {
            if (!a.Equals(b)) throw new ArgumentException("The objects are not equal");
        }


        /// <summary>
        /// Throws a NotSupportedException (always), with the given message.
        /// </summary>
        /// <param name="message">The message to be included</param>
        /// <param name="args">Any parameters that would otherwise be unused.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This is NotSupported, so disregard the args.")]
        public static void NotSupported(string message = null, params object[] args)
        {
            // This is mostly used to prevent "Exceptions should not be thrown from unexpected methods" since
            // Sonnet used operator overloading, which is normally considered an unexpected method for throwing 
            // exceptions.
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Throws an ArgumentNullException if the object is null.
        /// </summary>
        /// <param name="obj">The given object.</param>
        /// <param name="paramName">The given parameter name to be reported.</param>
        public static void NotNull(object obj, string paramName = null)
        {
            if (obj is null)
            {
                if (paramName != null) throw new ArgumentNullException(paramName);
                else throw new ArgumentNullException("obj", "object cannot be null, but is.");
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException for the given parameter name if the object is null or only spaces.
        /// </summary>
        /// <param name="value">The given string value.</param>
        /// <param name="paramName">The given parameter name to be reported.</param>
        public static void NotNullOrWhiteSpace(string value, string paramName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (paramName != null) throw new ArgumentNullException(paramName);
                else throw new ArgumentNullException("value", "Value cannot be null or whitespace, but is.");
            }
        }
    }
}