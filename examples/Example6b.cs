// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sonnet;

namespace SonnetExamples.Example6b
{
    /// <summary>
    /// Same as Example6, but now using lambda extensions to create variables and constraints
    /// Find a way to fit several words into a squar of given size.
    /// For example, the words ONE, TWO, TEN, EAN and NBE give solution
    /// T W O 
    /// E A N
    /// N B E
    /// Due to the differnt way the model is generated, the order of the variables in the model is 
    /// different from Example6, but the solution is the same.
    /// </summary>
    public class Example
    {
        static int n = 3; // size of the nxn square (n = 3 -> 3x3)
        static string[] Words = { "one", "two", "ten", "ean", "nbe" };
        static char[] Alfabet = GetAlfabet(Words).ToCharArray();
        
        public Solver Run()
        {
            double objValue;
            string solutionString;
            return Run(out objValue, out solutionString);
        }

        public Solver Run(out double objValue, out string solutionString)
        {
            char[] hv = { 'h', 'v' }; // write the word horizontally or vertically

            List<int> positions = new List<int>();
            for (int i = 0; i < n; i++) positions.Add(i);

            Model m = new Model();

            // NOTE: You DON'T _have to_ use code like below to create variables and constraints.
            // Variable x = new Variable(); etc. will work just as well.

            var x = Alfabet.ToMap(a => positions.ToMap(i => positions.ToMap(j =>
                new Variable(string.Format("x^({0})[{1},{2}]", a, i, j), 0, 1, VariableType.Integer)
            )));
            var y = Words.ToMap(w => hv.ToMap(h => positions.ToMap(i => positions.ToMap(j =>
                new Variable(string.Format("y{0}^({1})[{2},{3}]", h, w, i, j), 0, 1, VariableType.Integer)
            ))));

            // every (i,j) at most (exactly) one letter:
            // forall i,j :sum_a x[a][i,j] <= 1
            m.Add(positions.ForAll(i => positions.ForAll(j => Alfabet.Sum(a => x[a][i][j]) == 1)));

            // each words starts somewhere
            // foreach w : sum_{i,j} yh^w_ij + yv^w_ij >= 1
            m.Add(Words.ForAll(w => positions.Sum(i => positions.Sum(j => y[w]['h'][i][j] + y[w]['v'][i][j])) >= 1));

            // obj = sum_w sum_{i,j} yh^w_ij + yv^w_ij
            m.Objective = Words.Sum(w => positions.Sum(i => positions.Sum(j => y[w]['h'][i][j] + y[w]['v'][i][j])));
            m.ObjectiveSense = ObjectiveSense.Minimise;

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

            // Since below we don't use the return, use void ForAllDo rather than enumerable ForAll
            // because ForAll returns a lazy enumerable, which is NOT called unless used.
            Words.ForAllDo(w =>
            {
                Console.WriteLine("Building constraints for " + w);

                positions.ForAllDo(i => positions.ForAllDo(j =>
                {
                    // build yh
                    var yhwij = y[w]['h'][i][j];
                    if (j + w.Length - 1 >= n)
                    {
                        yhwij.Upper = 0.0;
                    }
                    else
                    {
                        for (int wj = 0; wj < w.Length; wj++)
                        {
                            char a = w[wj];
                            m.Add(yhwij <= x[a][i][j + wj]);
                        }
                    }

                    // build yv
                    var yvwij = y[w]['v'][i][j];
                    if (i + w.Length - 1 >= n)
                    {
                        yvwij.Upper = 0.0;
                    }
                    else
                    {
                        for (int wi = 0; wi < w.Length; wi++)
                        {
                            char a = w[wi];
                            m.Add(yvwij <= x[a][i + wi][j]);
                        }
                    }
                }));
            });


            Solver s = new Solver(m, typeof(COIN.OsiCbcSolverInterface));
            s.Solve();

            //Console.WriteLine(s.ToSolutionString())//;

            Console.WriteLine(s.IsFeasible());

            ToSolutionString(x, n);

            objValue = m.Objective.Value;
            solutionString = s.ToSolutionString();
            
            return s;
        }

        private static void ToSolutionString(Dictionary<char, Dictionary<int, Dictionary<int, Variable>>> x, int n)
        {            
            for (int i = 0; i < n; i++)
            {
                string line = "";
                for (int j = 0; j < n; j++)
                {
                    foreach (char a in Alfabet)
                    {
                        if (x[a][i][j].Value >= 0.99)
                        {
                            line += a + " ";
                        }
                    }
                }
                Console.WriteLine(line);
            }
        }

        /// <summary>
        /// Determine the letters used in at least one of the given words
        /// </summary>
        /// <param name="words">Array of words</param>
        /// <returns>String of letters used</returns>
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
    }
}
