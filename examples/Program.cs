using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sonnet;


namespace SonnetExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            	COIN.OsiCbcSolverInterface solver = new COIN.OsiCbcSolverInterface();
	//::OsiSolverInterface * solver;
	//solver = new ::OsiCpxSolverInterface(
		 
		 // Some of the underlying code sets logLevel on "third-party" message handlers.
	// However, the solver->passInMessageHandler causes all these handlers to be the same!!
	// which means the overall logLevel gets changed..

	solver.passInMessageHandler(new COIN.CoinMessageHandler());
	solver.messageHandler().setLogLevel(2);
	//solver->setIntParam(COIN::OsiIntParam::OsiNameDiscipline, 2);
	//int status = solver.readMps("D:\\SVN_ROOT\\Sonnet-project\\sonnet\\MSVisualStudio\\v10\\SonnetTest\\bin\\Debug\\alfa6.mps");
	//solver.branchAndBound();

    Sonnet.Model m = Sonnet.Model.New("D:\\SVN_ROOT\\Sonnet-project\\sonnet\\MSVisualStudio\\v10\\SonnetTest\\bin\\Debug\\alfa6.mps");
    COIN.OsiCbcSolverInterface cbc = new COIN.OsiCbcSolverInterface();
    cbc.SetCbcSolverArgs("-branchAndBound");
    Sonnet.Solver s = new Sonnet.Solver(m, cbc);
    s.LogLevel = 2;
    s.Solve();


            /*
            Example1 ex1 = new Example1();
            ex1.Run();

            Example2 ex2 = new Example2();
            ex2.Run();

            Example3 ex3 = new Example3();
            ex3.Run();

            Example4.Example4 ex4 = new Example4.Example4();
            ex4.Run();

            Example5.Example5 ex5 = new Example5.Example5();
            ex5.Run();

            //Example6.Example ex6
             */
        }
    }
}
