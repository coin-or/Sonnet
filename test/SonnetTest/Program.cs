// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sonnet;

namespace SonnetTest
{
    [TestClass]
    public static class Program
    {

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Executes once before the test run. (Optional)
            // Change to Invariant Culture, mainly for decimal point etc.
            // Many of the tests do string comparing with "." as decimal symbol obtained in InvariantCulture
            // So tests must be run in InvariantCulture to pass.
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// This is a very rudimentary way to run the test self-contained without framework.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            double startMemoryGb = Utils.AvailableMemoryGb;
            {
                AssemblyInit(null);

                Assembly assembly = typeof(Program).Assembly;
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes(typeof(TestClassAttribute), false).Length > 0)
                    .ToArray();
                foreach (var testType in types)
                {
                    // Use the following line to only run one TestClass
                    //if (testType != typeof(Sonnet_CbcTests)) continue;
                    //if (testType == typeof(Sonnet_StressTests)) continue;
                    
                    object testInstance = null;
                    var methods = testType.GetMethods()
                            .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
                            .ToArray();

                    // If there are any TestMethods found, then create an instance of the TestClass
                    if (methods.Any()) testInstance = assembly.CreateInstance(testType.FullName);
                    foreach (var method in methods)
                    {
                        // This Program.cs is ONLY used for debugging
                        // Automated tests do not use Program.cs
                        //if (method.Name != nameof(Sonnet_QuadraticTests.SonnetTestQuad3)) continue;

                        var p = method.GetParameters();
                        if (p.Length == 0)
                        {
                            Console.WriteLine($"Starting test {method.Name} ()");
                            method.Invoke(testInstance, null);
                            Console.WriteLine($"Finished test {method.Name}: Passed");
                        }
                        else if (p.Length == 1)
                        {
                            Console.WriteLine($"Starting test {method.Name}");
                            var dynamicDatas = method.GetCustomAttributes(typeof(DynamicDataAttribute), false);
                            Assert.IsTrue(dynamicDatas.Length == 1, "Found more than one DynamicData attributes");
                            if (dynamicDatas.Length == 1)
                            {
                                var dynamicData = (DynamicDataAttribute)dynamicDatas[0];
                                var datas = dynamicData.GetData(method);
                                // If throw System.ArgumentNullException: Value cannot be null. Parameter name: Property TestSolverTypes
                                // then check that the [DynamicData(nameof(MyProperty), typeof(MyClass))] has the correct MyClass mentioned

                                foreach (var data in datas)
                                {
                                    Console.WriteLine($"Starting test {method.Name} ({string.Join(",", data)})");
                                    method.Invoke(testInstance, data);
                                    Console.WriteLine($"Finished test {method.Name} ({string.Join(",", data)}): Passed");
                                }
                            }
                            Console.WriteLine($"Finished test {method.Name}: Passed");
                        }

                    }
                }
            }

            System.GC.Collect(); // Force Gc.Collect to ensure that indeed all memory is properly freed.

            double endMemoryGb = Utils.AvailableMemoryGb;
            Console.WriteLine("Rudimentary memory leak check:");
            Console.WriteLine($"Available Memory at the start of testing: {startMemoryGb} (GB)");
            Console.WriteLine($"Available Memory at the end of testing: {endMemoryGb} (GB)");
        }
    }
    public static class Utils
    {
        public static IEnumerable<object[]> TestSolverTypes
        {
            get
            {
                //return Solver.GetOsiSolverTypes().Select(t => new object[] { t }).ToArray();
                return new[]
                {
                    new object[] { typeof(COIN.OsiCbcSolverInterface) },
                    new object[] { typeof(COIN.OsiClpSolverInterface) }
                };
            }
        }

        internal static System.Reflection.FieldInfo GetNumberOfVariablesOfVariableClass()
        {
            return typeof(Variable).GetField("numberOfVariables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }

        internal static System.Reflection.FieldInfo GetNumberOfConstraintsOfConstraintClass()
        {
            return typeof(Constraint).GetField("numberOfConstraints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }

        internal static System.Reflection.FieldInfo GetNumberOfObjectivesOfObjectiveClass()
        {
            return typeof(Objective).GetField("numberOfObjectives", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }
        public static double AvailableMemoryGb
        {
            get
            {
#if NETCOREAPP
                #warning "Sonnet: TODO Implement AvailableMemoryGb for .NET Core."
                return 1.0;
#else
                var pc = new Microsoft.VisualBasic.Devices.ComputerInfo();
                //double memoryGB = (pc.TotalPhysicalMemory) / 1073741824.0; // totalvirtualmemory returns faaar too much
                double memoryGB = (pc.AvailablePhysicalMemory) / 1073741824.0;
                return memoryGB;
#endif
            }
        }

        public static double ProcessMemoryGb
        {
            get
            {
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                double totalGbMemoryUsed = currentProcess.WorkingSet64 / 1073741824.0;
                return totalGbMemoryUsed;
            }
        }

        public static string FindBinParent(string directory)
        {
            if (directory == null || directory.Length == 0) return string.Empty;

            string parent = System.IO.Path.GetDirectoryName(directory);
            if (parent == null || parent.Length == 0) return string.Empty;

#if (!VS2003) // must be defined if used in VS2003
            if (parent.EndsWith("bin", StringComparison.CurrentCultureIgnoreCase)) return parent;
#else
			if (parent.EndsWith("bin")) return parent;
#endif
            else return FindBinParent(parent);
        }

        /// <summary>
        /// Returns true if strings are equal.
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <returns></returns>
        public static bool EqualsString(string string1, string string2)
        {
            int n1 = string1.Length;
            int n2 = string2.Length;

            int n = Math.Min(n1, n2);
            for (int i = 0; i < n; i++)
            {
                if (!string1[i].Equals(string2[i]))
                {
                    int j = Math.Min(n - i - 1, 20);
                    System.Diagnostics.Debug.WriteLine("The final few characters are not the same:");
                    System.Diagnostics.Debug.WriteLine("first : " + string1.Substring(0, i + j));
                    System.Diagnostics.Debug.WriteLine("second: " + string2.Substring(0, i + j));
                    return false;
                }
            }

            if (n1 != n2) return false;
            return true;
        }

        public static double Epsilon
        {
            get { return 1e-5; }
        }

        /// <summary>
        /// Compares a and b using the given Epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>0 if a EQ b, or -1 if a LT b, or 1 if a GT b</returns>
        public static int CompareDouble(double a, double b)
        {
            if (a < b - Utils.Epsilon) return -1;
            if (a > b + Utils.Epsilon) return 1;

            return 0;
        }

        public static bool EqualsDouble(double a, double b)
        {
            return CompareDouble(a, b) == 0;
        }
    }

}
