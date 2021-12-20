// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using COIN;
using Sonnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonnetTest
{
    [TestClass]
    public class Sonnet_StressTests
    {
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Stress")]
        public void SonnetStressTest1(Type solverType)
        {
            GC.Collect();
            Console.WriteLine("SonnetStressTest1 - single thread stress test");
            Console.WriteLine("Available Memory (GB) Now: " + Utils.AvailableMemoryGb);

            double memoryGb = Utils.AvailableMemoryGb;
            // Don't try to use more than 1.0GB when running in 32-bit 
            // Don't try to use more than 1.0GB when running in 64-bit 
            if (IntPtr.Size * 8 == 32) memoryGb = Math.Min(memoryGb, 1.0); // 32 bit
            else memoryGb = Math.Min(memoryGb, 1.0); // 64 bit (or..)

            // Let's scale accordingly to fill up memory only to 50%.
            memoryGb *= 0.5;

            SonnetStressTestWorker(solverType, memoryGb);

        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Stress")]
        public void SonnetStressTest2(Type solverType)
        {
            Console.WriteLine($"{nameof(SonnetStressTest2)} - min(3,(n+1)) multi-processor stress test");
            int n = Math.Min(3, Environment.ProcessorCount + 1);
            double memoryGb = Utils.AvailableMemoryGb;
           
            // Don't try to use more than 1.0GB when running in 32-bit -- 1.25 GB that seems to be the limit
            // Don't try to use more than 1.0GB when running in 64-bit 
            if (IntPtr.Size * 8 == 32) memoryGb = Math.Min(memoryGb, 1.0); // 32 bit
            else memoryGb = Math.Min(memoryGb, 1.0); // 64 bit (or..)
            // Let's scale accordingly to fill up memory only to 50%.
            memoryGb *= 0.5;

            memoryGb /= n; // split across n threads
        
            Console.WriteLine("Available Memory (GB) Now: " + Utils.AvailableMemoryGb);
            Console.WriteLine("Number of threads: " + n);
            System.Threading.Thread[] threads = new System.Threading.Thread[n];
            for (int i = 0; i < n; i++)
            {
                threads[i] = new System.Threading.Thread(t => SonnetStressTestWorker(solverType, memoryGb));
                threads[i].Name = nameof(SonnetStressTest2) + "-thread-" + (i + 1);
                threads[i].Start(solverType);
                System.Threading.Thread.Sleep(2000);
            }

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                bool anyAlive = false;
                for (int i = 0; i < n; i++)
                {
                    if (threads[i].IsAlive)
                    {
                        anyAlive = true;
                        break;
                    }
                }

                if (!anyAlive) break;
            }
        }

        /// <summary>
        /// In this thread, try to create a model that uses the given amount of memory
        /// </summary>
        /// <param name="solverType">The type of the underlying solver to be used.</param>
        /// <param name="memoryGb">The amount of memory in GB to be used by this worker.</param>
        private void SonnetStressTestWorker(Type solverType, double memoryGb)
        {
            try
            {
                string threadid = System.Threading.Thread.CurrentThread.Name;
                Model model = new Model();
                Solver solver = new Solver(model, solverType);

                Console.WriteLine("Available Memory (GB) Now: " + Utils.AvailableMemoryGb);
                Console.WriteLine("Current process Memory (GB) Now: " + Utils.ProcessMemoryGb);
                Console.WriteLine("Attempting to use (GB): " + memoryGb);
                // N = 3K, M = 30K, Z=100 requires about 110MB in 32-bit and 140MB in 64-bit mode.
                // Note: Arguably only M and Z matter, but we scall also N
                double f;
                if (IntPtr.Size == 4) f = memoryGb / 0.110;
                else f = memoryGb / 0.140;
                
                // Somehow the memory usage is much less on Windows 10 with .NET 4.7, so multiply by 13
                // the stress test
                int N = (int)(f * 3000); // number of variables
                int M = (int)(f * 30000); // number of rangeconstraints
                int p = M / 10;
                int Z = 100; // number nonzeros per constraint

                Variable x = new Variable();
                Variable[] vars = Variable.New(N);

                for (int m = 0; m < M; m++)
                {
                    if (m % p == 0) Console.WriteLine(string.Format("Thread {0}: Building {1}", threadid, (1.0 * m / M).ToString("p")));
                    Expression expr = new Expression();
                    expr.Add(x);

                    for (int z = 0; z < Z; z++)
                    {
                        int i = (z + m) % N; // always between 0 and N-1
                        expr.Add(vars[i]);
                    }

                    int available = m;

                    string rowName = "MyConstraint(" + m + ")";
                    RangeConstraint con = (RangeConstraint)model.Add(rowName,
                        -model.Infinity <= expr <= 0.5*Z);
                    expr.Assemble();
                    Assert.IsTrue(expr.NumberOfCoefficients == Z + 1);

                    expr.Clear();
                }

                model.Objective = vars.Sum();
                model.ObjectiveSense = ObjectiveSense.Maximise;

                Console.WriteLine(string.Format("Thread {0}: Start Generate... (this can take a while)", threadid));

                solver.Generate();

                Assert.IsTrue(solver.OsiSolver.getNumRows() == model.NumberOfConstraints);
                Assert.IsTrue(model.NumberOfConstraints == M);
                Assert.IsTrue(solver.OsiSolver.getNumElements() == M * (Z + 1));
                GC.Collect();
                Console.WriteLine("Available Memory (GB) Now: " + Utils.AvailableMemoryGb);
                Console.WriteLine("Current process Memory (GB) Now: " + Utils.ProcessMemoryGb);
                Console.WriteLine(string.Format("Thread {0}: Finished", threadid));
                model.Clear();
                Console.WriteLine(string.Format("Thread {0}: Closed", threadid));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                Console.WriteLine(exception.StackTrace.ToString());
                Console.WriteLine("Available Memory (GB) Now: " + Utils.AvailableMemoryGb);
                Console.WriteLine("Current process Memory (GB) Now: " + Utils.ProcessMemoryGb);

                Assert.Fail("Stress test failed. Could be out-of-memory.");
                throw;
                // could be out-of-memory... Can we examine the SEHException?
            }
        }
    }
}
