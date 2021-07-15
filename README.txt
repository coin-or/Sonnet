SONNET is a modelling API and wrapper for COIN-OR mixed integer linear programming classes for the Microsoft .NET Framework.

For the wrapper, SONNET builds on top of well-known COIN-OR C++ projects, notably Cbc, Clp and Osi, and provides these objects for use in .NET. Using this wrapper, SONNET also adds a modelling API with classes like Model, Constraint, Variable, Expression, etc., and overloaded operators.
The SONNET libraries allow for code like this:

  // C# code
  // using Sonnet;  
  Model model = new Model();
  Variable x = new Variable();
  Variable y = new Variable();
  model.Add(2 * x + 3 * y <= 10);
  model.Objective = 3 * x + y;
  Solver solver = new Solver(model, typeof(COIN.OsiClpSolverInterface));
  solver.Maximise();

You can find the latest binaries and sources from CI builds on AppVeyor on
  https://ci.appveyor.com/project/coin-or/sonnet/build/artifacts
You can find SONNET on 
  https://github.com/coin-or/SONNET
In particular, the latest binary release:
  https://github.com/coin-or/SONNET/releases
Simply add the x86 or x64 libraries of Sonnet.dll and SonnetWrapper.dll to your References in Visual Studio.
Requires .NET Framework 4.x or .NET 5.
