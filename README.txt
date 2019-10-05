SONNET builds on top of the well-known COIN libraries for optimisation (see http://www.coin-or.org). 
It brings the power of COIN to the .NET realm.

SONNET consists of to parts: 
1) A .NET library that consists of .NET wrapper classes around existing COIN classes such as OsiSolverInterface. 
This library is called SonnetWrapper.
2) A .NET library implemented in C# that adds a powerful (rudimentary) modelling API with classes like Model, Constraint, Variable, Expression, etc, and overloaded operators. 
This library is called Sonnet.

The Sonnet library allows for code like

// using Sonnet;
Model model = new Model();
Variable x = new Variable();
Variable y = new Variable();
model.Add(2 * x + 3 * y <= 10);
model.Objective = 3 * x + y;
Solver solver = new Solver(model, typeof(COIN.OsiClpSolverInterface));
solver.Maximise();

(Actual C# code)

