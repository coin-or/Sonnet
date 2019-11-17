using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COIN;
using Sonnet;

namespace SonnetExamples
{
    /// <summary>
    /// Investment planning
    /// Taken from Task 2.1 - Orientatie Besliskunde (FdEW - RuL)
    /// </summary>
    public class Example1
    {
        public enum Projects { proj1, proj2, proj3, proj4, proj5 };
        public enum Years { year1, year2, year3, year4, year5, year6 };

        public void Run()
        {
            Console.WriteLine("Example1 - Optimising investments");

            var YearsList = Enum.GetValues(typeof(Years)).OfType<Years>().ToList();
            var ProjectsList = Enum.GetValues(typeof(Projects)).OfType<Projects>().ToList();

            double interest_rate = 0.1;
            Dictionary<Years, double> budget = new Dictionary<Years, double>();
            budget[Years.year1] = 500;
            budget[Years.year2] = 650;
            budget[Years.year3] = 0;
            budget[Years.year4] = 0;
            budget[Years.year5] = 0;
            budget[Years.year6] = 0;

            Dictionary<Projects, Dictionary<Years, double>> cashflow = new Dictionary<Projects, Dictionary<Years, double>>();
            #region cashflow table
            cashflow[Projects.proj1] = new Dictionary<Years, double>();
            cashflow[Projects.proj1][Years.year1] = -150;
            cashflow[Projects.proj1][Years.year2] = -50;
            cashflow[Projects.proj1][Years.year3] = 0;
            cashflow[Projects.proj1][Years.year4] = 90;
            cashflow[Projects.proj1][Years.year5] = 550;
            cashflow[Projects.proj1][Years.year6] = 0;
            cashflow[Projects.proj2] = new Dictionary<Years, double>();
            cashflow[Projects.proj2][Years.year1] = -80;
            cashflow[Projects.proj2][Years.year2] = -180;
            cashflow[Projects.proj2][Years.year3] = -200;
            cashflow[Projects.proj2][Years.year4] = 120;
            cashflow[Projects.proj2][Years.year5] = 210;
            cashflow[Projects.proj2][Years.year6] = 520;
            cashflow[Projects.proj3] = new Dictionary<Years, double>();
            cashflow[Projects.proj3][Years.year1] = -220;
            cashflow[Projects.proj3][Years.year2] = -300;
            cashflow[Projects.proj3][Years.year3] = -120;
            cashflow[Projects.proj3][Years.year4] = 150;
            cashflow[Projects.proj3][Years.year5] = 290;
            cashflow[Projects.proj3][Years.year6] = 550;
            cashflow[Projects.proj4] = new Dictionary<Years, double>();
            cashflow[Projects.proj4][Years.year1] = -110;
            cashflow[Projects.proj4][Years.year2] = -60;
            cashflow[Projects.proj4][Years.year3] = 70;
            cashflow[Projects.proj4][Years.year4] = 130;
            cashflow[Projects.proj4][Years.year5] = 0;
            cashflow[Projects.proj4][Years.year6] = 0;
            cashflow[Projects.proj5] = new Dictionary<Years, double>();
            cashflow[Projects.proj5][Years.year1] = 0;
            cashflow[Projects.proj5][Years.year2] = -100;
            cashflow[Projects.proj5][Years.year3] = 10;
            cashflow[Projects.proj5][Years.year4] = 90;
            cashflow[Projects.proj5][Years.year5] = 220;
            cashflow[Projects.proj5][Years.year6] = 630;
            #endregion

            Dictionary<Projects, Variable> invest = ProjectsList.ToMap(p => new Variable(0, 1, VariableType.Integer) { Name = "invest_" + p });
            Dictionary<Years, Variable> cash = YearsList.ToMap(y => new Variable() { Name = "case_" + y });
            Dictionary<Projects, Variable> fraction = ProjectsList.ToMap(p => new Variable() { Name = "fraction_" + p }); ;

            Model model = new Model("Investement_Planning");
            foreach (Years y in Enum.GetValues(typeof(Years)))
            {
                Expression sum = new Expression();
                foreach (Projects p in Enum.GetValues(typeof(Projects)))
                {
                    sum.Add(cashflow[p][y] * fraction[p]);
                }
                if (y > Years.year1) sum.Add(cash[y - 1]);
                sum.Add(budget[y]);

                string conName = "CashBalance[" + y + "]";
                model.Add(conName, cash[y] == sum * (1 + interest_rate));
            }

            foreach (Projects p in Enum.GetValues(typeof(Projects)))
            {
                model.Add("invest_fraction[" + p + "]", fraction[p] <= invest[p]);
            }

            model.Add(fraction[Projects.proj5] <= fraction[Projects.proj4]);
            model.Add(invest[Projects.proj1] + invest[Projects.proj2] <= 1);
            model.Add(invest[Projects.proj3] + invest[Projects.proj4] >= 1);
            model.Add(invest.Sum() >= 2);
            model.Add(invest.Sum() <= 3);

            model.Objective = cash[Years.year6];

            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface));
            solver.Maximise();

            Console.WriteLine(solver.ToSolutionString());
        }
    }
}
