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
    public class Sonnet_BasicTests
    {
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest0(Type solverType)
        {
            Console.WriteLine("SonnetTest0 - Testing the README.md example");

            Model model = new Model();
            Variable x = new Variable();
            Variable y = new Variable();
            model.Add(2 * x + 3 * y <= 10);
            model.Objective = 3 * x + y;
            Solver solver = new Solver(model, typeof(OsiClpSolverInterface));
            solver.Maximise();
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest1(Type solverType)
        {
            Console.WriteLine("SonnetTest1");

            Ensure.Is<COIN.OsiSolverInterface>(solverType, nameof(solverType));

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            RangeConstraint con0 = -model.Infinity <= x0 * 2 + x1 * 1 <= 10;
            RangeConstraint con1 = -model.Infinity <= x0 * 1 + x1 * 3 <= 15;
            model.Add("con0", con0); // same as con0.Name = "con0" model.Add(con0)
            model.Add("con1", con1);

            Objective obj = model.Objective = x0 * 3 + x1 * 1;
            x0.Lower = 0;
            x1.Lower = 0;

            x0.Upper = model.Infinity;
            x1.Upper = model.Infinity;

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            x0.Upper = 0.0;
            x1.Upper = 0.0;

            x0.Upper = model.Infinity;
            x1.Upper = model.Infinity;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsFalse(solver.IsProvenPrimalInfeasible);
            Assert.IsFalse(solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(con0.Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 5.0));

            model.Objective.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsFalse(solver.IsProvenPrimalInfeasible);
            Assert.IsFalse(solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest1b(Type solverType)
        {
            Console.WriteLine("SonnetTest1b");

            Model model = new Model("SonnetTest1b");
            Solver solver = new Solver(model, solverType, "SonnetTest1b");

            Utils.GetNumberOfVariablesOfVariableClass().SetValue(null, 0);
            Utils.GetNumberOfConstraintsOfConstraintClass().SetValue(null, 0);
            Utils.GetNumberOfObjectivesOfObjectiveClass().SetValue(null, 0);
            solver.NameDiscipline = 2;

            Variable x1 = new Variable();
            x1.Name = "x1";
            Variable x2 = new Variable("x2");
            Variable x3 = new Variable("x3", -1.0, 1.0, VariableType.Continuous);
            Variable x4 = new Variable("x4", -1.0, 1.0);
            Variable x5 = new Variable("x5", VariableType.Continuous);
            x5.Name = "x5";
            Variable x6 = new Variable(0.0, 2.0, VariableType.Continuous);
            x6.Name = "x6";
            Variable x7 = new Variable(VariableType.Integer);
            x7.Name = "x7";
            Variable x8 = new Variable(-1.0, 2.0);
            x8.Name = "x8";

            int i = 0;
            Expression[] exprs = new Expression[100];
            Expression expr1 = exprs[i++] = new Expression(x1);					// x1
            Expression expr2 = exprs[i++] = new Expression(x1 + 2.0 * x2);		// x1 + 2.0 x2
            Expression expr3 = exprs[i++] = new Expression(1.0);				// 1
            Expression expr4 = exprs[i++] = new Expression();					// 0
            Expression expr5 = exprs[i++] = new Expression(x1);					// x1
            Expression expr6 = exprs[i++] = new Expression(expr1.Add(5 * x2));	// x1 + 5 x2
            //expr1 = exprs[0] = new Expression(x1);
            Expression expr7 = exprs[i++] = expr1 + 5.0;						// x1 + 5 x2 + 5
            Expression expr8 = exprs[i++] = new Expression(x1);					// x1
            Expression expr9 = exprs[i++] = x2 + x3;	// returns new			// x2 + x3
            Expression expr10 = exprs[i++] = 1 + x4;                            // x4 + 1

            Expression expr11 = exprs[i++] = x5 + 1;                            // x5 + 1
            Expression expr12 = exprs[i++] = 1.0 * x6;							// x6
            Expression expr13 = exprs[i++] = x7 * 2.0;							// 2 x7
            Expression expr14 = exprs[i++] = x8 / 2.0;							// 0.5 x8
            Expression expr15 = exprs[i++] = new Expression(1.0);				// 1
            //Expression expr16 = exprs[i++] = new Expression(expr1.Add(1.0).Add(x1).Add(expr2).Add(3.5, x8));
            Expression expr16 = exprs[i++] = new Expression(expr1.Add(1.0).Add(x1).Add(expr2).Add(2.0 * 3.5, expr14));
            //  x1 + 5 x2 + x1 + x1 + 2.0 x2 + 3.5 x8 + 1
            //expr1 = exprs[0] = new Expression(x1);
            Expression expr17 = exprs[i++] = (new Expression()).Subtract(1.0).Subtract(x1).Subtract(expr2).Subtract(3.5, x8);
            //- x1 - x1 - 2.0 x2 - 3.5 x8 - 1
            Expression expr18 = exprs[i++] = (new Expression()).Divide(5.0).Multiply(3.2);
            //  0
            Expression expr19 = exprs[i++] = expr1 + x1;						// x1 + 5 x2 + x1 + x1 + 2.0 x2 + 3.5 x8 + x1 + 1
            Expression expr20 = exprs[i++] = x1 + expr1;						// x1 + x1 + 5 x2 + x1 + x1 + 2.0 x2 + 3.5 x8 + x1 + 1
            Expression expr21 = exprs[i++] = x1 - expr1;						// x1 - x1 - 5 x2 - x1 - x1 - 2.0 x2 - 3.5 x8 - x1 - 1
            Expression expr22 = exprs[i++] = x1 - x2;							// x1 - x2
            Expression expr23 = exprs[i++] = x1 - 1.0;							// x1 - 1
            Expression expr24 = exprs[i++] = 1.0 - x1;							// - x1 + 1
            Expression expr25 = exprs[i++] = 1.0 - expr1;						// - x1 - 5 x2 - x1 - x1 - 2.0 x2 - 3.5 x8 - x1 
            Expression expr26 = exprs[i++] = 1.0 + expr1;						// x1 + 5 x2 + x1 + x1 + 2.0 x2 + 3.5 x8 + x1 + 2
            Expression expr27 = exprs[i++] = expr1 / 2.0;						// 0.5 x1 + 2.5 x2 + 0.5 x1 + 0.5 x1 + 1.0 x2 + 1.75 x8 + 0.5
            Expression expr28 = exprs[i++] = x1 / 2.0;							// 0.5 x1

            List<Constraint> rawConstraints = new List<Constraint>();

            int n = i;
            for (i = 0; i < n; i++)
            {
                Constraint constraint = 1.0 * x1 <= exprs[i];
                constraint.Name = "Expr_con" + i;
                rawConstraints.Add(constraint);
            }

            model.Add(rawConstraints);

            i = -1;
            Constraint[] cons = new Constraint[100];
            cons[++i] = 1.0 == x1;
            cons[++i] = x1 == 1.0;
            cons[++i] = x2 == x1;
            cons[++i] = x1 == expr1;
            cons[++i] = expr1 + x1 == x1;
            cons[++i] = 1.0 == expr1;
            cons[++i] = expr1 == 1.0;
            cons[++i] = 1.0 >= x3;
            cons[++i] = 1.0 >= expr1;
            cons[++i] = x2 >= 1.0;
            cons[++i] = x2 >= x3;
            cons[++i] = x2 >= expr1;
            int m = i + 1;

            for (i = 0; i < m; i++)
            {
                model.Add(cons[i]);
            }

            string referenceUngeneratedModelToString =
                "Model 'SonnetTest1b'\r\n" +
                "x1 : Continuous : [0, Inf]\r\n" +
                "x2 : Continuous : [0, Inf]\r\n" +
                "x8 : Continuous : [-1, 2]\r\n" +
                "x3 : Continuous : [-1, 1]\r\n" +
                "x4 : Continuous : [-1, 1]\r\n" +
                "x5 : Continuous : [0, Inf]\r\n" +
                "x6 : Continuous : [0, 2]\r\n" +
                "x7 : Integer : [0, Inf]\r\n" +
                "Objective: Objective obj : 0\r\n" +
                "Constraints:\r\n" +
                "Expr_con0 : x1 <= x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n" +
                "Expr_con1 : x1 <= x1 + 2 x2\r\n" +
                "Expr_con2 : x1 <= 1\r\n" +
                "Expr_con3 : x1 <= 0\r\n" +
                "Expr_con4 : x1 <= x1\r\n" +
                "Expr_con5 : x1 <= x1 + 5 x2\r\n" +
                "Expr_con6 : x1 <= x1 + 5 x2 + 5\r\n" +
                "Expr_con7 : x1 <= x1\r\n" +
                "Expr_con8 : x1 <= x2 + x3\r\n" +
                "Expr_con9 : x1 <= x4 + 1\r\n" +
                "Expr_con10 : x1 <= x5 + 1\r\n" +
                "Expr_con11 : x1 <= x6\r\n" +
                "Expr_con12 : x1 <= 2 x7\r\n" +
                "Expr_con13 : x1 <= 0.5 x8\r\n" +
                "Expr_con14 : x1 <= 1\r\n" +
                "Expr_con15 : x1 <= x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n" +
                "Expr_con16 : x1 <= - x1 - x1 - 2 x2 - 3.5 x8 - 1\r\n" +
                "Expr_con17 : x1 <= 0\r\n" +
                "Expr_con18 : x1 <= x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + x1 + 1\r\n" +
                "Expr_con19 : x1 <= x1 + x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n" +
                "Expr_con20 : x1 <= x1 - x1 - 5 x2 - x1 - x1 - 2 x2 - 3.5 x8 - 1\r\n" +
                "Expr_con21 : x1 <= x1 - x2\r\n" +
                "Expr_con22 : x1 <= x1 - 1\r\n" +
                "Expr_con23 : x1 <= - x1 + 1\r\n" +
                "Expr_con24 : x1 <= - x1 - 5 x2 - x1 - x1 - 2 x2 - 3.5 x8\r\n" +
                "Expr_con25 : x1 <= x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 2\r\n" +
                "Expr_con26 : x1 <= 0.5 x1 + 2.5 x2 + 0.5 x1 + 0.5 x1 + x2 + 1.75 x8 + 0.5\r\n" +
                "Expr_con27 : x1 <= 0.5 x1\r\n" +
                "Con_28 : 1 == x1\r\n" +
                "Con_29 : x1 == 1\r\n" +
                "Con_30 : x2 == x1\r\n" +
                "Con_31 : x1 == x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n" +
                "Con_32 : x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + x1 + 1 == x1\r\n" +
                "Con_33 : 1 == x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n" +
                "Con_34 : x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1 == 1\r\n" +
                "Con_35 : 1 >= x3\r\n" +
                "Con_36 : 1 >= x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n" +
                "Con_37 : x2 >= 1\r\n" +
                "Con_38 : x2 >= x3\r\n" +
                "Con_39 : x2 >= x1 + 5 x2 + x1 + x1 + 2 x2 + 3.5 x8 + 1\r\n";

            string modelUngeneratedString = model.ToString();
            Assert.IsTrue(Utils.EqualsString(modelUngeneratedString, referenceUngeneratedModelToString));

            solver.Generate();

            int istart = i + 1;
            cons[++i] = expr1 >= 1.0;
            cons[++i] = expr1 >= x1;
            cons[++i] = expr1 >= expr2;
            cons[++i] = 1.0 <= x3;
            cons[++i] = 1.0 <= expr1;
            cons[++i] = x2 <= 1.0;
            cons[++i] = x2 <= x3;
            cons[++i] = x2 <= expr1;
            cons[++i] = expr1 <= 1.0;
            cons[++i] = expr1 <= x1;
            cons[++i] = expr1 <= expr2;
            cons[++i] = new Constraint(expr1, ConstraintType.EQ, expr2);
            cons[++i] = new Constraint("mycon", expr3, ConstraintType.EQ, expr4);
            cons[++i] = new Constraint(cons[i - 1]);
            cons[++i] = new Constraint("othercon", cons[i - 1]);
            cons[++i] = new Expression(0.0) >= new Expression();
            cons[++i] = 1.2 * (new Variable("xNEW")) >= new Expression();
            m = i + 1;

            for (i = istart; i < m; i++)
            {
                model.Add(cons[i]);
            }

            solver.Generate();

            string referenceGeneratedModel =
                "Solver 'SonnetTest1b'\r\n" +
                "Model 'SonnetTest1b'\r\n" +
                "x1 : Continuous : [0, Inf]\r\n" +
                "x2 : Continuous : [0, Inf]\r\n" +
                "x8 : Continuous : [-1, 2]\r\n" +
                "x3 : Continuous : [-1, 1]\r\n" +
                "x4 : Continuous : [-1, 1]\r\n" +
                "x5 : Continuous : [0, Inf]\r\n" +
                "x6 : Continuous : [0, 2]\r\n" +
                "x7 : Integer : [0, Inf]\r\n" +
                "xNEW : Continuous : [0, Inf]\r\n" +
                "Objective: Objective obj : 0\r\n" +
                "Constraints:\r\n" +
                "Expr_con0 : - 2 x1 - 7 x2 - 3.5 x8 <= 1\r\n" +
                "Expr_con1 : - 2 x2 <= 0\r\n" +
                "Expr_con2 : x1 <= 1\r\n" +
                "Expr_con3 : x1 <= 0\r\n" +
                "Expr_con4 : 0 <= 0\r\n" +
                "Expr_con5 : - 5 x2 <= 0\r\n" +
                "Expr_con6 : - 5 x2 <= 5\r\n" +
                "Expr_con7 : 0 <= 0\r\n" +
                "Expr_con8 : x1 - x2 - x3 <= 0\r\n" +
                "Expr_con9 : x1 - x4 <= 1\r\n" +
                "Expr_con10 : x1 - x5 <= 1\r\n" +
                "Expr_con11 : x1 - x6 <= 0\r\n" +
                "Expr_con12 : x1 - 2 x7 <= 0\r\n" +
                "Expr_con13 : x1 - 0.5 x8 <= 0\r\n" +
                "Expr_con14 : x1 <= 1\r\n" +
                "Expr_con15 : - 2 x1 - 7 x2 - 3.5 x8 <= 1\r\n" +
                "Expr_con16 : 3 x1 + 2 x2 + 3.5 x8 <= -1\r\n" +
                "Expr_con17 : x1 <= 0\r\n" +
                "Expr_con18 : - 3 x1 - 7 x2 - 3.5 x8 <= 1\r\n" +
                "Expr_con19 : - 3 x1 - 7 x2 - 3.5 x8 <= 1\r\n" +
                "Expr_con20 : 3 x1 + 7 x2 + 3.5 x8 <= -1\r\n" +
                "Expr_con21 : x2 <= 0\r\n" +
                "Expr_con22 : 0 <= -1\r\n" +
                "Expr_con23 : 2 x1 <= 1\r\n" +
                "Expr_con24 : 4 x1 + 7 x2 + 3.5 x8 <= 0\r\n" +
                "Expr_con25 : - 2 x1 - 7 x2 - 3.5 x8 <= 2\r\n" +
                "Expr_con26 : - 0.5 x1 - 3.5 x2 - 1.75 x8 <= 0.5\r\n" +
                "Expr_con27 : 0.5 x1 <= 0\r\n" +
                "Con_28 : - x1 == -1\r\n" +
                "Con_29 : x1 == 1\r\n" +
                "Con_30 : - x1 + x2 == 0\r\n" +
                "Con_31 : - 2 x1 - 7 x2 - 3.5 x8 == 1\r\n" +
                "Con_32 : 3 x1 + 7 x2 + 3.5 x8 == -1\r\n" +
                "Con_33 : - 3 x1 - 7 x2 - 3.5 x8 == 0\r\n" +
                "Con_34 : 3 x1 + 7 x2 + 3.5 x8 == 0\r\n" +
                "Con_35 : - x3 >= -1\r\n" +
                "Con_36 : - 3 x1 - 7 x2 - 3.5 x8 >= 0\r\n" +
                "Con_37 : x2 >= 1\r\n" +
                "Con_38 : x2 - x3 >= 0\r\n" +
                "Con_39 : - 3 x1 - 6 x2 - 3.5 x8 >= 1\r\n" +
                "Con_40 : 3 x1 + 7 x2 + 3.5 x8 >= 0\r\n" +
                "Con_41 : 2 x1 + 7 x2 + 3.5 x8 >= -1\r\n" +
                "Con_42 : 2 x1 + 5 x2 + 3.5 x8 >= -1\r\n" +
                "Con_43 : - x3 <= -1\r\n" +
                "Con_44 : - 3 x1 - 7 x2 - 3.5 x8 <= 0\r\n" +
                "Con_45 : x2 <= 1\r\n" +
                "Con_46 : x2 - x3 <= 0\r\n" +
                "Con_47 : - 3 x1 - 6 x2 - 3.5 x8 <= 1\r\n" +
                "Con_48 : 3 x1 + 7 x2 + 3.5 x8 <= 0\r\n" +
                "Con_49 : 2 x1 + 7 x2 + 3.5 x8 <= -1\r\n" +
                "Con_50 : 2 x1 + 5 x2 + 3.5 x8 <= -1\r\n" +
                "Con_51 : 2 x1 + 5 x2 + 3.5 x8 == -1\r\n" +
                "mycon : 0 == -1\r\n" +
                "Con_53 : 0 == -1\r\n" +
                "othercon : 0 == -1\r\n" +
                "Con_55 : 0 >= 0\r\n" +
                "Con_56 : 1.2 xNEW >= 0\r\n";

            string modelString = solver.ToString();
            Assert.IsTrue(Utils.EqualsString(modelString, referenceGeneratedModel));

            solver.Export("test.mps");
            string testMps;
            using (System.IO.StreamReader test = new System.IO.StreamReader("test.mps"))
            {
                testMps = test.ReadToEnd();
            }

            string referenceTestMps = "NAME BLANK FREE\n" +
                "ROWS\n" +
                " N OBJROW\n" +
                " L Expr_con0\n" +
                " L Expr_con1\n" +
                " L Expr_con2\n" +
                " L Expr_con3\n" +
                " L Expr_con4\n" +
                " L Expr_con5\n" +
                " L Expr_con6\n" +
                " L Expr_con7\n" +
                " L Expr_con8\n" +
                " L Expr_con9\n" +
                " L Expr_con10\n" +
                " L Expr_con11\n" +
                " L Expr_con12\n" +
                " L Expr_con13\n" +
                " L Expr_con14\n" +
                " L Expr_con15\n" +
                " L Expr_con16\n" +
                " L Expr_con17\n" +
                " L Expr_con18\n" +
                " L Expr_con19\n" +
                " L Expr_con20\n" +
                " L Expr_con21\n" +
                " L Expr_con22\n" +
                " L Expr_con23\n" +
                " L Expr_con24\n" +
                " L Expr_con25\n" +
                " L Expr_con26\n" +
                " L Expr_con27\n" +
                " E Con_28\n" +
                " E Con_29\n" +
                " E Con_30\n" +
                " E Con_31\n" +
                " E Con_32\n" +
                " E Con_33\n" +
                " E Con_34\n" +
                " G Con_35\n" +
                " G Con_36\n" +
                " G Con_37\n" +
                " G Con_38\n" +
                " G Con_39\n" +
                " G Con_40\n" +
                " G Con_41\n" +
                " G Con_42\n" +
                " L Con_43\n" +
                " L Con_44\n" +
                " L Con_45\n" +
                " L Con_46\n" +
                " L Con_47\n" +
                " L Con_48\n" +
                " L Con_49\n" +
                " L Con_50\n" +
                " E Con_51\n" +
                " E mycon\n" +
                " E Con_53\n" +
                " E othercon\n" +
                " G Con_55\n" +
                " G Con_56\n" +
                "COLUMNS\n" +
                " x1 Expr_con0 -2. Expr_con2 1. \n" +
                " x1 Expr_con3 1. Expr_con8 1. \n" +
                " x1 Expr_con9 1. Expr_con10 1. \n" +
                " x1 Expr_con11 1. Expr_con12 1. \n" +
                " x1 Expr_con13 1. Expr_con14 1. \n" +
                " x1 Expr_con15 -2. Expr_con16 3. \n" +
                " x1 Expr_con17 1. Expr_con18 -3. \n" +
                " x1 Expr_con19 -3. Expr_con20 3. \n" +
                " x1 Expr_con23 2. Expr_con24 4. \n" +
                " x1 Expr_con25 -2. Expr_con26 -0.5 \n" +
                " x1 Expr_con27 0.5 Con_28 -1. \n" +
                " x1 Con_29 1. Con_30 -1. \n" +
                " x1 Con_31 -2. Con_32 3. \n" +
                " x1 Con_33 -3. Con_34 3. \n" +
                " x1 Con_36 -3. Con_39 -3. \n" +
                " x1 Con_40 3. Con_41 2. \n" +
                " x1 Con_42 2. Con_44 -3. \n" +
                " x1 Con_47 -3. Con_48 3. \n" +
                " x1 Con_49 2. Con_50 2. \n" +
                " x1 Con_51 2. \n" +
                " x2 Expr_con0 -7. Expr_con1 -2. \n" +
                " x2 Expr_con5 -5. Expr_con6 -5. \n" +
                " x2 Expr_con8 -1. Expr_con15 -7. \n" +
                " x2 Expr_con16 2. Expr_con18 -7. \n" +
                " x2 Expr_con19 -7. Expr_con20 7. \n" +
                " x2 Expr_con21 1. Expr_con24 7. \n" +
                " x2 Expr_con25 -7. Expr_con26 -3.5 \n" +
                " x2 Con_30 1. Con_31 -7. \n" +
                " x2 Con_32 7. Con_33 -7. \n" +
                " x2 Con_34 7. Con_36 -7. \n" +
                " x2 Con_37 1. Con_38 1. \n" +
                " x2 Con_39 -6. Con_40 7. \n" +
                " x2 Con_41 7. Con_42 5. \n" +
                " x2 Con_44 -7. Con_45 1. \n" +
                " x2 Con_46 1. Con_47 -6. \n" +
                " x2 Con_48 7. Con_49 7. \n" +
                " x2 Con_50 5. Con_51 5. \n" +
                " x8 Expr_con0 -3.5 Expr_con13 -0.5 \n" +
                " x8 Expr_con15 -3.5 Expr_con16 3.5 \n" +
                " x8 Expr_con18 -3.5 Expr_con19 -3.5 \n" +
                " x8 Expr_con20 3.5 Expr_con24 3.5 \n" +
                " x8 Expr_con25 -3.5 Expr_con26 -1.75 \n" +
                " x8 Con_31 -3.5 Con_32 3.5 \n" +
                " x8 Con_33 -3.5 Con_34 3.5 \n" +
                " x8 Con_36 -3.5 Con_39 -3.5 \n" +
                " x8 Con_40 3.5 Con_41 3.5 \n" +
                " x8 Con_42 3.5 Con_44 -3.5 \n" +
                " x8 Con_47 -3.5 Con_48 3.5 \n" +
                " x8 Con_49 3.5 Con_50 3.5 \n" +
                " x8 Con_51 3.5 \n" +
                " x3 Expr_con8 -1. Con_35 -1. \n" +
                " x3 Con_38 -1. Con_43 -1. \n" +
                " x3 Con_46 -1. \n" +
                " x4 Expr_con9 -1. \n" +
                " x5 Expr_con10 -1. \n" +
                " x6 Expr_con11 -1. \n" +
                " x7 Expr_con12 -2. \n" +
                " xNEW Con_56 1.2 \n" +
                "RHS\n" +
                " RHS Expr_con0 1. Expr_con2 1. \n" +
                " RHS Expr_con6 5. Expr_con9 1. \n" +
                " RHS Expr_con10 1. Expr_con14 1. \n" +
                " RHS Expr_con15 1. Expr_con16 -1. \n" +
                " RHS Expr_con18 1. Expr_con19 1. \n" +
                " RHS Expr_con20 -1. Expr_con22 -1. \n" +
                " RHS Expr_con23 1. Expr_con25 2. \n" +
                " RHS Expr_con26 0.5 Con_28 -1. \n" +
                " RHS Con_29 1. Con_31 1. \n" +
                " RHS Con_32 -1. Con_35 -1. \n" +
                " RHS Con_37 1. Con_39 1. \n" +
                " RHS Con_41 -1. Con_42 -1. \n" +
                " RHS Con_43 -1. Con_45 1. \n" +
                " RHS Con_47 1. Con_49 -1. \n" +
                " RHS Con_50 -1. Con_51 -1. \n" +
                " RHS mycon -1. Con_53 -1. \n" +
                " RHS othercon -1. \n" +
                "BOUNDS\n" +
                " LO BOUND x8 -1. \n" +
                " UP BOUND x8 2. \n" +
                " LO BOUND x3 -1. \n" +
                " UP BOUND x3 1. \n" +
                " LO BOUND x4 -1. \n" +
                " UP BOUND x4 1. \n" +
                " UP BOUND x6 2. \n" +
                " UI BOUND x7 1e+30\n" +
                "ENDATA\n";

            testMps = testMps.Replace("\r", "");
            while (testMps.IndexOf("  ") >= 0)
            {
                testMps = testMps.Replace("  ", " ");
            }

            if (solver.OsiSolverFullName.Contains("OsiClp"))
            {
                testMps = testMps.Replace("ClpDefau", "BLANK");
                Assert.IsTrue(Utils.EqualsString(testMps, referenceTestMps));
            }
            else if (solver.OsiSolverFullName.Contains("OsiCbc"))
            {
                testMps = testMps.Replace("ClpDefau", "BLANK");
                Assert.IsTrue(Utils.EqualsString(testMps, referenceTestMps));
            }
            else if (solver.OsiSolverFullName.Contains("OsiCpx"))
            {
                Assert.IsTrue(true); // skip
            }
            else
            {
                Assert.IsTrue(Utils.EqualsString(testMps, referenceTestMps));
            }
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest1c(Type solverType)
        {
            Console.WriteLine("SonnetTest1c");

            Variable[] variables;
            Model model = Model.New("test.mps", out variables);
            model.Name = "SonnetTest1c";
            Assert.IsTrue(model != null);
            Solver solver = new Solver(model, solverType, "SonnetTest1c");
            solver.Generate();

            Variable x7 = variables.GetVariable("x7");
            x7.Upper = model.Infinity;

            string referenceGeneratedModel =
                    "Solver 'SonnetTest1c'\r\n" +
                    "Model 'SonnetTest1c'\r\n" +
                    "x1 : Continuous : [0, Inf]\n" +
                    "x2 : Continuous : [0, Inf]\n" +
                    "x8 : Continuous : [-1, 2]\n" +
                    "x3 : Continuous : [-1, 1]\n" +
                    "x4 : Continuous : [-1, 1]\n" +
                    "x5 : Continuous : [0, Inf]\n" +
                    "x6 : Continuous : [0, 2]\n" +
                    "x7 : Integer : [0, Inf]\n" +
                    "xNEW : Continuous : [0, Inf]\n" +
                    "Objective: Objective OBJROW : 0\n" +
                    "Constraints:\n" +
                    "Expr_con0 : - 2 x1 - 7 x2 - 3.5 x8 <= 1\n" +
                    "Expr_con1 : - 2 x2 <= 0\n" +
                    "Expr_con2 : x1 <= 1\n" +
                    "Expr_con3 : x1 <= 0\n" +
                    "Expr_con4 : 0 <= 0\n" +
                    "Expr_con5 : - 5 x2 <= 0\n" +
                    "Expr_con6 : - 5 x2 <= 5\n" +
                    "Expr_con7 : 0 <= 0\n" +
                    "Expr_con8 : x1 - x2 - x3 <= 0\n" +
                    "Expr_con9 : x1 - x4 <= 1\n" +
                    "Expr_con10 : x1 - x5 <= 1\n" +
                    "Expr_con11 : x1 - x6 <= 0\n" +
                    "Expr_con12 : x1 - 2 x7 <= 0\n" +
                    "Expr_con13 : x1 - 0.5 x8 <= 0\n" +
                    "Expr_con14 : x1 <= 1\n" +
                    "Expr_con15 : - 2 x1 - 7 x2 - 3.5 x8 <= 1\n" +
                    "Expr_con16 : 3 x1 + 2 x2 + 3.5 x8 <= -1\n" +
                    "Expr_con17 : x1 <= 0\n" +
                    "Expr_con18 : - 3 x1 - 7 x2 - 3.5 x8 <= 1\n" +
                    "Expr_con19 : - 3 x1 - 7 x2 - 3.5 x8 <= 1\n" +
                    "Expr_con20 : 3 x1 + 7 x2 + 3.5 x8 <= -1\n" +
                    "Expr_con21 : x2 <= 0\n" +
                    "Expr_con22 : 0 <= -1\n" +
                    "Expr_con23 : 2 x1 <= 1\n" +
                    "Expr_con24 : 4 x1 + 7 x2 + 3.5 x8 <= 0\n" +
                    "Expr_con25 : - 2 x1 - 7 x2 - 3.5 x8 <= 2\n" +
                    "Expr_con26 : - 0.5 x1 - 3.5 x2 - 1.75 x8 <= 0.5\n" +
                    "Expr_con27 : 0.5 x1 <= 0\n" +
                    "Con_28 : - x1 == -1\n" +
                    "Con_29 : x1 == 1\n" +
                    "Con_30 : - x1 + x2 == 0\n" +
                    "Con_31 : - 2 x1 - 7 x2 - 3.5 x8 == 1\n" +
                    "Con_32 : 3 x1 + 7 x2 + 3.5 x8 == -1\n" +
                    "Con_33 : - 3 x1 - 7 x2 - 3.5 x8 == 0\n" +
                    "Con_34 : 3 x1 + 7 x2 + 3.5 x8 == 0\n" +
                    "Con_35 : - x3 >= -1\n" +
                    "Con_36 : - 3 x1 - 7 x2 - 3.5 x8 >= 0\n" +
                    "Con_37 : x2 >= 1\n" +
                    "Con_38 : x2 - x3 >= 0\n" +
                    "Con_39 : - 3 x1 - 6 x2 - 3.5 x8 >= 1\n" +
                    "Con_40 : 3 x1 + 7 x2 + 3.5 x8 >= 0\n" +
                    "Con_41 : 2 x1 + 7 x2 + 3.5 x8 >= -1\n" +
                    "Con_42 : 2 x1 + 5 x2 + 3.5 x8 >= -1\n" +
                    "Con_43 : - x3 <= -1\n" +
                    "Con_44 : - 3 x1 - 7 x2 - 3.5 x8 <= 0\n" +
                    "Con_45 : x2 <= 1\n" +
                    "Con_46 : x2 - x3 <= 0\n" +
                    "Con_47 : - 3 x1 - 6 x2 - 3.5 x8 <= 1\n" +
                    "Con_48 : 3 x1 + 7 x2 + 3.5 x8 <= 0\n" +
                    "Con_49 : 2 x1 + 7 x2 + 3.5 x8 <= -1\n" +
                    "Con_50 : 2 x1 + 5 x2 + 3.5 x8 <= -1\n" +
                    "Con_51 : 2 x1 + 5 x2 + 3.5 x8 == -1\n" +
                    "mycon : 0 == -1\n" +
                    "Con_53 : 0 == -1\n" +
                    "othercon : 0 == -1\n" +
                    "Con_55 : 0 >= 0\n" +
                    "Con_56 : 1.2 xNEW >= 0\n";


            referenceGeneratedModel = referenceGeneratedModel.Replace("\r\n", "\n");
            string modelString = solver.ToString().Replace("\r\n", "\n");

            // OsiCpx writeMps is unreliable
            if (!solver.OsiSolverFullName.Contains("OsiCpx"))
            {
                Assert.IsTrue(Utils.EqualsString(modelString, referenceGeneratedModel));
            }
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest2(Type solverType)
        {
            Console.WriteLine("SonnetTest2");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            Constraint r0 = x0 * 2 + x1 * 1 <= 10.0;
            Constraint r1 = x0 * 1 + x1 * 3 <= 15.0;
            Constraint r2 = 1 <= x0 * 1 + x1 * 1 <= model.Infinity;
            Objective obj = model.Objective = (3.0 * x0 + 1.0 * x1);
            model.ObjectiveSense = ObjectiveSense.Maximise;

            model.Add(r0);
            model.Add(r1);
            model.Add(r2);

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 5.0) &&
                Utils.EqualsDouble(r2.Value, 5.0));

            obj.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 15.0) &&
                Utils.EqualsDouble(r2.Value, 7.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest3(Type solverType)
        {
            Console.WriteLine("SonnetTest3");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", 0.0, 10.0);
            Variable x1 = new Variable("x1", 0.0, 10.0);

            RangeConstraint con0 = 0.0 <= x0 * 2 + x1 * 1 <= 10;
            RangeConstraint con1 = 0.0 <= x0 * 1 + x1 * 3 <= 15;

            model.Add("con0", con0);

            solver.Generate();

            model.Add("con1", con1);

            Objective obj = model.Objective = (x0 * 3 + x1 * 1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 5.0));

            model.Objective.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest4(Type solverType)
        {
            Console.WriteLine("SonnetTest4");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            RangeConstraint r0 = -model.Infinity <= x0 * 2 + x1 * 1 <= 10;
            RangeConstraint r1 = -model.Infinity <= x0 * 1 + x1 * 3 <= 15;

            model.Add(r0);

            solver.Generate();

            model.Add(r1);

            Objective obj = model.Objective = (x0 * 3 + x1 * 1);
            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 5.0));

            model.Objective.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest5(Type solverType)
        {
            Console.WriteLine("SonnetTest5");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", -model.Infinity, model.Infinity);
            Variable x1 = new Variable("x1", -model.Infinity, model.Infinity);

            Objective obj = model.Objective = x0 * 3 + x1 * 1;

            RangeConstraint r0 = -model.Infinity <= x0 * 2 + x1 * 1 <= 10;
            RangeConstraint r1 = -model.Infinity <= x0 * 1 + x1 * 3 <= 15;

            model.Add(r0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(solver.IsProvenDualInfeasible);

            obj.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest6(Type solverType)
        {
            Console.WriteLine("SonnetTest6");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", 0, model.Infinity);
            Variable x1 = new Variable("x1", 0, model.Infinity);

            Objective obj = new Objective();
            obj.SetCoefficient(x0, 3.0);
            obj.SetCoefficient(x1, 1.0);
            model.Objective = obj;

            Constraint r0 = x0 * 2 + x1 * 1 <= 10;
            Constraint r1 = x0 * 1 + x1 * 3 <= 15;

            model.Add(r0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 5.0));

            obj.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest7(Type solverType)
        {
            Console.WriteLine("SonnetTest7");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", 4, model.Infinity);
            Variable x1 = new Variable("x1", 3, model.Infinity);

            Objective obj = model.Objective = 3 * x0 + 1 * x1;

            Constraint r0 = x0 * 2 + x1 * 1 <= 10;
            Constraint r1 = x0 * 1 + x1 * 3 <= 15;

            model.Add(r0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(solver.IsProvenPrimalInfeasible);

            obj.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(solver.IsProvenPrimalInfeasible);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest8(Type solverType)
        {
            Console.WriteLine("SonnetTest8");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", Double.MinValue, Double.MaxValue);
            Variable x1 = new Variable("x1", Double.MinValue, Double.MaxValue);

            Objective obj = model.Objective = (3 * x0 + 1 * x1);

            RangeConstraint r0 = 0.0 <= x0 * 2 + x1 * 1 <= 10;
            RangeConstraint r1 = 0.0 <= x0 * 1 + x1 * 3 <= 15;

            model.Add(r0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 6.0) &&
                Utils.EqualsDouble(x1.Value, -2.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 0.0));

            obj.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest9(Type solverType)
        {
            Console.WriteLine("SonnetTest9");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", Double.MinValue, Double.MaxValue);
            Variable x1 = new Variable("x1", Double.MinValue, Double.MaxValue);

            Objective obj = model.Objective = (3 * x0 + 1 * x1);

            RangeConstraint r0 = 0.0 <= x0 * 2 + x1 * 1 <= 10;
            RangeConstraint r1 = 0.0 <= x0 * 1 + x1 * 3 <= 15;
            Constraint r2 = 1 * x0 + 4 * x1 >= 12;

            model.Add(r0);
            model.Add(r1);
            model.Add(r2);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 4.0) &&
                Utils.EqualsDouble(x1.Value, 2.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 10.0) &&
                Utils.EqualsDouble(r2.Value, 12.0));

            obj.SetCoefficient(x0, 1.0);
            obj.SetCoefficient(x1, 1.0);

            solver.Resolve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 15.0) &&
                Utils.EqualsDouble(r2.Value, 19.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest10(Type solverType)
        {
            Console.WriteLine("SonnetTest10");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            Objective obj = model.Objective = (3 * x0 + 1 * x1);

            Constraint r0 = 4 * x0 + 2 * x1 <= 20;
            Constraint r1 = 1 * x0 + 3 * x1 <= 15;

            model.Add(r0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 5.0));

            obj.SetCoefficient(x0, 1);
            obj.SetCoefficient(x1, 1);

            solver.Resolve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest11(Type solverType)
        {
            Console.WriteLine("SonnetTest11");

            Model model = new Model();
            Solver solver;
            solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            x0.Lower = Double.MinValue;
            x1.Lower = Double.MinValue;

            model.Objective = (3 * x0 + 1 * x1);

            RangeConstraint r0 = Double.MinValue <= x0 + x0 + x1 <= 10;
            RangeConstraint r1 = Double.MinValue <= x0 * 1 + x1 + x1 <= 15;

            model.Add(r0);
            model.Add(r1);

            r1.SetCoefficient(x1, 3.0);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver = new Solver(model, solverType);
            solver.Solve();


            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(solver.IsProvenDualInfeasible);

            r0.Lower = 0.0;
            r1.Lower = 0.0;

            solver.Resolve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 6.0) &&
                Utils.EqualsDouble(x1.Value, -2.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 10.0) &&
                Utils.EqualsDouble(r1.Value, 0.0));

            r0.Lower = Double.MinValue;
            r1.Lower = Double.MinValue;
            r0.Upper = 20.0;
            r1.Upper = 15.0;
            r0.SetCoefficient(x0, 4.0);
            r0.SetCoefficient(x1, 1.0);
            r1.SetCoefficient(x0, 2.0);
            r1.SetCoefficient(x1, 3.0);

            solver.Resolve();


            Console.WriteLine(solver.ToString());
            Console.WriteLine(solver.ToSolutionString());

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 4.5) &&
                Utils.EqualsDouble(x1.Value, 2.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest12(Type solverType)
        {
            Console.WriteLine("SonnetTest12");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            model.Objective = (3 * x0 + 1 * x1);

            RangeConstraint r0 = 10 <= 4 * x0 - 30 <= -10;
            Constraint r1 = 2 * x0 + 3 * x1 <= 15 + x0;

            model.Add(r0);
            solver.Generate();

            Assert.IsTrue(Utils.EqualsDouble(r0.Lower, 40.0));

            r0.Lower = 0.0;
            r0.SetCoefficient(x1, 2.0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 5.0));

            model.Objective.SetCoefficient(x0, 1);
            model.Objective.SetCoefficient(x1, 1);

            solver.Resolve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 15.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest13(Type solverType)
        {
            Console.WriteLine("SonnetTest13");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");

            model.Objective = (3 * x0 + 1 * x1);

            RangeConstraint r0 = 10 <= 4 * x0 - 30 <= -10;
            Constraint r1 = 2 * x0 + 3 * x1 <= 15 + x0;

            model.Add(r0);
            solver.Generate();

            Assert.IsTrue(Utils.EqualsDouble(r0.Lower, 40.0));
            r0.Lower = 0.0;
            r0.SetCoefficient(x1, 2.0);
            model.Add(r1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            Assert.IsTrue(Utils.EqualsDouble(r0.GetCoefficient(x0), 4.0));
            Assert.IsTrue(Utils.EqualsDouble(r0.GetCoefficient(x1), 2.0));

            solver.Solve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 5.0));


            RangeConstraint r2 = -Double.MaxValue <= 4 * x0 + x1 - 20 <= 0;
            model.Add(r2);

            solver.Generate();

            Assert.IsTrue(Utils.EqualsDouble(r2.Lower, -Double.MaxValue));
            Assert.IsTrue(Utils.EqualsDouble(r2.Upper, 20.0));

            RangeConstraint r3 = -40.0 <= -4 * x0 - x1 - 20.0 <= Double.MaxValue;
            model.Add(r3);

            solver.Generate();

            Assert.IsTrue(Utils.EqualsDouble(r3.Lower, -20.0));
            Assert.IsTrue(Utils.EqualsDouble(r3.Upper, Double.MaxValue));


            solver.Solve();


            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(r0.Value, 20.0) &&
                Utils.EqualsDouble(r1.Value, 5.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest14(Type solverType)
        {
            Console.WriteLine("SonnetTest14");

            /*
            With this matrix we have a primal/dual infeas problem. Leaving the first
            row makes it primal feas, leaving the first col makes it dual feas. 
            (JWG but leaving the first col out of the primal representation will
            make the primal unfeasible..)
            All vars are >= 0

            obj:-1  2 -3  4 -5 (min)

                0 -1  0  0 -2  >=  1
                1  0 -3  0  4  >= -2
                0  3  0 -5  0  >=  3
                0  0  5  0 -6  >= -4
                2 -4  0  6  0  >=  5 
				
            <=>
            primal:
            max 1 -2  3 -4  5
                0  1  0  0  2  <= -1
               -1  0  3  0 -4  <=  2
                0 -3  0  5  0  <= -3
                0  0 -5  0  6  <=  4
               -2  4  0 -6  0  <= -5 
			
            dual:
            min-1  2 -3  4 -5
                0 -1  0  0 -2  >=  1 (*)
                1  0 -3  0  4  >= -2 	
                0  3  0 -5  0  >=  3
                0  0  5  0 -6  >= -4
                2 -4  0  6  0  >=  5
                */

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x1, x2, x3, x4, x5;

            Variable[] xArray = Variable.New(5);
            int i = 0;
            foreach (Variable x in xArray)
            {
                x.Name = "x" + (++i);
            }

            x1 = xArray[0];
            x2 = xArray[1];
            x3 = xArray[2];
            x4 = xArray[3];
            x5 = xArray[4];

            Expression expr = xArray.Sum();
            Expression expr2 = -1.0 * expr;
            expr2.ToString();
            Objective obj = model.Objective = (-1.0 * expr);
            obj.ToString();

            RangeConstraint r1 = (RangeConstraint)model.Add(1 <= -1 * x2 + 0 * x3 - 2 * x5 <= Double.MaxValue);

            solver.Generate();

            model.Objective.SetCoefficient(x2, 2.0);
            model.Objective.SetCoefficient(x3, -3.0);
            model.Objective.SetCoefficient(x4, 4.0);
            model.Objective.SetCoefficient(x5, -5.0);

            model.ObjectiveSense = ObjectiveSense.Minimise;


            double[] coefs = { 1.0, 0.0, -3.0, 0.0, 4.0 };
            model.Add(-2.0 <= Expression.ScalarProduct(coefs, xArray) <= Double.MaxValue);

            System.Collections.Generic.LinkedList<double> morecoefs = new System.Collections.Generic.LinkedList<double>(new double[] { 0, 3, 0, -5, 0 });

            model.Add(Expression.ScalarProduct(morecoefs, xArray) >= 3);
            model.Add(5 * x3 - 6 * x5 >= -4.0);
            model.Add(5.0 <= 2 * x1 - 4 * x2 + 6 * x4 <= Double.MaxValue);

            solver.Solve();

            //this problem is primal infeasible and dual infeasible, but depending on the algorithm, at least one
            // of these two should be proven.
            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(solver.IsProvenPrimalInfeasible || solver.IsProvenDualInfeasible);

            r1.Enabled = false;
            solver.Solve();

            // now, the problem is primal feasible but unbounded. 
            // The dual is infeasible because of the first dual constraint -> because of the first primal variable.
            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(solver.IsProvenDualInfeasible);
        }

        /// <summary>
        /// Test default variable bounds: no given bound sets [0, inf)
        /// </summary>
        /// <param name="model"></param>
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest15(Type solverType)
        {
            Console.WriteLine("SonnetTest15");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x1 = new Variable();
            Variable x2 = new Variable();

            model.Objective = (x1 - x2);
            model.ObjectiveSense = ObjectiveSense.Minimise;
            model.Add(x1 == -1);

            solver.Solve();


            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(solver.IsProvenPrimalInfeasible || solver.IsProvenDualInfeasible);
        }

        /// <summary>
        /// This tests the resolve: in a min. problem, increase the objective function coefs
        /// of variables that are _not_ in the initial solution shouldnt change in the resolve, and the resolve must be quick (pref. 0 iterations)
        /// </summary>
        /// <param name="model"></param>
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest16(Type solverType)
        {
            Console.WriteLine("SonnetTest16");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Random rnd = new Random(1);

            const int nVars = 400;
            const int nCons = 400;
            const int nVarsPerCon = 5;

            Expression obj = new Expression();
            Variable[] xArray = new Variable[nVars];
            for (int i = 0; i < nVars; i++)
            {
                xArray[i] = new Variable("var" + i);
                obj.Add(1.0, xArray[i]);
            }
            model.Objective = Expression.Sum(xArray);
            model.ObjectiveSense = ObjectiveSense.Minimise;

            Constraint[] conArray = new Constraint[nCons];
            for (int j = 0; j < nCons; j++)
            {
                Expression expr = new Expression();
                for (int i = 0; i < nVarsPerCon; i++)
                {
                    int v = rnd.Next(nVars);
                    double coef = (double)rnd.Next(1, 40);
                    expr.Add(coef, xArray[v]);
                }
                if (j % 1000 == 0) Console.WriteLine("Constructed " + j + " constraints.");
                conArray[j] = expr >= 100.0;
                conArray[j].Name = "con" + j.ToString();
            }
            model.Add(conArray);
            solver.Generate();


            solver.Solve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            int firstIterationCount = solver.OsiSolver.getIterationCount();

            Objective objective = model.Objective;
            // in this min. problem, increasing the objective function value of a variable that was at its lower bound shouldnt change the solution
            foreach (Variable var in xArray)
            {
                double oldcoef = objective.GetCoefficient(var);
                if (var.Value == 0.0 && oldcoef > 0.0)
                {
                    objective.SetCoefficient(var, oldcoef * 1.1);
                    break;
                }
            }

            solver.Resolve();


            int secondIterationCount = solver.OsiSolver.getIterationCount();
            Assert.IsTrue(secondIterationCount < 5);	// allow for a few iteration, but basically, 0 should be enough
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest17(Type solverType)
        {
            Console.WriteLine("SonnetTest17");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", 0.0, 10.0);
            Variable x1 = new Variable("x1", 0.0, 10.0);

            Constraint con0 = x0 * 2 + x1 * 1 <= 10;
            Constraint con1 = x0 * 1 + x1 * 3 <= 15;

            model.Add("con0", con0);

            solver.Generate();

            model.Add("con1", con1);

            Objective obj = model.Objective = (x0 * 3 + x1 * 1);

            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();
            int iterationCount1 = solver.IterationCount;


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 5.0));

            Objective obj2 = new Objective(x0 + x1);
            model.Objective = obj2;

            Assert.IsTrue(!obj.AssignedTo(solver));

            solver.Resolve();
            int iterationCount2 = solver.IterationCount;


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0) &&
                Utils.EqualsDouble(x1.Value, 4.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 15.0));


            obj2.SetCoefficient(x0, 3);
            model.Objective.SetCoefficient(x1, 2);

            Assert.IsTrue(Utils.EqualsDouble(obj2.GetCoefficient(x1), 2.0));
            Assert.IsTrue(Utils.EqualsDouble(obj2.GetCoefficient(x0), 3.0));
            obj2.SetCoefficient(x1, 1);

            solver.Resolve();
            int iterationCount3 = solver.IterationCount;


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 5.0));

            model.Objective = obj;

            solver.Resolve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 5.0) &&
                Utils.EqualsDouble(x1.Value, 0.0));

            Assert.IsTrue(Utils.EqualsDouble(model.GetConstraint("con0").Value, 10.0) &&
                Utils.EqualsDouble(model.GetConstraint("con1").Value, 5.0));

        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest18(Type solverType)
        {
            Console.WriteLine("SonnetTest18");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x1 = new Variable("x1");
            Variable x2 = new Variable("x2");
            Variable x3 = new Variable("x3");

            model.Add(x1 * 2 + x2 * 1 <= 10);
            model.Add(x1 * 1 + x2 * 3 <= 15);

            solver.Generate();

            model.Objective = (3.0 * x1 + 1.0 * x2 - x3);
            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Solve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x1.Value, 5.0) &&
                Utils.EqualsDouble(x2.Value, 0.0) &&
                Utils.EqualsDouble(x3.Value, 0.0));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest19(Type solverType)
        {
            Console.WriteLine("SonnetTest19");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x1 = new Variable("x1");
            Variable x2 = new Variable("x2");
            Variable x3 = new Variable("x3");

            model.Add(x1 * 2 + x2 * 1 <= 10);
            model.Add(x1 * 1 + x2 * 3 <= 15);

            solver.Generate();

            model.Objective = (3.0 * x1 + 1.0 * x2 - x3);
            model.ObjectiveSense = (ObjectiveSense.Maximise);

            solver.Solve();


            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(x1.Value, 5.0) &&
                Utils.EqualsDouble(x2.Value, 0.0) &&
                Utils.EqualsDouble(x3.Value, 0.0));

            // TEST ALL HINTPARAMETERS and HINTSTRENGTHS
            foreach (OsiHintParam hintParam in Enum.GetValues(typeof(OsiHintParam)))
            {
                if (hintParam == OsiHintParam.OsiLastHintParam) continue;

                if (hintParam == OsiHintParam.OsiDoReducePrint) continue; // let's not mess with that

                bool yesNoTest = false;
                OsiHintStrength strengthTest = OsiHintStrength.OsiHintIgnore;
                solver.OsiSolver.getHintParam(hintParam, out yesNoTest, out strengthTest); // returns only if the given parameter and strength are valid values; otherwise exception

                for (int j = 0; j < (int)OsiHintStrength.OsiForceDo; j++) // SKIP ForceDo
                {
                    OsiHintStrength hintStrength = (OsiHintStrength)j;

                    // test get and set the hint param at given strength to TRUE
                    solver.OsiSolver.setHintParam(hintParam, true, hintStrength);
                    solver.OsiSolver.getHintParam(hintParam, out yesNoTest, out strengthTest);

                    Assert.IsTrue(yesNoTest);
                    Assert.IsTrue(strengthTest == hintStrength);

                    // test get and set the hint param at given strength to FALSE
                    solver.OsiSolver.setHintParam(hintParam, false, hintStrength);
                    solver.OsiSolver.getHintParam(hintParam, out yesNoTest, out strengthTest);

                    Assert.IsTrue(!yesNoTest);
                    Assert.IsTrue(strengthTest == hintStrength);
                }

                solver.OsiSolver.setHintParam(hintParam, false, OsiHintStrength.OsiHintIgnore);
            }
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest20(Type solverType)
        {
            Console.WriteLine("SonnetTest20");


            /*
            With this matrix we have a primal/dual infeas problem. Leaving the first
            row makes it primal feas, leaving the first col makes it dual feas. 
            (JWG but leaving the first col out of the primal representation will
            make the primal unfeasible..)
            All vars are >= 0

            obj:-1  2 -3  4 -5 (min)

                0 -1  0  0 -2  >=  1
                1  0 -3  0  4  >= -2
                0  3  0 -5  0  >=  3
                0  0  5  0 -6  >= -4
                2 -4  0  6  0  >=  5 
				
            <=>
            primal:
            max 1 -2  3 -4  5
                0  1  0  0  2  <= -1
               -1  0  3  0 -4  <=  2
                0 -3  0  5  0  <= -3
                0  0 -5  0  6  <=  4
               -2  4  0 -6  0  <= -5 
			
            dual:
            min-1  2 -3  4 -5
                0 -1  0  0 -2  >=  1 (*)
                1  0 -3  0  4  >= -2 	
                0  3  0 -5  0  >=  3
                0  0  5  0 -6  >= -4
                2 -4  0  6  0  >=  5
                */

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x1, x2, x3, x4, x5;

            List<Variable> xArray = new List<Variable>(5);
            for (int i = 0; i < 5;)
            {
                Variable x = new Variable();
                xArray.Add(x); // adds at the end!
                x.Name = "x" + (++i);
            }

            x1 = xArray[0];
            x2 = xArray[1];
            x3 = xArray[2];
            x4 = xArray[3];
            x5 = xArray[4];

            Expression expr = Expression.Sum(xArray);
            Expression expr2 = -1.0 * expr;
            Objective obj = model.Objective = (-1.0 * expr);

            RangeConstraint r1 = (RangeConstraint)model.Add(1 <= -1 * x2 + 0 * x3 - 2 * x5 <= Double.MaxValue);

            solver.Generate();

            model.Objective.SetCoefficient(x2, 2.0);
            model.Objective.SetCoefficient(x3, -3.0);
            model.Objective.SetCoefficient(x4, 4.0);
            model.Objective.SetCoefficient(x5, -5.0);

            model.ObjectiveSense = ObjectiveSense.Minimise;

            double[] coefs = { 1.0, 0.0, -3.0, 0.0, 4.0 };
            RangeConstraint r2 = (RangeConstraint)model.Add(-2.0 <= xArray.ScalarProduct(coefs) <= Double.MaxValue);

            Constraint r3 = model.Add(3 * x2 - 5 * x4 >= 3);
            Constraint r4 = model.Add(5 * x3 - 6 * x5 >= -4.0);
            RangeConstraint r5 = (RangeConstraint)model.Add(5.0 <= 2 * x1 - 4 * x2 + 6 * x4 <= Double.MaxValue);

            solver.Solve();

            //this problem is primal infeasible and dual infeasible, but depending on the algorithm, at least one
            // of these two should be proven.
            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(solver.IsProvenPrimalInfeasible || solver.IsProvenDualInfeasible);

            r1.Enabled = false;
            solver.Solve();

            // now, the problem is primal feasible but unbounded. 
            // The dual is infeasible because of the first dual constraint -> because of the first primal variable.
            Assert.IsTrue(!solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(solver.IsProvenDualInfeasible);
        }

        /// <summary>
        /// Test the Model::Contains(constraint)
        /// </summary>
        /// <param name="model"></param>
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest21(Type solverType)
        {
            Console.WriteLine("SonnetTest21 - test Model::Contains(constraint)");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", 4, model.Infinity);
            Variable x1 = new Variable("x1", 3, model.Infinity);

            model.Objective = (3 * x0 + 1 * x1);

            Constraint r0 = x0 * 2 + x1 * 1 <= 10;
            Constraint r1 = x0 * 1 + x1 * 3 <= 15;
            Constraint r2 = x0 * 1 + x1 * 1 <= 10;

            model.Add(r0);

            model.ObjectiveSense = (ObjectiveSense.Maximise);

            Assert.IsTrue(model.Contains(r0));
            Assert.IsTrue(!model.Contains(r1));

            model.Add(r1);
            Assert.IsTrue(!solver.IsRegistered(r1));
            Assert.IsTrue(model.Contains(r1));

            solver.Solve();

            model.Add(r2);

            Assert.IsTrue(model.Contains(r0));
            Assert.IsTrue(solver.Contains(r0));
            Assert.IsTrue(solver.IsRegistered(r0));
            Assert.IsTrue(solver.IsRegistered(r1));
            Assert.IsTrue(!solver.IsRegistered(r2));

            Assert.IsTrue(model.Contains(r2));


        }

        /// <summary>
        /// Test the Model::Contains(constraint)
        /// </summary>
        /// <param name="model"></param>
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest22(Type solverType)
        {
            Console.WriteLine("SonnetTest22");

            Model model = new Model();
            Solver solver;
            int value;

            solver = new Solver(model, typeof(COIN.OsiClpSolverInterface));
            solver.OsiSolver.setIntParam(OsiIntParam.OsiMaxNumIteration, 1234);
            solver.OsiSolver.getIntParam(OsiIntParam.OsiMaxNumIteration, out value);
            Assert.IsTrue(value == 1234);

            solver = new Solver(model, typeof(COIN.OsiCbcSolverInterface));// new: we DONT copy parameters
            solver.OsiSolver.setIntParam(OsiIntParam.OsiMaxNumIteration, 1234);
            solver.OsiSolver.getIntParam(OsiIntParam.OsiMaxNumIteration, out value);
            Assert.IsTrue(value == 1234);

            solver = new Solver(model, typeof(COIN.OsiClpSolverInterface)); // new: we DONT copy parameters
            solver.OsiSolver.setIntParam(OsiIntParam.OsiMaxNumIteration, 1234);
            solver.OsiSolver.getIntParam(OsiIntParam.OsiMaxNumIteration, out value);
            Assert.IsTrue(value == 1234);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest23(Type solverType)
        {
            Console.WriteLine("SonnetTest23");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable();
            Variable x1 = new Variable();
            model.Add(18 * x0 >= 100);
            model.Objective = (x0 + x1);
            model.ObjectiveSense = ObjectiveSense.Minimise;

            solver.OsiSolver.setHintParam(OsiHintParam.OsiDoPresolveInInitial, true, OsiHintStrength.OsiHintDo);
            solver.OsiSolver.setHintParam(OsiHintParam.OsiDoDualInInitial, false, OsiHintStrength.OsiHintDo);

            solver.Generate();
            solver.Resolve(); // !

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 5.55555555));
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Level(), 5.55555555));
            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest24(Type solverType)
        {
            Console.WriteLine("SonnetTest24");

            Model model = Model.New("expect-feasible.mps"); // added file to project, "Copy Always"
            Solver solver = new Solver(model, solverType);

            //string bin = FindBinParent(Environment.CurrentDirectory);
            //string filePath = (bin != null && bin.Length > 0) ? bin + "\\..\\" : "";
            Assert.IsTrue(model != null); // added file to project, "Copy Always"

            solver.Generate();
            solver.Resolve();

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 5.55555555));
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Level(), 5.55555555));
            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            solver.UnGenerate();
            solver.Solve();

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 5.55555555));
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Level(), 5.55555555));
            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest25(Type solverType)
        {
            Console.WriteLine("SonnetTest25");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable();
            x0.Type = VariableType.Integer;
            Variable x1 = new Variable();
            model.Add(x0 + 2 * x1 <= 3.9);
            model.Objective = (x0 + x1);
            model.ObjectiveSense = ObjectiveSense.Maximise;

            solver.Generate();
            int numElements = solver.OsiSolver.getNumElements();
            int numInts = solver.OsiSolver.getNumIntegers();
            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
            Assert.IsTrue(numElements == solver.OsiSolver.getNumElements());
            Assert.IsTrue(numInts == solver.OsiSolver.getNumIntegers());

            // with resetaftermipsolve the objectivevalue() doesnt work.
            //Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, model.ObjectiveValue()));
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 3.45));
            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0));
            Assert.IsTrue(Utils.EqualsDouble(x1.Value, 0.45));

            model.Add(x0 <= 2.0);
            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 2.95));
            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 2.0));
            Assert.IsTrue(Utils.EqualsDouble(x1.Value, 0.95));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest26(Type solverType)
        {
            Console.WriteLine("SonnetTest26");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable();
            x0.Type = VariableType.Integer;
            Variable x1 = new Variable();
            model.Add(x0 + 2 * x1 <= 3.9);
            model.Objective = (x0 + x1);
            model.ObjectiveSense = (ObjectiveSense.Maximise);

            // FOR CLP, we have to supply upper bounds on the integer variables!
            // note: you dont really want to use CLP's B&B anyway..
            //x0.Upper = 1000.0; // should be fixed now.

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 3.45));
            Assert.IsTrue(Utils.EqualsDouble(x0.Value, 3.0));
            Assert.IsTrue(Utils.EqualsDouble(x1.Value, 0.45));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest26b(Type solverType)
        {
            Console.WriteLine("SonnetTest26b");
            if (solverType != typeof(OsiCbcSolverInterface)) return;

            Model model = Model.New("egout.mps");
            Solver solver = new Solver(model, solverType);

            solver.Generate();
            int numElements = solver.OsiSolver.getNumElements();
            int numInts = solver.OsiSolver.getNumIntegers();

            solver.Solve();

            // this fails.. why does the CBC Reset/save after mip not work??
            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 568.1007));
            Assert.IsTrue(numElements == solver.OsiSolver.getNumElements());
            Assert.IsTrue(numInts == solver.OsiSolver.getNumIntegers());
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest26c(Type solverType)
        {
            Console.WriteLine("SonnetTest26c : Cbc with Osi BranchAndBound");
            if (solverType != typeof(OsiCbcSolverInterface)) return;

            Model model = Model.New("egout.mps");
            Solver solver = new Solver(model, solverType);
            var osiCbc = solver.OsiSolver as OsiCbcSolverInterface;
            if (osiCbc != null)
            {
                osiCbc.SetCbcSolverArgs("-branchAndBound");
            }

            solver.Generate();
            int numElements = solver.OsiSolver.getNumElements();
            int numInts = solver.OsiSolver.getNumIntegers();

            solver.Solve();

            // this fails.. why does the CBC Reset/save after mip not work??
            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 568.1007));
            Assert.IsTrue(numElements == solver.OsiSolver.getNumElements());
            Assert.IsTrue(numInts == solver.OsiSolver.getNumIntegers());
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest27(Type solverType)
        {
            Console.WriteLine("SonnetTest27");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0");
            Variable x1 = new Variable("x1");
            Variable x2 = new Variable("x2");
            Variable x3 = new Variable("x3");
            Variable x4 = new Variable("x4");
            Variable x5 = new Variable("x5");
            Variable x6 = new Variable("x6");
            Variable h = new Variable("h");

            model.Add(h >= x0);
            model.Add(h >= x1);
            model.Add(h >= x2);
            model.Add(h >= x3);

            model.Add(Math.Sqrt(2.0) * x0 + Math.PI * x1 <= 6 * Math.PI - 1);
            model.Add(Math.Sqrt(3.0) * x2 + Math.E * x5 <= 5.2 * Math.PI);
            model.Add(Math.Sqrt(7.0) * x3 + Math.Cos(1.3) * x4 <= 5.2 * Math.PI - 1);
            model.Add(Math.Sqrt(6.0) * x2 + Math.Cos(2) * x3 >= 1.9 * Math.PI);
            model.Add(Math.Sqrt(4.0) * x5 + Math.Cos(1) * x2 >= 2.1 * Math.PI);
            model.Add(Math.Sqrt(1.7) * x1 + Math.Cos(1.6) * x6 >= 1.6 * Math.PI - 1);

            model.Objective = (x0 + x1 + x2 + x3);
            model.ObjectiveSense = (ObjectiveSense.Maximise);

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            x0.Lower = x0.Upper = x0.Value;
            x1.Lower = x1.Upper = x1.Value;
            x2.Lower = x2.Upper = x2.Value;
            x3.Lower = x3.Upper = x3.Value;

            model.Objective = (1.0 * h);
            model.ObjectiveSense = (ObjectiveSense.Minimise);

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            x4.Lower = x4.Upper = x4.Value;
            x5.Lower = x5.Upper = x5.Value;
            x6.Lower = x6.Upper = x6.Value;

            model.Objective = (x0 + x1 + x2 + x3 + x4 + x5 + x6);
            model.ObjectiveSense = (ObjectiveSense.Minimise);

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest28(Type solverType)
        {
            Console.WriteLine("SonnetTest28");

            Variable[] variables;
            Model model = Model.New("egout.mps", out variables);
            Assert.IsTrue(model != null);
            Solver solver = new Solver(model, solverType);
            solver.Generate();

            foreach (Variable variable in variables)
            {
                variable.Type = VariableType.Continuous;
            }

            solver.Solve(true);

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 149.58876622009566));

            model.Objective = (Expression.Sum(variables));
            model.ObjectiveSense = (ObjectiveSense.Minimise);

            foreach (Variable variable in variables)
            {
                variable.Freeze();
            }

            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("WarmStart")]
        public void SonnetTest29(Type solverType)
        {
            Console.WriteLine("SonnetTest29 : WarmStart test");

            Model model = new Model();
            Solver solver = new Solver(model, solverType);

            Variable x0 = new Variable("x0", 0, model.Infinity);
            Variable x1 = new Variable("x1", 0, model.Infinity);

            Objective obj = model.Objective = (3 * x0 + 1 * x1);

            Constraint r0 = x0 * 2 + x1 * 1 <= 10;
            Constraint r1 = x0 * 1 + x1 * 3 <= 15;
            Constraint r2 = x0 * 1 + x1 * 1 <= 10;

            model.Add("r0", r0);
            model.Add("r1", r1);
            model.Add("r2", r2);
            model.Objective = (obj);

            solver.Maximise(true);
            int iterationCount = solver.IterationCount;
            WarmStart warmStart = solver.GetWarmStart();

            // Now reset solver, and use warmStart
            if (solver.OsiSolverFullName.Contains("OsiCpx"))
            {
                // OsiCpx emptyWarmStart doesnt work as expected
                solver.Dispose();
                solver = new Solver(model, solverType);
            }
            else
            {
                // With Empty WarmStart, should have same number of iterations
                WarmStart emptyWarmStart = solver.GetEmptyWarmStart();
                solver.SetWarmStart(emptyWarmStart);
            }

            solver.Maximise(true);
            Assert.IsTrue(iterationCount >= solver.IterationCount); // should be EQUAL, but for some reason new iteration count is sometimes lower.

            // With WARM warmstart, should have zero iterations to optimality!
            solver.SetWarmStart(warmStart);
            solver.Maximise(true);
            Assert.IsTrue(solver.IterationCount == 0);
        }

        private void BuildChipsModel(Model model, out Variable[] variables)
        {
            Variable xp = new Variable("xp"); // plain chips
            Variable xm = new Variable("xm"); // Mexican chips
            Variable xt = new Variable("xt", 1.0, 2.0); // test

            Objective P = new Objective(2 * xp + 1.5 * xm);
            Constraint slicing = 2 * xp + 4 * xm <= 345;
            Constraint frying = 4 * xp + 5 * xm <= 480;
            Constraint packing = 4 * xp + 2 * xm + xt <= 330;

            model.Objective = (P);
            model.ObjectiveSense = (ObjectiveSense.Maximise);

            model.Add("SlicingConstraint", slicing);
            model.Add("FryingConstraint", frying);
            model.Add("PackingConstraint", packing);

            variables = new Variable[] { xp, xm, xt };
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("WarmStart")]
        public void SonnetTest30(Type solverType)
        {
            Console.WriteLine("SonnetTest30 : WarmStart test 2");
            Model model = new Model();
            Variable[] variables;
            BuildChipsModel(model, out variables);

            Solver solver = new Solver(model, solverType);

            model.GetConstraint("PackingConstraint").Enabled = false;

            solver.Maximise();
            int iterationCount = solver.IterationCount;
            WarmStart warmStart = solver.GetWarmStart();

            model.GetConstraint("PackingConstraint").Enabled = true;

            // but use the old warmstart!
            solver.SetWarmStart(warmStart);

            bool passed = false;
            try
            {
                solver.Maximise();
                passed = true;
            }
            catch
            {
                passed = false;
            }

            // test if this doesnt cause a crash..
            Assert.IsTrue(passed);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1199:Nested code blocks should not be used", Justification = "For simplicity here.")]
        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("WarmStart")]
        public void SonnetTest31(Type solverType)
        {
            Console.WriteLine("SonnetTest31 : WarmStart test 3");

            string filename = "brandy.mps";
            //first,use coin wrapper directly
            {
                object rawSolver = Activator.CreateInstance(solverType);
                COIN.OsiSolverInterface solver = (COIN.OsiSolverInterface)rawSolver;
                solver.readMps(filename);

                solver.initialSolve();
                var warmStart = solver.getWarmStart();

                //now, restart but with limited iter max.
                solver = (COIN.OsiSolverInterface)Activator.CreateInstance(solverType);
                solver.readMps(filename);

                int maxNumIteration;
                solver.getIntParam(OsiIntParam.OsiMaxNumIteration, out maxNumIteration);
                solver.setIntParam(OsiIntParam.OsiMaxNumIteration, 2);
                solver.initialSolve();

                solver.setIntParam(OsiIntParam.OsiMaxNumIteration, maxNumIteration);
                solver.setWarmStart(warmStart);
                solver.resolve();
                int iterationCount = solver.getIterationCount();
                Assert.IsTrue(iterationCount == 0);
            }

            // now with sonnet
            {
                Model model = Model.New(filename);
                Solver solver = new Solver(model, solverType);
                solver.Solve(true);

                WarmStart warmStart = solver.GetWarmStart();

                // Now reset solver, and use warmStart
                if (solver.OsiSolverFullName.Contains("OsiCpx"))
                {
                    // OsiCpx emptyWarmStart doesnt work as expected
                    solver.Dispose();
                    solver = new Solver(model, solverType);
                }
                else
                {
                    // With Empty WarmStart, should have same number of iterations
                    WarmStart emptyWarmStart = solver.GetEmptyWarmStart();
                    solver.SetWarmStart(emptyWarmStart);
                }

                int maxNumIteration;
                solver.OsiSolver.getIntParam(OsiIntParam.OsiMaxNumIteration, out maxNumIteration);
                solver.OsiSolver.setIntParam(OsiIntParam.OsiMaxNumIteration, 2);
                solver.Solve(true);

                solver.OsiSolver.setIntParam(OsiIntParam.OsiMaxNumIteration, maxNumIteration);

                // With WARM warmstart, should have zero iterations to optimality!
                solver.SetWarmStart(warmStart);
                solver.Resolve(true);
                Assert.IsTrue(solver.IterationCount == 0);
            }
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("WarmStart")]
        public void SonnetTest32(Type solverType)
        {
            Console.WriteLine("SonnetTest34 : WarmStart test 4: add variables to the model between warmstarts");

            Variable[] variables;
            Model model = new Model();
            Solver solver = new Solver(model, new OsiCbcSolverInterface());

            solver.OsiSolver.setHintParam(OsiHintParam.OsiDoDualInInitial, false, OsiHintStrength.OsiHintDo);
            solver.OsiSolver.setHintParam(OsiHintParam.OsiDoDualInResolve, false, OsiHintStrength.OsiHintDo);
            BuildChipsModel(model, out variables);
            Objective originalObjective = model.Objective;

            solver.Solve();
            WarmStart warmStart = solver.GetWarmStart();

            model.GetConstraint("PackingConstraint").Enabled = false;
            model.Objective = variables.Sum();
            solver.Resolve();

            model.GetConstraint("PackingConstraint").Enabled = true;
            model.Objective = originalObjective;

            Variable x1 = new Variable();
            Variable x2 = new Variable();
            model.Add(x1 + 2 * x2 <= 5);
            solver.Generate(); // this is necessary, because otherwise these variables are not yet registered when we set the coefficients (below)
            // alternatively, assign the originalObjective *after* setting the coefficients (since orig. obj is currently NOT active in the model)

            originalObjective.SetCoefficient(x1, 1.0);
            originalObjective.SetCoefficient(x2, 1.0);

            solver.SetWarmStart(warmStart);
            solver.Resolve();

            Assert.IsTrue(solver.IterationCount <= 1);
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Objective")]
        public void SonnetTest33(Type solverType)
        {
            Console.WriteLine("SonnetTest33 - testing objective values");

            Model model = new Model();

            Variable x0 = new Variable();
            x0.Type = VariableType.Integer;
            Variable x1 = new Variable();
            model.Add(x0 + 2 * x1 <= 3.9);
            model.Objective = (x0 + x1 + 10.0);
            model.ObjectiveSense = (ObjectiveSense.Maximise);

            Solver solver = new Solver(model, solverType);
            solver.Solve();

            Assert.IsTrue(solver.IsProvenOptimal);
            Assert.IsTrue(!solver.IsProvenPrimalInfeasible);
            Assert.IsTrue(!solver.IsProvenDualInfeasible);

            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 13.45)); // with Constant
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Level(), 13.45)); // with Constant
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Objective")]
        public void SonnetTest34(Type solverType)
        {
            Console.WriteLine("SonnetTest34 - testing objective values");

            Model model = Model.New("MIP-124725.mps");
            Solver solver = new Solver(model, solverType);
            solver.Minimise();
            Assert.IsTrue(Utils.EqualsDouble(model.Objective.Value, 124725));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod]
        public void SonnetTest37(Type solverType)
        {
            Console.WriteLine("SonnetTest37 - Test MIP solve followed by solving lp-relaxation");

            Model m = new Model();
            Variable x = new Variable(VariableType.Integer);
            Variable y = new Variable(VariableType.Integer);

            m.Add("con1", 0 <= 1.3 * x + 3 * y <= 10);
            m.Objective = x + 2 * y;

            Solver s = new Solver(m, solverType);
            s.Maximise();

            Console.WriteLine("Status: Optimal? " + s.IsProvenOptimal);
            Console.WriteLine("Status: x = " + x.Value);
            Console.WriteLine("Status: y = " + y.Value);
            Console.WriteLine("Status: obj = " + m.Objective.Value);

            Assert.IsTrue(Utils.EqualsDouble(m.Objective.Value, 7));


            s.Maximise(true);
            Console.WriteLine("Status: Optimal? " + s.IsProvenOptimal);
            Console.WriteLine("Status: x = " + x.Value);
            Console.WriteLine("Status: y = " + y.Value);
            Console.WriteLine("Status: obj = " + m.Objective.Value);
            Assert.IsTrue(Utils.EqualsDouble(m.Objective.Value, 7.6923076923076907));

            m.GetConstraint("con1").Lower = 4;

            s.Minimise();
            Console.WriteLine("Status: Optimal? " + s.IsProvenOptimal);
            Console.WriteLine("Status: x = " + x.Value);
            Console.WriteLine("Status: y = " + y.Value);
            Console.WriteLine("Status: obj = " + m.Objective.Value);
            Assert.IsTrue(Utils.EqualsDouble(m.Objective.Value, 3.0));

            s.Minimise(true);
            Console.WriteLine("Status: Optimal? " + s.IsProvenOptimal);
            Console.WriteLine("Status: x = " + x.Value);
            Console.WriteLine("Status: y = " + y.Value);
            Console.WriteLine("Status: obj = " + m.Objective.Value);
            Assert.IsTrue(Utils.EqualsDouble(m.Objective.Value, 2.66666666666));
        }

        [DynamicData(nameof(Utils.TestSolverTypes), typeof(Utils))]
        [TestMethod, TestCategory("Export")]
        public void SonnetTest38(Type solverType)
        {
            Console.WriteLine("SonnetTest38 - Test create new model from export files");

            Model modelOrig = Model.New("MIP-124725.mps");
            Assert.IsTrue(modelOrig.Objective.Name.Equals("OBJROW"));

            string stringOrig = modelOrig.ToString();
            Solver solver = new Solver(modelOrig, solverType);
            solver.NameDiscipline = 2;

            // Check exporting MPS
            // OsiCpx writeMps is unreliable
            if (!solver.OsiSolverFullName.Contains("OsiCpx"))
            {
                solver.Export("export-test.mps");

                // Check that the original and re-imported .mps model are identical
                Model modelMps = Model.New("export-test.mps");
                modelMps.Name = modelOrig.Name;
                string stringMps = modelMps.ToString();
                Assert.IsTrue(Utils.EqualsString(stringOrig, stringMps));
                //File.Delete("export-test.mps");
            }
            else { Console.WriteLine("Skipping test for export/import MPS"); }

            // Check LP export
            // For OsiCbc, the lp export determines there's a problem with the Names, and reverts back to default.
            if (!solver.OsiSolverFullName.Contains("OsiCbc"))
            {
                solver.Export("export-test.lp"); // export LP has strict rules about valid row&col names!! Check log for warnings!

                // Check that the original and re-imported .lp model are identical
                Model modelLp = Model.New("export-test.lp");
                modelLp.Name = modelOrig.Name;
                string stringLp = modelLp.ToString();
                Assert.IsTrue(Utils.EqualsString(stringOrig, stringLp));

                //File.Delete("export-test.lp");
            }
            else { Console.WriteLine("Skipping test for export/import LP"); }
        }

        [TestMethod, TestCategory("Extensions")]
        public void SonnetTest39()
        {
            Console.WriteLine("SonnetTest39 - Test ForAll extensions for constraint sets");

            string[] toys = { "truck", "airplane", "ball" };
            int[] colors = { 1, 2, 3 };

            int N = 5;
            Dictionary<string, Variable> y = toys.ToMap(t => new Variable("y_" + t));
            var x = toys.ToMap(t => colors.ToMap(c => new Variable("x_(" + t + "_" + c + ")")));
            // It is not _required_ to construct variables and constraints using such expressions

            string constraintsTest = "Limit_truck : y_truck <= 5\r\nLimit_airplane : y_airplane <= 5\r\nLimit_ball : y_ball <= 5\r\n";
            // for each type of toy, the total number produced must be less or equal to N
            // forall t in toys: y_t <= N
            var cons1 = toys.ForAll("Limit", t => y[t] <= N);
            Console.WriteLine(" - Testing ForAll (simple)");
            Assert.IsTrue(Utils.EqualsString(cons1.ToItemString(), constraintsTest));
            // m.Add(cons1);

            var cons1b = toys.ForAll(t => new Constraint(y[t] <= N) { Name = "Limit_" + t });
            Console.WriteLine(" - Testing ForAll with names");
            Assert.IsTrue(Utils.EqualsString(cons1b.ToItemString(), constraintsTest));

            var cons1c = toys.ForAll(t => (y[t] <= N).WithName("Limit_" + t));
            Console.WriteLine(" - Testing ForAll with using .WithName");
            Assert.IsTrue(Utils.EqualsString(cons1c.ToItemString(), constraintsTest));

            // ForAll is not much more than native .NET Select 
            // Note we use new Constraint copy constructor to allow immedate initialization
            // Also, note that the constraint is not actually created until the cons2 set is used, since below merely defines the projection
            var cons2 = toys.Select(t => new Constraint(y[t] <= N) { Name = "Limit_" + t });
            // Not necessary here to do m.Add(cons2);
            Console.WriteLine(" - Testing Select vs ForAll");
            Assert.IsTrue(Utils.EqualsString(cons2.ToItemString(), constraintsTest));

            List<Constraint> cons3 = new List<Constraint>();
            foreach (string t in toys)
            {
                Expression expr = new Expression();
                expr.Add(1.0, y[t]);
                Constraint con = expr <= N;
                con.Name = "Limit_" + t;
                cons3.Add(con);
            }
            Console.WriteLine(" - Testing using foreach to create constraints");
            Assert.IsTrue(Utils.EqualsString(cons3.ToItemString(), constraintsTest));


            //  for each type of toy, the total number of that type has to be less or equal to y_t
            // forall t in toys: sum_{c in colors} x_{t, c} <= y_t    ( "SumOverColors_t" )
            string cons4Test = "SumOverColors_truck : x_(truck_1) + x_(truck_2) + x_(truck_3) <= y_truck\r\nSumOverColors_airplane : x_(airplane_1) + x_(airplane_2) + x_(airplane_3) <= y_airplane\r\nSumOverColors_ball : x_(ball_1) + x_(ball_2) + x_(ball_3) <= y_ball\r\n";
            var cons4 = toys.ForAll("SumOverColors", t => colors.Sum(c => x[t][c]) <= y[t]);
            Console.WriteLine(" - Testing using ForAll with Sum");
            Assert.IsTrue(Utils.EqualsString(cons4.ToItemString(), cons4Test));

            // For each type of toy and each color, at most 2 can be made
            // forall t in toys, forall c in colors : x[t][c] <= 2  ( "AtMostTwo_t_c")

            string constraintsTest2 = "AtMostTwo_truck_1 : x_(truck_1) <= 2\r\nAtMostTwo_truck_2 : x_(truck_2) <= 2\r\nAtMostTwo_truck_3 : x_(truck_3) <= 2\r\nAtMostTwo_airplane_1 : x_(airplane_1) <= 2\r\nAtMostTwo_airplane_2 : x_(airplane_2) <= 2\r\nAtMostTwo_airplane_3 : x_(airplane_3) <= 2\r\nAtMostTwo_ball_1 : x_(ball_1) <= 2\r\nAtMostTwo_ball_2 : x_(ball_2) <= 2\r\nAtMostTwo_ball_3 : x_(ball_3) <= 2\r\n";

            var cons5 = toys.ForAll("AtMostTwo", t => colors.ForAll("", c => x[t][c] <= 2));
            Console.WriteLine(" - Testing using multiple ForAll with names");
            Assert.IsTrue(Utils.EqualsString(cons5.ToItemString(), constraintsTest2));

            var cons5b = toys.ForAll(t => colors.ForAll(c => new Constraint(x[t][c] <= 2) { Name = "AtMostTwo_" + t + "_" + c }));
            Console.WriteLine(" - Testing using three ForAll nested with names");
            Assert.IsTrue(Utils.EqualsString(cons5b.ToItemString(), constraintsTest2));

            // all loops use SelectMany except for the inner-most which uses Select
            // In this way, the final collection is an enumerable of constraints.
            var cons6 = toys.SelectMany(t => colors.Select(c => new Constraint(x[t][c] <= 2) { Name = "AtMostTwo_" + t + "_" + c }));
            Console.WriteLine(" - Testing using SelectMany and Select instead of ForAll");
            Assert.IsTrue(Utils.EqualsString(cons6.ToItemString(), constraintsTest2));

            // test the ForAllDo, which is merely a foreach(t in toys)..
            Console.WriteLine(" - Testing ForAllDo and ToMap for variable names");
            toys.ForAllDo(t => Assert.IsTrue(y[t].Name == "y_" + t));
        }

        [TestMethod, TestCategory("Import")]
        public void SonnetTest40()
        {
            Console.WriteLine("SonnetTest40 - Load large MPS");

            // A bug was found in Model.New (load MPS) when the garbage collector finalized osiClp earlier than expected. 
            // This tests that scenario.

            SonnetLog.Default.LogLevel = 4;

            Sonnet_CoinNativeTests cbcNative = new Sonnet_CoinNativeTests();
            string mpsfile = cbcNative.MipLibDir + "\\" + "nw04";

            Model model = Model.New(mpsfile, ".mps");

            Assert.IsNotNull(model, $"Model file {mpsfile} failed to load.");
        }

    }
}

