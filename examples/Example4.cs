using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using COIN;
using Sonnet;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace SonnetExamples.Example4
{
    /// <summary>
    /// </summary>
    public class Example4
    {
        public void Run()
        {
            List<Day> days = new List<Day>();
            days.Add(new Day(1, 900));
            days.Add(new Day(2, 1440));
            days.Add(new Day(3, 1260));
            days.Add(new Day(4, 1620));
            days.Add(new Day(5, 1296));

            List<Method> methods = new List<Method>();
            methods.Add(new Method("method1", 0.5, 0.4, 1));
            methods.Add(new Method("method2", 0.25, 0.2, 2));
            methods.Add(new Method("method3", 0.1, 0.1, 3));

            LinnenRentalCo linnenRentalCo = new LinnenRentalCo(days, methods, 2.5);
            linnenRentalCo.Initialize();
            linnenRentalCo.Build();
            linnenRentalCo.Solve();
            
            linnenRentalCo.SaveXml("linnenrentalco.xml");

            LinnenRentalCo linnenRentalCo2 = LinnenRentalCo.LoadXml("linnenrentalco.xml");
            linnenRentalCo2.Initialize();
            linnenRentalCo2.Build();
            linnenRentalCo2.Solve();
        }
    }

    [DataContract(Name = "LinnenRentalCo", Namespace = "http://www.linnenrental.com")]
    public class LinnenRentalCo
    {
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public List<Day> Days { get; set; }
        [DataMember]
        public List<Method> Methods { get; set; }

        private Model model;

        public LinnenRentalCo(List<Day> days, List<Method> methods, double price)
        {
            Days = new List<Day>(days);
            Methods = new List<Method>(methods);
            Price = price;
        }

        public void SaveXml(string filePath)
        {
            using (XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;

                DataContractSerializer ser = new DataContractSerializer(typeof(LinnenRentalCo));
                ser.WriteObject(writer, this);
                writer.Close();
            }
        }

        public static LinnenRentalCo LoadXml(string filePath)
        {
            using (XmlTextReader reader = new XmlTextReader(filePath))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(LinnenRentalCo));
                LinnenRentalCo deserialized = (LinnenRentalCo)ser.ReadObject(reader, true);
                reader.Close();

                return deserialized;
            }
        }

        public void Initialize()
        {
            foreach (Day day in Days)
            {
                day.Initialize();
            }

            foreach (Method method in Methods)
            {
                method.Initialize(Days);
            }
        }

        public void Build()
        {
            model = new Model();
            model.Name = "Buy or clean";
            model.Objective = Price * Days.Sum(d => d.Buy) + Methods.Sum(m => m.Cost * m.Clean.Sum());

            foreach (Day day in Days)
            {
                day.Build(model, Days, Methods);
            }

            foreach (Method method in Methods)
            {
                method.Build(model);
            }
        }

        public void Solve()
        {
            if (model == null) Build();

            Solver solver = new Solver(model, typeof(OsiClpSolverInterface));
            solver.Minimise();

            Console.WriteLine(solver.ToSolutionString());
        }
    }

    [DataContract(Name = "Day", Namespace = "http://www.linnenrental.com")]
    public class Day
    {
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public double Demand { get; set; }

        public Variable Buy { get; private set; }
        public Variable Stock { get; private set; }

        public Day(int index, double demand)
        {
            Index = index;
            Demand = demand;
        }

        public void Initialize()
        {
            string buyName = string.Format("Buy[{0}]", ToString());
            Buy = new Variable(buyName);

            string stockName = string.Format("Stock[{0}]", ToString());
            Stock = new Variable(stockName);
        }

        public void Build(Model model, List<Day> days, List<Method> methods)
        {
            string cleaningConName = string.Format("cleaning[{0}]", this);
            model.Add(cleaningConName,
                methods.Select(m => m.Clean[this]).Sum() <= this.Demand);

            string materialBalanceConName = string.Format("materialBalance[{0}]", this);
            if (this.Index == 1) model.Add(materialBalanceConName, this.Buy == this.Stock + this.Demand);
            else
            {
                // sum[(m,dd)| d - dd == duration_m, (1 - loss_m) * clean_{m,dd} ]
                //  + buy_d + stock_{d-1) == stock_d + demand_d
                Constraint con =
                    methods.Sum(m => days.Where(dd => this.Index - dd.Index == m.Duration).Sum(dd => (1 - m.Loss) * m.Clean[dd]))
                    + this.Buy + days.Where(d => d.Index == this.Index - 1).Sum(d => d.Stock)
                    == this.Stock + this.Demand;
                model.Add(materialBalanceConName, con);

                // The above is the same as the following:
                //Expression expr = new Expression();
                //foreach (Method m in methods)
                //{
                //    foreach (Day dd in days)
                //    {
                //        if (day.Index - dd.Index == m.Duration)
                //        {
                //            expr.Add((1 - m.Loss) * m.Clean[dd]);
                //        }
                //    }
                //}
                //expr.Add(day.Buy);
                //foreach (Day dd in days)
                //{
                //    if (dd.Index == day.Index - 1) expr.Add(dd.Stock);
                //}
                //Constraint con = expr == day.Stock + day.Demand;
            }
        }

        public override string ToString()
        {
            return "Day" + Index;
        }
    }

    [DataContract(Name = "Method", Namespace = "http://www.linnenrental.com")]
    public class Method
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Cost { get; set; }
        [DataMember]
        public double Loss { get; set; }
        [DataMember]
        public double Duration { get; set; }

        public Dictionary<Day, Variable> Clean { get; private set; }

        public Method(string name, double cost, double loss, double duration)
        {
            Name = name;
            Cost = cost;
            Loss = loss;
            Duration = duration;
        }

        public void Initialize(IEnumerable<Day> days)
        {
            string varname = string.Format("Clean[{0}]", Name);
            Clean = Variable.New(days, varname);
        }

        public void Build(Model model)
        {
            // nothing to be done
        }

        public override string ToString()
        {
            return Name;
        }
    }
}


