using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using COIN;
using Sonnet;

namespace SonnetExamples.Example5
{
    /// <summary>
    /// Machine Assignment
    /// Taken from Task 2.5 - Orientatie Besliskunde (FdEW - RuL)
    /// 
    /// Description:
    /// An agricultural cooperation of four grain producers maintains a fleet
    /// of harvesting machines. The fleet consists of three types of machines: Senior 1, 2 and 3.
    /// The cooperation owns six vehicles of each of the types.
    /// Historical data shows that there are large differences between 
    /// the maintenance costs of the three types of machines. Futhermore, then 
    /// maintenance costs depend on the producer where the machine is used.
    /// The average weekfly maintenance costs per machine per producer are 
    /// shown in the following table:
    /// Costs             Senior
    /// Producer    1       2       3
    /// A           $16     $13     $15
    /// B           $20     $29     $25
    /// C           $40     $38     $42
    /// D           $37     $49     $45
    /// 
    /// The capacity (in ton of grain) of each of the types of machines also 
    /// depends on produre where the machine is operated, as shown in the 
    /// following tabel:
    /// Capacity          Senior
    /// Producer    1       2       3
    /// A           250    200    230
    /// B           270    350    300
    /// C           490    470    520
    /// D           460    630    550
    /// 
    /// For the upcoming season, the weekly production of grain is 
    /// estimate to be 950 (A), 1200 (B), 1500 (C) and 1800 (D) ton.
    /// Determine the optimal assignment of the machines to producers
    /// for the coming season.
    /// </summary>
    public class Example5
    {
        public void Run()
        {
            double objValue;
            string solutionString;
            Run(out objValue, out solutionString);
        }

        public void Run(out double objValue, out string solutionString)
        {
            Producer producerA = new Producer("A", 950);
            Producer producerB = new Producer("B", 1200);
            Producer producerC = new Producer("C", 1500);
            Producer producerD = new Producer("D", 1800);
            Producer[] producers = { producerA, producerB, producerC, producerD };

            Machine senior1 = new Machine("senior1", 6);
            Machine senior2 = new Machine("senior2", 6);
            Machine senior3 = new Machine("senior3", 6);
            Machine[] machines = { senior1, senior2, senior3 };

            MachineAssignment[] machineAssignments = {
                new MachineAssignment(senior1, producerA, 16, 250), 
                new MachineAssignment(senior1, producerB, 20, 270), 
                new MachineAssignment(senior1, producerC, 40, 490), 
                new MachineAssignment(senior1, producerD, 37, 460), 
                new MachineAssignment(senior2, producerA, 13, 200), 
                new MachineAssignment(senior2, producerB, 29, 350), 
                new MachineAssignment(senior2, producerC, 38, 470), 
                new MachineAssignment(senior2, producerD, 49, 630), 
                new MachineAssignment(senior3, producerA, 15, 230), 
                new MachineAssignment(senior3, producerB, 25, 300), 
                new MachineAssignment(senior3, producerC, 42, 520), 
                new MachineAssignment(senior3, producerD, 45, 550), 
            };

            AgriCoop agriCoop = new AgriCoop(machines, producers, machineAssignments);
            agriCoop.Initialize();
            agriCoop.Build();
            agriCoop.Solve();

            agriCoop.SaveXml("agricoop.xml");

            AgriCoop agriCoop2 = AgriCoop.LoadXml("agricoop.xml");
            agriCoop2.Initialize();
            agriCoop2.Build();
            agriCoop2.Solve(out objValue, out solutionString);
        }
    }

    [DataContract(Namespace = "http://www.agricoop.org")]
    public class AgriCoop
    {
        [DataMember(Order = 1)] // set order to ensure that these Machines will be the base for the references used in MachineAssignments
        public IEnumerable<Machine> Machines { get; private set; }
        [DataMember(Order = 2)] // set order to ensure that these Producers will be the base for the references used in MachineAssignments
        public IEnumerable<Producer> Producers { get; private set; }
        [DataMember(Order = 3)]
        public IEnumerable<MachineAssignment> MachineAssignments { get; private set; }

        private Model model;

        public AgriCoop(IEnumerable<Machine> machines, IEnumerable<Producer> producers, IEnumerable<MachineAssignment> machineAssignments)
        {
            Machines = machines;
            Producers = producers;
            MachineAssignments = machineAssignments;
        }

        public void SaveXml(string filePath)
        {
            using (XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;

                DataContractSerializer ser = new DataContractSerializer(typeof(AgriCoop));
                
                // Using references for Machine and Producer means that the Id attribute is used, which is defined in the MS Serialization namespace
                // To get cleaner XML, we declare this namespace first.
                // ser.WriteObject(writer, this); // replace by following lines.
                ser.WriteStartObject(writer, this);
                writer.WriteAttributeString("xmlns", "z", null, "http://schemas.microsoft.com/2003/10/Serialization/");
                ser.WriteObjectContent(writer, this);
                ser.WriteEndObject(writer);

                writer.Close();
            }
        }

        public static AgriCoop LoadXml(string filePath)
        {
            using (XmlTextReader reader = new XmlTextReader(filePath))
            {
                reader.DtdProcessing = DtdProcessing.Prohibit;

                DataContractSerializer ser = new DataContractSerializer(typeof(AgriCoop));
                AgriCoop deserialized = (AgriCoop)ser.ReadObject(reader, true);
                reader.Close();

                return deserialized;
            }
        }

        public void Initialize()
        {
            model = new Model();
            model.Name = "MachineAssignment";

            foreach (Machine machine in Machines)
            {
                machine.Initialize();
            }

            foreach (Producer producer in Producers)
            {
                producer.Initialize();
            }

            foreach (MachineAssignment machineAssignment in MachineAssignments)
            {
                machineAssignment.Initialize();

                // we can choose between storing the machine assignments twice (2*n),
                // or enumerating when building the constraints (n*n).
                machineAssignment.Machine.AddMachineAssignment(machineAssignment);
                machineAssignment.Producer.AddMachineAssignment(machineAssignment);
            }
        }

        public void Build()
        {
            model.Objective = MachineAssignments.Sum(a => a.Cost * a.Assign);

            foreach (Machine machine in Machines)
            {
                machine.Build(model);
            }

            foreach (Producer producer in Producers)
            {
                producer.Build(model);
            }

            foreach (MachineAssignment machineAssignment in MachineAssignments)
            {
                machineAssignment.Build(model);
            }
        }

        public void Solve()
        {
            double objValue;
            string solutionString;
            Solve(out objValue, out solutionString);
        }

        public void Solve(out double objValue, out string solutionString)
        {
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface), "MachineAssignmentMIP");
            solver.Minimise();

            objValue = model.Objective.Value;
            solutionString = solver.ToSolutionString();

            Console.WriteLine(solver.ToSolutionString());
        }
    }

    [DataContract(Namespace = "http://www.agricoop.org", IsReference = true)]
    public class Machine
    {
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public int Available { get; private set; }

        private List<MachineAssignment> machineAssignments;

        public Machine(string name, int available)
        {
            Name = name;
            Available = available;
        }

        public override string ToString()
        {
            return Name;
        }

        public void Initialize()
        {
            machineAssignments = new List<MachineAssignment>();
        }

        public void AddMachineAssignment(MachineAssignment machineAssignment)
        {
            machineAssignments.Add(machineAssignment);
        }

        public void Build(Model model)
        {
            string name = string.Format("Availability[{0}]", this);
            model.Add(name,
                machineAssignments.Sum(a => a.Assign) <= Available);
        }
    }

    [DataContract(Namespace = "http://www.agricoop.org", IsReference = true)]
    public class Producer
    {
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public double Requirement { get; private set; }

        private List<MachineAssignment> machineAssignments;

        public Producer(string name, double requirement)
        {
            Name = name;
            Requirement = requirement;
        }

        public override string ToString()
        {
            return Name;
        }

        public void Initialize()
        {
            machineAssignments = new List<MachineAssignment>();
        }

        public void AddMachineAssignment(MachineAssignment machineAssignment)
        {
            machineAssignments.Add(machineAssignment);
        }

        public void Build(Model model)
        {
            string name = string.Format("Requirements[{0}]", this);
            model.Add(name,
                machineAssignments.Sum(a => a.Capacity * a.Assign) >= Requirement);
        }
    }

    [DataContract(Namespace = "http://www.agricoop.org")]
    public class MachineAssignment
    {
        [DataMember]
        public Machine Machine { get; private set; }
        [DataMember]
        public Producer Producer { get; private set; }
        [DataMember]
        public double Cost { get; private set; }
        [DataMember]
        public double Capacity { get; private set; }

        public Variable Assign { get; private set; }


        public MachineAssignment(Machine machine, Producer producer, double cost, double capacity)
        {
            Machine = machine;
            Producer = producer;
            Cost = cost;
            Capacity = capacity;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Producer.Name, Machine.Name);
        }

        public void Initialize()
        {
            string name = string.Format("Assign[{0}]", this);
            Assign = new Variable(name, VariableType.Integer);
        }

        public void Build(Model model)
        {
            // nothing to be done.
        }
    }
}