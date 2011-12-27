using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using COIN;
using Sonnet;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace SonnetExamples.Example5
{
    /// <summary>
    /// Machine Assignment
    /// Taken from Task 2.5 - Orientatie Besliskunde (FdEW - RuL)
    /// 
    /// Description:
    /// </summary>
    public class Example5
    {
        public void Run()
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
            agriCoop2.Solve();
        }
    }

    [DataContract(Namespace = "http://www.agricoop.org")]
    public class AgriCoop
    {
        [DataMember(Order = 1)]
        public IEnumerable<Machine> Machines { get; private set; }
        [DataMember(Order = 2)]
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
                //ser.WriteObject(writer, this);

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
            Solver solver = new Solver(model, typeof(OsiCbcSolverInterface), "MachineAssignmentMIP");
            solver.Minimise();

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