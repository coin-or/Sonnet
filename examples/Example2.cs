using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COIN;
using Sonnet;

namespace SonnetExamples
{
    /// <summary>
    /// Personel planning
    /// Taken from Task 2.2 - Orientatie Besliskunde (FdEW - RuL)
    /// </summary>
    public class Example2
    {
        public enum Days { sun, mon, tue, wed, thu, fri, sat };

        public void Run()
        {
            Array DaysArray = Enum.GetValues(typeof(Days));

            Dictionary<Days, double> demand = new Dictionary<Days, double>();
            demand[Days.sun] = 12;
            demand[Days.mon] = 18;
            demand[Days.tue] = 16;
            demand[Days.wed] = 15;
            demand[Days.thu] = 16;
            demand[Days.fri] = 19;
            demand[Days.sat] = 14;

            double shift = 5;

            var emp = Variable.New<Days>("emp", 0, 140, VariableType.Integer);
            Variable totemp = new Variable("totemp");

            Model model = new Model("Personel_Planning");
            model.Add("objective", totemp == emp.Sum());

            int card = DaysArray.Length;
            foreach (Days d in DaysArray)
            {
                Expression sum = new Expression();
                foreach (Days dd in DaysArray)
                {
                    if ((dd <= d && (d - dd) < shift) ||
                        (dd - d > card - shift))
                    {
                        sum.Add(emp[dd]);
                    }
                }

                model.Add(sum >= demand[d]);
            }

            model.Objective = totemp;
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            solver.Minimise();

            Console.WriteLine(solver.ToSolutionString());
        }
    }
}
