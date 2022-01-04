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
    public class Sonnet_CbcTests
    {
        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest1()
        {
            Console.WriteLine("SonnetCbcTest1 - Cbc test set CbcStrategyNull and addCutGenerator");

            Model model = Model.New("MIP-124725.mps"); // added file to project, "Copy Always";
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));

            OsiCbcSolverInterface osisolver = solver.OsiSolver as OsiCbcSolverInterface;
            Assert.IsTrue(osisolver != null);

            CbcModel cbcModel = osisolver.getModelPtr();
            cbcModel.setStrategy(new CbcStrategyNull());
            cbcModel.addCutGenerator(new CglProbing());
            Assert.IsTrue(cbcModel.numberCutGenerators() == 1);
            //cbcModel.cutGenerators();

            solver.Minimise();

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 124725));
            //Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 104713.12807881772));
        }

        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest2()
        {
            Console.WriteLine("SonnetCbcTest2 - Cbc set CbcStrategyDefault");

            Model model = Model.New("MIP-124725.mps"); // added file to project, "Copy Always"
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            model.ObjectiveSense = ObjectiveSense.Minimise;

            OsiCbcSolverInterface osisolver = solver.OsiSolver as OsiCbcSolverInterface;
            Assert.IsTrue(osisolver != null);

            CbcModel cbcModel = osisolver.getModelPtr();
            cbcModel.setStrategy(new CbcStrategyDefault(1, 5, 5));
            //cbcModel.strategy().setupCutGenerators(cbcModel);

            solver.AutoResetMIPSolve = true;
            solver.Minimise();

            string message = "Used cut generators: " + string.Join(", ", cbcModel.cutGenerators().Select(generator => generator.generator().GetType().Name));

            Console.WriteLine(message);

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 124725));

            solver.Solve(true);
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 104713.12807881772));
        }

        [TestMethod, TestCategory("Cbc"), TestCategory("Logging")]
        public void SonnetCbcTest3()
        {
            Console.WriteLine("SonnetCbcTest3 - Test log message handler vs CbcMain");

            Model model = Model.New("MIP-124725.mps"); // added file to project, "Copy Always";
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            SonnetLog.Default.LogLevel = 2;

            OsiCbcSolverInterface osisolver = solver.OsiSolver as OsiCbcSolverInterface;
            //SonnetLog.Default.PassToSolver(osisolver);
            Assert.IsTrue(osisolver != null);

            CbcModel cbcModel = osisolver.getModelPtr();
            solver.Minimise();
            Ensure.IsTrue(SonnetLog.Default.LogLevel == 2, "Somehow the SonnetLog LogLevel changed!");

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 124725));
        }

        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest4()
        {
            Console.WriteLine("SonnetCbcTest4 - Test CbcModel with args to stop after solution");

            SonnetLog.Default.LogLevel = 0;
            SonnetLog.Default.Debug("Log Debug");
            SonnetLog.Default.Info("Log Info");
            SonnetLog.Default.Warn("Log Warn");
            SonnetLog.Default.Error("Log Error");

            Model model = Model.New("mas74.mps");
            Expression obj = (Expression)model.Objective;
            obj.Add(10000.0); // check that the objective constant is taken into account.
            model.Objective = obj;

            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            OsiCbcSolverInterface osiCbc = solver.OsiSolver as OsiCbcSolverInterface;
            osiCbc.getModelPtr().setMaximumSeconds(5.0); // Stop within 5 seconds
            solver.Solve();

            Assert.IsTrue(solver.IsFeasible(), "with solution and hence feasible");
            Assert.IsFalse(solver.IsProvenOptimal, "should not be optimal yet (unless solver got a lot better)");
            Assert.IsTrue(model.Objective.Bound < model.Objective.Value, "Bound must be less than current solution.");

            // The problem is minimization.
            // Allow also better solutions, but not worse. 
            // If your machine is significantly slower, the solution value will be worse (higher) and this test will fail--but can be ignored.
            Assert.IsTrue(model.Objective.Value >= 21801.18 && model.Objective.Value <= 22465, $"Best minimization solution of mas74 until now is ${model.Objective.Value} but should be between 21801.18 (opt) and 22465");
            
            // bound is expected to be 20482 on the reference machine.
            // If your machine is slower, the bound will be worse (lower)
            // If your machine is faster, the bound could be better (higher)
            Assert.IsTrue(model.Objective.Bound >= 20300.0 && model.Objective.Bound <= 20700.0, $"Bound (Gap) is now ${model.Objective.Bound} but should be betweeen 20300 and 20700");
        }

        private static int SonnetCbcTest5numSolutions = 0;
        public static CbcAction SonnetCbcTest5EventHandler(CbcModel model, CbcEvent cbcevent)
        {
            // This is an example EventHandler for CbcModel, invoked at events such as a solution was found, etc.
            // This depends on the heuristics applied during the solving, so the solutions found can be different.
            if (cbcevent == CbcEvent.solution)
            {
                SonnetCbcTest5numSolutions++;

                Assert.IsTrue(model.getBestPossibleObjValue() >= 10482.79 && model.getBestPossibleObjValue() <= 11801.18, $"Dual bound is ${model.getBestPossibleObjValue()} but should be between 10482.79 and 11801.18 (opt)");
                Assert.IsTrue(model.getObjValue() >= 11801.18 && model.getObjValue() <= 17602.0, $"Best minimization solution of mas74 until now is ${model.getObjValue()} but should be between 11801.18 (opt) and 14372.88");
                // If either of these asserts fail, it might be that the underlying CbcSolver improved with better cuts, etc.
                // If this assert fails, then the best solution found so far (by heuristics or on the tree) is worse than expected.

                if (SonnetCbcTest5numSolutions == 1)
                {
                    return CbcAction.noAction;
                }
                else if (SonnetCbcTest5numSolutions == 5)
                {
                    // Note: the action to Stop is not always respected by Cbc, for example, during heuristics.
                    return CbcAction.stop;
                }
            }

            return CbcAction.noAction;
        }

        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest5()
        {
            Console.WriteLine("SonnetCbcTest5 - Test CbcModel Event Handler");

            SonnetLog.Default.LogLevel = 4;
            SonnetLog.Default.Debug("Log Debug");
            SonnetLog.Default.Info("Log Info");
            SonnetLog.Default.Warn("Log Warn");
            SonnetLog.Default.Error("Log Error");

            Model model = Model.New("mas74.mps");
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            OsiCbcSolverInterface osiCbc = solver.OsiSolver as OsiCbcSolverInterface;
            osiCbc.getModelPtr().setMaximumSeconds(5.0); // Stop within 5 seconds

            CbcEventHandler handler = delegate (CbcModel m, CbcEvent cbcEvent) { return CbcAction.noAction; };
            handler += new CbcEventHandler(SonnetCbcTest5EventHandler); // will stop after 2nd solution
            osiCbc.Model.passInEventHandler(handler); //

            int numCalls = 0;
            CbcSolver.CallBack = delegate (CbcModel m, int whereFrom) { numCalls++; return 0; };
            solver.Solve();
            // DONT use the cbcModel after solver.Solve() because of ResetAfterMIPSolveInternal at the end of Solve()

            Assert.IsTrue(solver.IsFeasible());
            Assert.IsFalse(solver.IsProvenOptimal, "should not be optimal yet");
            Assert.IsTrue(SonnetCbcTest5numSolutions >= 2, "should have found at least two solutions by now");
            Assert.IsTrue(numCalls == 5);

            // Allow also better solutions, but not worse. The problem is minimization.
            // If you machine is significantly slower, the solution will be worse and this test will fail--but can be ignored.
            Assert.IsTrue(model.Objective.Value >= 11801.18 && model.Objective.Value <= 14168.34, $"Best minimization solution of mas74 until now is ${model.Objective.Value} but should be between 11801.18 (opt) and 14168.34");
        }

        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest6()
        {
            Console.WriteLine("SonnetCbcTest6 - Test CbcModel setObjValue and setCutoff for Minimization");

            // This test check the improvement of solving when a good solution obj value is given in advance.
            // This should help most if the relaxation bound is tight.

            SonnetLog.Default.LogLevel = 4;
            SonnetLog.Default.Debug("Log Debug");
            SonnetLog.Default.Info("Log Info");
            SonnetLog.Default.Warn("Log Warn");
            SonnetLog.Default.Error("Log Error");

            Model model = Model.New("mip-124725.mps");
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            OsiCbcSolverInterface osiCbc = solver.OsiSolver as OsiCbcSolverInterface;
            osiCbc.AddCbcSolverArgs("-preprocess", "off");
            osiCbc.AddCbcSolverArgs("-strong", "0");
            osiCbc.AddCbcSolverArgs("-heurist", "off");
            osiCbc.AddCbcSolverArgs("-cuts", "off");

            solver.Solve();

            // but use at least one solve (or generate and savebefore) to ensure there's something to reset to
            // dont reset after mip solve, otherwise CbcModel has been reset after solver.Solve() so we wouldnt getNodeCount, etc. for the solve
            solver.AutoResetMIPSolve = false;

            solver.ResetAfterMIPSolve();
            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes1 = osiCbc.getNodeCount();

            solver.ResetAfterMIPSolve();

            osiCbc.Model.setObjValue(125035.0);
            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes2 = osiCbc.getNodeCount();
            Assert.IsTrue(nodes1 > nodes2, "Providing a feasible obj value must result in improved performance, but number of nodes went from {nodes1} to {nodes2}.");

            solver.ResetAfterMIPSolve();
            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes1b = osiCbc.getNodeCount();
            Assert.IsTrue(nodes1 == nodes1b, "Number of nodes should be equal to original but {nodes1} != {nodes1b}.");

            // Not advised to use osiCbc.Model.setCutoff  Unreliable results especially for Maximisation
        }

        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest7()
        {
            Console.WriteLine("SonnetCbcTest7 - Test CbcModel setObjValue for Maximization");
            SonnetLog.Default.LogLevel = 4;
            // This test check the improvement of solving when a good solution obj value is given in advance.
            // This should help most if the relaxation bound is tight.

            Model model = Model.New("mip-124725.mps");
            Assert.IsTrue(model.ObjectiveSense == ObjectiveSense.Minimise);

            model.Objective = new Objective(-1.0 * (Expression)model.Objective);
            model.ObjectiveSense = ObjectiveSense.Maximise;

            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            OsiCbcSolverInterface osiCbc = solver.OsiSolver as OsiCbcSolverInterface;
            osiCbc.AddCbcSolverArgs("-preprocess", "off");
            osiCbc.AddCbcSolverArgs("-strong", "0");
            osiCbc.AddCbcSolverArgs("-heurist", "off");
            osiCbc.AddCbcSolverArgs("-cuts", "off");

            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");

            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");

            // but use at least one solve (or generate and savebefore) to ensure there's something to reset to
            solver.AutoResetMIPSolve = false; // dont reset after mip solve, otherwise CbcModel has been reset after solver.Solve()

            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes1 = osiCbc.getNodeCount();
            double objValue1 = model.Objective.Value;

            solver.ResetAfterMIPSolve();

            osiCbc.Model.setObjValue(-125035.0);
            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes2 = osiCbc.getNodeCount();
            Assert.IsTrue(nodes1 > nodes2, "Providing a feasible obj value must result in improved performance, but number of nodes went from {nodes1} to {nodes2}.");

            solver.ResetAfterMIPSolve();

            solver.Solve();
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes1b = osiCbc.getNodeCount();
            Assert.IsTrue(nodes1 == nodes1b, "Number of nodes should be equal to original but {nodes1} != {nodes1b}.");

            // Not advised to use osiCbc.Model.setCutoff  Unreliable results especially for Maximisation

        }

        [TestMethod, TestCategory("Cbc")]
        public void SonnetCbcTest8()
        {
            Console.WriteLine("SonnetCbcTest8 - Test solver.SetMIPStart");
            SonnetLog.Default.LogLevel = 4;

            Model model = Model.New("mip-124725.mps");
            var objLimit = model.Add((Expression)model.Objective >= 125035.0);

            // try get the intermediate solution with value 125035 by adding a constraint and then solving
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            solver.Solve();
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 125035));
            // Now the Variable Values represent the solution 125035

            // To see the benefit, disable preprocessing etc.
            OsiCbcSolverInterface osiCbc = solver.OsiSolver as OsiCbcSolverInterface;
            osiCbc.AddCbcSolverArgs("-preprocess", "off");
            osiCbc.AddCbcSolverArgs("-strong", "0");
            osiCbc.AddCbcSolverArgs("-heurist", "off");
            osiCbc.AddCbcSolverArgs("-cuts", "off");

            solver.AutoResetMIPSolve = false;
            objLimit.Enabled = false; // disable the artificial constraint on objective value
            solver.SetMIPStart(true); // take the Variable Values

            solver.Solve();
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            Assert.IsTrue(solver.IsFeasible());
            int nodes1 = osiCbc.getNodeCount(); // nodes using setMIPStart
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 124725));

            solver.ResetAfterMIPSolve();
            //objLimit.Enabled = true;
            //objLimit.Enabled = false;
            solver.Solve();
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            Assert.IsTrue(solver.IsFeasible());
            Assert.IsTrue(solver.IsProvenOptimal, "should be optimal");
            int nodes2 = osiCbc.getNodeCount(); // nodes using setMIPStart
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 124725));

            Assert.IsTrue(nodes1 < nodes2, "Providing a feasible solution must result in improved performance, but number of nodes went from {nodes1} to {nodes2}.");
        }
    }
}