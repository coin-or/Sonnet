// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sonnet;

namespace SonnetTest
{
    [TestClass]
    public class Sonnet_QuadraticTests
    {
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Quadratic")]
        public void SonnetTestQuad1(Type solverType)
        {
            Console.WriteLine("SonnetTestQuad1 - Create model with quad term in objective and assemble (no solving)");

            Variable x1 = new Variable("x1");
            Variable x2 = new Variable("x2");

            Expression obj = 6.0 + 0.5 * x1 * x1 + x2 * x2 - x1 * x2 - 2 * x1 - 6 * x2 - 6.0 + x1 * x1 - x2 * x1;
            Assert.IsTrue(Utils.EqualsString(obj.ToString(), "- 2 x1 - 6 x2 + 0.5 x1^2 + x2^2 - x1x2 + x1^2 - x1x2"));

            obj.Assemble();

            Assert.IsTrue(Utils.EqualsString(obj.ToString(), "- 2 x1 - 6 x2 + 1.5 x1^2 - 2 x1x2 + x2^2"));

            obj.Add(6.0);
            Assert.IsTrue(Utils.EqualsString(obj.ToString(), "- 2 x1 - 6 x2 + 1.5 x1^2 - 2 x1x2 + x2^2 + 6"));

            Expression expr = 6.0 - x2 + 1.5 * x2;
            expr = 2.0 * expr * x1;
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "12 x1 - 2 x1x2 + 3 x1x2"));
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "12 x1 + x1x2"));


            expr = 6.0 - x2 + 1.5 * x2;
            expr = x1 * expr * -1.0;
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "- 6 x1 + x1x2 - 1.5 x1x2"));
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "- 6 x1 - 0.5 x1x2"));

            expr = (6.0 - x2 + 1.5 * x2) * (6.0 - x2 + 1.5 * x2);
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "6 x2 + 0.25 x2^2 + 36"));
            // 36 - 6 x2 + 9x2 - 6 x2 + x2^2 - 1.5x2^2 - 1.5x2^2 + 9 x2 + 2.25 x2^2
            // = 6 x2 + 0.25 x2^2 + 36

            expr = (6.0 - x1 + 1.5 * x2) * (6.0 - x1 + 1.5 * x2);
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "- 12 x1 + 18 x2 + x1^2 - 3 x1x2 + 2.25 x2^2 + 36"));
            // 36 - 6 x1 + 9 x2 - 6 x1 + x1^2 - 1.5 x1x2 + 9 x2 - 1.5 x1x2 + 2.25 x2^2
            // = -12 x1 +18 x2 + x1^2 - 3 x1 x2 + 2.25 x2^2 + 36

            expr = (6.0 - x1 + 1.5 * x2).Squared();
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "- 12 x1 + 18 x2 + x1^2 - 3 x1x2 + 2.25 x2^2 + 36"));

            expr = (6.0 - x1 + 1.5 * x2 * x2) * 6.0;
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "- 6 x1 + 9 x2^2 + 36"));

            expr = 6.0 * (6.0 - x1 + 1.5 * x2 * x2);
            expr.Assemble();
            Assert.IsTrue(Utils.EqualsString(expr.ToString(), "- 6 x1 + 9 x2^2 + 36"));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Quadratic")]
        public void SonnetTestQuad2(Type solverType)
        {
            Console.WriteLine("SonnetTestQuad2 - Linear model with quad objective (QP)");

            // See https://www.inverseproblem.co.nz/OPTI/index.php/Probs/MIQP
            //  min 0.5 x1^2 + x2^2 - x1x2 - 2x1 - 6x2
            //  st. x1 + x2 <= 2
            //	   -x1 + 2x2 <= 2
            //     2x1 + x2 <= 3
            //      x1 >= 0, x2 >= 0 
            // 
            // linear relaxation: x1 = 0.66, x2 = 1.333  -> -8.22222

            Variable x1 = new Variable("x1");
            Variable x2 = new Variable("x2");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);
            Objective obj = new Objective(0.5 * x1 * x1 + x2 * x2 - x1 * x2 - 2 * x1 - 6 * x2);

            Constraint con1 = x1 + x2 <= 2;
            Constraint con2 = -1 * x1 + 2 * x2 <= 2;
            Constraint con3 = 2 * x1 + x2 <= 3;

            model.Add(con1);
            model.Add(con2);
            model.Add(con3);
            model.Objective = obj;
            model.ObjectiveSense = ObjectiveSense.Minimise;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(obj.Value, -74 / 9.0));

            Assert.IsTrue(Utils.EqualsDouble(x1.Value, 2.0 / 3.0) &&
                Utils.EqualsDouble(x2.Value, 4.0 / 3.0));

        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Quadratic")]
        public void SonnetTestQuad3(Type solverType)
        {
            Console.WriteLine("SonnetTestQuad3 - Read QP from SampleDir as MPS and solve");

            Sonnet_CoinNativeTests native = new Sonnet_CoinNativeTests();
            string sampledir = native.SampleDir;

            var files = new (string filename, double opt)[] { ("share2qp.mps", -415.73224074)/*, ("boyd1.mps", -61735219.57) */};
            foreach (var file in files)
            {
                Model model = Model.New(native.SampleDir + "\\" + file.filename);
                Solver solver = new Solver(model, solverType);
                Assert.IsFalse(solver.IsMIP, "Testing {file.filename} is expected to be QP, but is MIQP.");

                solver.Solve();

                Assert.IsTrue(solver.IsProvenOptimal);
                Assert.IsTrue(Utils.EqualsDoubleRel(model.Objective.Value, file.opt), "Optimal value of is {file.filename} is {model.Objective.Value} but should be {file.opt}");
            }
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Quadratic")]
        public void SonnetTestQuad4(Type solverType)
        {
            Console.WriteLine("SonnetTestQuad4 - Test MIQP export and that solving integer linear model with quad objective (MIQP) is NOT supported");

            // See https://www.inverseproblem.co.nz/OPTI/index.php/Probs/MIQP
            //  min 0.5 x1^2 + x2^2 - x1x2 - 2x1 - 6x2
            //  st. x1 + x2 <= 2
            //	   -x1 + 2x2 <= 2
            //     2x1 + x2 <= 3
            //      x1 >= 0, x2 >= 0 
            //  x1, x2 integer
            // Solution: x1 = 1, x2 = 1 -> obj = -7.5

            Variable x1 = new Variable("x1", VariableType.Integer);
            Variable x2 = new Variable("x2", VariableType.Integer);

            Model model = new Model();
            Solver solver = new Solver(model, typeof(COIN.OsiCbcSolverInterface));
            Objective obj = new Objective(0.5 * x1 * x1 + x2 * x2 - x1 * x2 - 2 * x1 - 6 * x2);

            Constraint con1 = x1 + x2 <= 2;
            Constraint con2 = -1 * x1 + 2 * x2 <= 2;
            Constraint con3 = 2 * x1 + x2 <= 3;

            model.Add(con1);
            model.Add(con2);
            model.Add(con3);
            model.Objective = obj;
            model.ObjectiveSense = ObjectiveSense.Minimise;

            //Test that exporting to mps and then importing results in the same model.
            solver.Export("testmiqp.mps");

            Model model2 = Model.New("testmiqp.mps");
            string model1string = model.ToString();
            string model2string = model2.ToString();
            // some renaming: we use the file name as model name, and use the Name (ClpDefau) as objective name.
            model2string = model2string.Replace("Model 'testmiqp'", $"Model '{model.Name}'");
            model2string = model2string.Replace("Objective OBJROW", $"Objective {model.Objective.Name}");
            Assert.IsTrue(Utils.EqualsString(model1string, model2string));

            // Solving MIQP doesnt work using Clp nor Cbc (in Stable), and is therefore not supported (for now).
            try
            {
                solver.Solve();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Correctly Not Supported:" + ex.Message);
                Assert.IsTrue(ex is NotSupportedException, "MIQP must be not supported");
            }
        }
    }
}
