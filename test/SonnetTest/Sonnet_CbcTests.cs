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
            //Ensure.IsTrue(SonnetLog.Default.LogLevel == 2, "Somehow the SonnetLog LogLevel changed!");

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
            // Stop within 5 seconds. 
            osiCbc.SetCbcSolverArgs("-sec", "5");
            //osiCbc.getModelPtr().setMaximumSeconds(5.0); // Doesnt work for now Cbc #567
            solver.Solve();

            Assert.IsTrue(solver.IsFeasible(), "with solution and hence feasible");
            Assert.IsFalse(solver.IsProvenOptimal, "should not be optimal yet (unless solver got a lot better)");
            //Assert.IsTrue(model.Objective.Bound < model.Objective.Value, "Bound must be less than current solution.");

            // The problem is minimization.
            // Allow also better solutions, but not worse. 
            // If your machine is significantly slower, the solution value will be worse (higher) and this test will fail--but can be ignored.
            Assert.IsTrue(model.Objective.Value >= 21801.18 && model.Objective.Value <= 22900, $"Best minimization solution of mas74 until now is ${model.Objective.Value} but should be between 21801.18 (opt) and 22465");
            
            // bound is expected to be 20482 on the reference machine.
            // If your machine is slower, the bound will be worse (lower)
            // If your machine is faster, the bound could be better (higher)
            //Assert.IsTrue(model.Objective.Bound >= 20300.0, $"Bound (Gap) is now ${model.Objective.Bound} but should be above 20300 by now.");
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
            osiCbc.SetCbcSolverArgs("-preprocess", "off",
                "-strong", "0",
                "-heurist", "off",
                "-cuts", "off");

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
        }
    }
}