using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sonnet;

namespace SonnetExamples.Example7
{
    class Example
    {
        public enum Lengths { L300, L330, L360, L390, L420, L450, L480, L510, L540, L570, L600 };
        public List<Lengths> lengths = Enum.GetValues(typeof(Lengths)).OfType<Lengths>().ToList();

        public enum Minmax { min, max};         // set Minmax := { "min", "max" };

        public void Run()
        {
            Model m = new Model();

            // param group1[Lengths] := <"300"> 0, <"330"> 0,  <"360"> 0,  <"390"> 0,  <"420"> 1,  <"450"> 1,  <"480"> 1, <"510"> 0,  <"5
            // 40"> 0,  <"570"> 0,  <"600"> 0;
            Dictionary<Lengths, int> group1 = new Dictionary<Lengths, int>()
            {
                { Lengths.L300, 0}, { Lengths.L330, 0}, { Lengths.L360, 0}, { Lengths.L390, 0},
                { Lengths.L420, 1}, { Lengths.L450, 1}, { Lengths.L480, 1}, { Lengths.L510, 0},
                { Lengths.L540, 0}, { Lengths.L570, 0}, { Lengths.L600, 0}
            };

            // param group1_minmax[Lengths * Minmax] :=    | "min" , "max" | | "330"  |  0 ,  100  | | "360"  |  0 ,  100  | | "390"  |  0
            //  ,  100  | | "420"  |  0 ,  100  | | "450"  |  0 ,  100  | | "480"  |  0 ,  100  | | "510"  |  0 ,  100  | | "540"  |  0 ,  1
            // 00  | | "570"  |  0 ,  100  | | "600"  |  0 ,  100  |;

            Dictionary<Minmax, Dictionary<Lengths, int>> group1_minmax = new Dictionary<Minmax, Dictionary<Lengths, int>>();
            group1_minmax[Minmax.min] = new Dictionary<Lengths, int>()
            {
                { Lengths.L330, 0}, { Lengths.L360, 0}, { Lengths.L390, 0}, { Lengths.L420, 0},
                { Lengths.L450, 0}, { Lengths.L480, 0}, { Lengths.L510, 0}, { Lengths.L540, 0},
                { Lengths.L570, 0}, { Lengths.L600, 0}
            };

            group1_minmax[Minmax.max] = new Dictionary<Lengths, int>()
            {
                { Lengths.L330, 100}, { Lengths.L360, 100}, { Lengths.L390, 100}, { Lengths.L420, 100},
                { Lengths.L450, 100}, { Lengths.L480, 100}, { Lengths.L510, 100}, { Lengths.L540, 100},
                { Lengths.L570, 100}, { Lengths.L600, 100},
            };

            // param target_distribution[Lengths] := <"300"> 100, <"330"> 120,  <"360"> 150,  <"390"> 180,  <"420"> 210,  <"450"> 250,  <
            // "480"> 210,  <"510"> 180,  <"540"> 150,  <"570"> 120,  <"600"> 100;
            Dictionary<Lengths, int> target_distribution = new Dictionary<Lengths, int>()
            {
                { Lengths.L300, 100}, { Lengths.L330, 120}, { Lengths.L360, 150}, { Lengths.L390, 180},
                { Lengths.L420, 210}, { Lengths.L450, 250}, { Lengths.L480, 210}, { Lengths.L510, 180},
                { Lengths.L540, 150}, { Lengths.L570, 120}, { Lengths.L600, 100}
            };

            // param storage[Lengths] := <"300"> 50, <"330"> 120,  <"360"> 200,  <"390"> 10,  <"420"> 420,  <"450"> 500,  <"480"> 180,  <
            // "510"> 120,  <"540"> 50, 1 <"570"> 300,  <"600"> 200;
            Dictionary<Lengths, int> storage = new Dictionary<Lengths, int>()
            {
                { Lengths.L300, 50}, { Lengths.L330, 120}, { Lengths.L360, 200}, { Lengths.L390, 10},
                { Lengths.L420, 420}, { Lengths.L450, 500}, { Lengths.L480, 180}, { Lengths.L510, 120},
                { Lengths.L540, 50}, { Lengths.L570, 300}, { Lengths.L600, 200}
            };

            // param package_size[Lengths] := <"300"> 3.5, <"330"> 3.8,  <"360"> 4.2,  <"390"> 4.6,  <"420"> 5.0,  <"450"> 5.3,  <"480"> 
            // 5.7,  <"510"> 6.0,  <"540"> 6.3,  <"570"> 6.6,  <"600"> 7.0;
            Dictionary<Lengths, double> package_size = new Dictionary<Lengths, double>()
            {
                { Lengths.L300, 3.5 }, { Lengths.L330, 3.8 }, { Lengths.L360, 4.2 }, { Lengths.L390, 4.6 },
                { Lengths.L420, 5.0 }, { Lengths.L450, 5.3 }, { Lengths.L480, 5.7 }, { Lengths.L510, 6.0 },
                { Lengths.L540, 6.3 }, { Lengths.L570, 6.6 }, { Lengths.L600, 7.0 }
            };
            
            int order_size = 100;// param order_size := 100;
            int group1_min = 30; // param group1_min := 30;
            int group1_max = 70; // param group1_max := 70;
            int group1_nmin = 3;// param group1_nmin := 3;
            int group1_nmax = 3;// param group1_nmax := 3;
            int group1_single_min = 5; // param group1_single_min := 5;
            int group1_single_max = 100;// param group1_single_max := 100;
            int M = 300000; // param M := 300000;
            
            // var x[Lengths] integer >= 0 ;
            var x = lengths.ToMap(l => new Variable("x_" + l, 0.0, double.MaxValue, VariableType.Continuous));
            // var n[Lengths] binary ;
            var n = lengths.ToMap(l => new Variable("n_" + l, 0.0, 1.0, VariableType.Continuous));
            // subto obj: delta_squares == sum<i> in Lengths :  (target_distribution[i] - storage[i] + x[i] * package_size[i]) ^2;
            Objective delta_squares = lengths.Sum(i => (target_distribution[i] - storage[i] + x[i] * package_size[i]).Squared());
            m.Objective = delta_squares;
            m.ObjectiveSense = ObjectiveSense.Minimise;

            // var delta_squares;
            // minimize total_error: delta_squares;

            // subto order_total: sum<i> in Lengths : x[i] * package_size[i] <= order_size ;
            m.Add("order_total", lengths.Sum(i => x[i] * package_size[i]) <= order_size);

            // subto group1min: sum<i> in Lengths with group1[i] == 1:  x[i] * package_size[i] >= group1_min;
            m.Add("group1min", lengths.Where(i => group1[i] == 1).Sum(i => x[i] * package_size[i]) >= group1_min);

            // subto group1max: sum<i> in Lengths with group1[i] == 1:  x[i] * package_size[i] <= group1_max;
            m.Add("group1max", lengths.Where(i => group1[i] == 1).Sum(i => x[i] * package_size[i]) <= group1_max);

            // subto group1nmin: sum<i> in Lengths with group1[i] == 1: n[i] >= group1_nmin;
            m.Add("group1nmin", lengths.Where(i => group1[i] == 1).Sum(i => n[i]) >= group1_nmin);

            // subto group1nmax: sum<i> in Lengths with group1[i] == 1: n[i] <= group1_nmax;
            m.Add("group1nmax", lengths.Where(i => group1[i] == 1).Sum(i => n[i]) <= group1_nmax);

            // subto iflength1: forall<i> in Lengths do x[i] <= M* n[i];
            m.Add(lengths.ForAll("iflength1", i => x[i] <= M * n[i]));

            // subto iflength2: forall<i> in Lengths do x[i] >= - M* (1 - n[i]) ;
            m.Add(lengths.ForAll("iflength2", i => x[i] >= -M * (1 - n[i])));

            // subto mingroup1single_case_n_out_m: forall<i> in Lengths with group1[i] == 1:        
            // sum <j> in Lengths with group1[j] == 1:     
            //      if i == j then x[j] * package_size[j] * (100 - group1_single_min) / 100      
            //      else - x[j] * package_size[j] * group1_single_min / 100
            //      end      >= - (1 - n[i]) * M;
            m.Add(lengths.Where(i => group1[i] == 1).ForAll("mingroup1single_case_n_out_m", i =>
                  lengths.Where(j => group1[j] == 1).Sum(j => (i == j) ?
                    (x[j] * package_size[j] * (100 - group1_single_min) / 100) :
                    (-1.0 * x[j] * package_size[j] * group1_single_min / 100)) >= -(1 - n[i]) * M));

            // subto maxgroup1single_case_n_out_m: forall<i> in Lengths with group1[i] == 1: 
            // sum <j> in Lengths with group1[j] == 1:
            //      if i == j then x[j] * package_size[j] * (100 - group1_single_max) / 100 
            //      else - x[j] * package_size[j] * group1_single_max / 100 
            //      end      <= 0;
            m.Add(lengths.Where(i => group1[i] == 1).ForAll("maxgroup1single_case_n_out_m", i =>
                  lengths.Where(j => group1[j] == 1).Sum(j => (i == j) ?
                    (x[j] * package_size[j] * (100 - group1_single_max) / 100) :
                    (-x[j] * package_size[j] * group1_single_max / 100)) <= 0));

            // subto mingroup1single: forall<i> in Lengths with group1[i] == 1:
            //   sum <j> in Lengths with group1[j] == 1:
            //      if i == j then x[j] * package_size[j] * (100 - group1_minmax[i, "min"]) / 100
            //      else - x[j] * package_size[j] * group1_minmax[i, "min"] / 100
            //      end      >= 0;
            m.Add(lengths.Where(i => group1[i] == 1).ForAll("mingroup1single", i =>
                  lengths.Where(j => group1[j] == 1).Sum(j => (i == j) ?
                    (x[j] * package_size[j] * (100 - group1_minmax[Minmax.min][i]) / 100):
                    (-x[j] * package_size[j] * group1_minmax[Minmax.min][i] / 100)) >= 0));

            // subto maxgroup1single: forall<i> in Lengths with group1[i] == 1:
            //   sum<j> in Lengths with group1[j] == 1:
            //      if i == j then x[j] * package_size[j] * (100 - group1_minmax[i, "max"]) / 100
            //      else - x[j] * package_size[j] * group1_minmax[i, "max"] / 100
            //      end      <= 0;
            m.Add(lengths.Where(i => group1[i] == 1).ForAll("maxgroup1single", i =>
                 lengths.Where(j => group1[j] == 1).Sum(j => (i == j) ?
                   (x[j] * package_size[j] * (100 - group1_minmax[Minmax.max][i]) / 100) :
                   (-x[j] * package_size[j] * group1_minmax[Minmax.max][i] / 100)) <= 0));

            Solver s = new Solver(m, typeof(COIN.OsiCbcSolverInterface));
            s.Export("newlengths.mps");
            s.Solve();

            Console.WriteLine(s.ToSolutionString());
        }
    }
}
