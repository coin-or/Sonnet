// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sonnet;

namespace SonnetExamples.Example6
{
    /// <summary>
    /// Find a way to fit several words into a squar of given size.
    /// For example, the words ONE, TWO, TEN, EAN and NBE give solution
    /// T W O 
    /// E A N
    /// N B E
    /// </summary>
    public class Example
    {
        static int n = 3; // size of the nxn square (n = 3 -> 3x3)
        static string[] Words = { "one", "two", "ten", "ean", "nbe" };
        static string Alfabet = GetAlfabet(Words);
        
        public Solver Run()
        {
            double objValue;
            string solutionString;
            return Run(out objValue, out solutionString);
        }

        public Solver Run(out double objValue, out string solutionString)
        {
            Model m = new Model();

            Dictionary<char, Variable[,]> x = CreateXVariables(n);
            Dictionary<string, Variable[][,]> y = CreateYVariables(n);

            // every (i,j) at most (exactly) one letter:
            // forall i,j :sum_a x[a][i,j] <= 1
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Expression sum = new Expression();
                    foreach (char a in Alfabet)
                    {
                        sum.Add(x[a][i, j]);
                    }
                    m.Add(sum == 1);
                }
            }

            // each words starts somewhere
            // foreach w : sum_{i,j} yh^w_ij + yv^w_ij >= 1
            Expression obj = new Expression();
            foreach (string w in Words)
            {
                Variable[][,] yw = y[w];
                m.Add(Expression.Sum(yw[0].Cast<Variable>()) + Expression.Sum(yw[1].Cast<Variable>()) == 1);
                obj.Add(Expression.Sum(yw[0].Cast<Variable>()) + Expression.Sum(yw[1].Cast<Variable>()));
            }

            // if a words start at yh^w_ij, then all subsequent letters fit
            // yh^w_i,j <= x^(w_0)_i,j
            // yh^w_i,j <= x^(w_1)_i,j+1
            // ..
            // yh^w_i,j <= x^(w_n-1)_i,j+n-1
            // and
            // yv^w_i,j <= x^(w_0)_i,j
            // yv^w_i,j <= x^(w_1)_i+1,j
            // ..
            // yv^w_i,j <= x^(w_n-1)_i+n-1,j
            foreach (string w in Words)
            {
                Console.WriteLine("Building constraints for " + w);
                // build yh
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        var yhwij = y[w][0][i, j];
                        if (j + w.Length - 1 >= n)
                        {
                            yhwij.Upper = 0.0;
                        }
                        else
                        {
                            for (int wj = 0; wj < w.Length; wj++)
                            {
                                char a = w[wj];
                                m.Add(yhwij <= x[a][i, j + wj]);
                            }
                        }
                    }
                }
                // build yv
                for (int j = 0; j < n; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        var yvwij = y[w][1][i, j];
                        if (i + w.Length - 1 >= n)
                        {
                            yvwij.Upper = 0.0;
                        }
                        else
                        {
                            for (int wi = 0; wi < w.Length; wi++)
                            {
                                char a = w[wi];
                                m.Add(yvwij <= x[a][i + wi, j]);
                            }
                        }
                    }
                }
            }

            m.Objective = obj;
            m.ObjectiveSense = ObjectiveSense.Minimise;
            Solver s = new Solver(m, typeof(COIN.OsiCbcSolverInterface));
            s.Solve();

            Console.WriteLine(s.ToSolutionString());

            Console.WriteLine(s.IsFeasible());

            ToSolutionString(n, x);

            objValue = m.Objective.Value;
            solutionString = s.ToSolutionString();

            return s;
        }

        private static void ToSolutionString(int n, Dictionary<char, Variable[,]> x)
        {
            for (int i = 0; i < n; i++)
            {
                string line = "";
                for (int j = 0; j < n; j++)
                {
                    foreach (char a in Alfabet)
                    {
                        if (x[a][i, j].Value >= 0.99)
                        {
                            line += a + " ";
                        }
                    }
                }
                Console.WriteLine(line);
            }
        }

        static string GetAlfabet(string[] words)
        {
            string alfabet = "";
            foreach (char a in "abcdefghijklmnopqrstuvwxyz")
            {
                foreach (string word in words)
                {
                    if (word.Contains(a))
                    {
                        alfabet += a;
                        break;
                    }
                }
            }
            return alfabet;
        }

        static Dictionary<char, Variable[,]> CreateXVariables(int n)
        {
            Dictionary<char, Variable[,]> x = new Dictionary<char, Variable[,]>();
            foreach (char a in Alfabet)
            {
                x[a] = new Variable[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        string name = string.Format("x^({0})[{1},{2}]", a, i, j);
                        x[a][i, j] = new Variable(name, 0, 1, VariableType.Integer);
                    }
                }
            }
            return x;
        }

        static Dictionary<string, Variable[][,]> CreateYVariables(int n)
        {
            Dictionary<string, Variable[][,]> y = new Dictionary<string, Variable[][,]>();
            foreach (string w in Words)
            {
                y[w] = new Variable[2][,];
                y[w][0] = new Variable[n, n];
                y[w][1] = new Variable[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        string name = string.Format("^({0})[{1},{2}]", w, i, j);
                        y[w][0][i, j] = new Variable("yh"+name, 0, 1, VariableType.Integer);
                        y[w][1][i, j] = new Variable("yv"+name, 0, 1, VariableType.Integer);
                    }
                }
            }
            return y;
        }
    }
}
