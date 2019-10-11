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

            Example6.Example ex6 = new Example6.Example();
            ex6.Run();

            Example6b.Example ex6b = new Example6b.Example();
            ex6b.Run();
        }
    }
}
