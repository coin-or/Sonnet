// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COIN;
using Sonnet;

namespace SonnetExamples
{
    /// <summary>
    /// This example is taken from Paul Williams' book "Model Building in Mathematical Programming" 
    /// Williams_12_1_FoodManufacture.
    /// Thanks to Tim Chippington Derrick.
    /// </summary>
    public class Example3
    {
        public void Run()
        {
            double objValue;
            string solutionString;
            Run(out objValue, out solutionString);
        }
        public void Run(out double objValue, out string solutionString)
        {
            Console.WriteLine("Williams_12_1_FoodManufacture");

            const int numberRawMaterials = 5;
            const int numberMonths = 6;
            List<string> rawMaterials = new List<string> { "Veg1", "Veg2", "Oil1", "Oil2", "Oil3" };
            List<string> months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            double[,] rawMaterialCost = new double[numberRawMaterials, numberMonths];
            // Veg1
            rawMaterialCost[0, 0] = 110.0; // Jan
            rawMaterialCost[0, 1] = 130.0; // Feb
            rawMaterialCost[0, 2] = 110.0; // Mar
            rawMaterialCost[0, 3] = 120.0; // Apr
            rawMaterialCost[0, 4] = 100.0; // May
            rawMaterialCost[0, 5] = 90.0;  // Jun
            // Veg2
            rawMaterialCost[1, 0] = 120.0; // Jan
            rawMaterialCost[1, 1] = 130.0; // Feb
            rawMaterialCost[1, 2] = 140.0; // Mar
            rawMaterialCost[1, 3] = 110.0; // Apr
            rawMaterialCost[1, 4] = 120.0; // May
            rawMaterialCost[1, 5] = 100.0; // Jun
            // Oil1 
            rawMaterialCost[2, 0] = 130.0; // Jan
            rawMaterialCost[2, 1] = 110.0; // Feb
            rawMaterialCost[2, 2] = 130.0; // Mar
            rawMaterialCost[2, 3] = 120.0; // Apr
            rawMaterialCost[2, 4] = 150.0; // May
            rawMaterialCost[2, 5] = 140.0; // Jun
            // Oil2
            rawMaterialCost[3, 0] = 110.0; // Jan
            rawMaterialCost[3, 1] = 90.0;  // Feb
            rawMaterialCost[3, 2] = 100.0; // Mar
            rawMaterialCost[3, 3] = 120.0; // Apr
            rawMaterialCost[3, 4] = 110.0; // May
            rawMaterialCost[3, 5] = 80.0;  // Jun
            // Oil2
            rawMaterialCost[4, 0] = 115.0; // Jan
            rawMaterialCost[4, 1] = 115.0; // Feb
            rawMaterialCost[4, 2] = 95.0;  // Mar
            rawMaterialCost[4, 3] = 125.0; // Apr
            rawMaterialCost[4, 4] = 105.0; // May
            rawMaterialCost[4, 5] = 135.0; // Jun

            double[] hardness = new double[numberRawMaterials] { 8.8, 6.1, 2.0, 4.2, 5.0 };
            double productValue = 150.0;
            double minHardness = 3.0;
            double maxHardness = 6.0;
            bool[] isVeg = new bool[numberRawMaterials] { true, true, false, false, false };
            bool[] isOil = new bool[numberRawMaterials] { false, false, true, true, true };
            double maxVegRefineCapacity = 200.0; // tonnes per month
            double maxOilRefineCapacity = 250.0; // tonnes per month
            double[] maxStorageCapacity = new double[numberRawMaterials] { 1000.0, 1000.0, 1000.0, 1000.0, 1000.0 }; // tonnes
            double[] storageCosts = new double[numberRawMaterials] { 5.0, 5.0, 5.0, 5.0, 5.0 }; // per tonne per month
            double[] initialStock = new double[numberRawMaterials] { 500.0, 500.0, 500.0, 500.0, 500.0 }; // tonnes
            double[] finalStock = new double[numberRawMaterials] { 500.0, 500.0, 500.0, 500.0, 500.0 }; // tonnes

            Model model = new Model();
            Solver solver = new Solver(model, typeof(OsiClpSolverInterface));

            // Create the main decision variables
            Variable[] productionVar = new Variable[numberMonths];
            Variable[,] buyVar = new Variable[numberRawMaterials, numberMonths];
            Variable[,] useVar = new Variable[numberRawMaterials, numberMonths];
            Variable[,] storeVar = new Variable[numberRawMaterials, numberMonths];
            int i, j;
            for (j = 0; j < numberMonths; j++)
            {
                string name = "Produce_" + months[j];
                productionVar[j] = new Variable(name, 0.0, maxVegRefineCapacity + maxOilRefineCapacity, VariableType.Continuous);
                for (i = 0; i < numberRawMaterials; i++)
                {
                    double maxUse = 0.0;
                    if (isVeg[i]) maxUse = maxVegRefineCapacity;
                    if (isOil[i]) maxUse = maxOilRefineCapacity;
                    double maxBuy = maxUse + maxStorageCapacity[i];
                    double maxStore = maxStorageCapacity[i];
                    name = "Buy_" + rawMaterials[i] + "_" + months[j];
                    buyVar[i, j] = new Variable(name, 0.0, maxBuy, VariableType.Continuous);
                    name = "Use_" + rawMaterials[i] + "_" + months[j];
                    useVar[i, j] = new Variable(name, 0.0, maxUse, VariableType.Continuous);
                    name = "Store_" + rawMaterials[i] + "_" + months[j];
                    storeVar[i, j] = new Variable(name, 0.0, maxStore, VariableType.Continuous);
                }
            }

            // Limit the total amount of each type of raw material that can be refined each month
            for (j = 0; j < numberMonths; j++)
            {
                Expression totalVegUseInMonth = new Expression();
                Expression totalOilUseInMonth = new Expression();
                for (i = 0; i < numberRawMaterials; i++)
                {
                    if (isVeg[i])
                        totalVegUseInMonth.Add(useVar[i, j]);
                    if (isOil[i])
                        totalOilUseInMonth.Add(useVar[i, j]);
                }
                Constraint limitVeg = (totalVegUseInMonth <= maxVegRefineCapacity);
                limitVeg.Name = "LimitVegCt_" + months[j];
                Constraint limitOil = (totalVegUseInMonth <= maxVegRefineCapacity);
                limitOil.Name = "LimitOilCt_" + months[j];
                model.Add(limitVeg);
                model.Add(limitOil);
            }

            // Limit the final product hardness to be within the specified range
            for (j = 0; j < numberMonths; j++)
            {
                Expression hardnessInMonth = new Expression();
                for (i = 0; i < numberRawMaterials; i++)
                    hardnessInMonth.Add(hardness[i], useVar[i, j]);
                Constraint maxHardnessCt = (hardnessInMonth - (maxHardness * productionVar[j]) <= 0.0);
                maxHardnessCt.Name = "MaxHardnessCt_" + months[j];
                Constraint minHardnessCt = (hardnessInMonth - (minHardness * productionVar[j]) >= 0.0);
                minHardnessCt.Name = "MinHardnessCt_" + months[j];
                model.Add(maxHardnessCt);
                model.Add(minHardnessCt);
            }

            // Fix the final product quantity to be the sum of the raw materials used for each month
            for (j = 0; j < numberMonths; j++)
            {
                Expression totalRawMaterialUsedInMonth = new Expression();
                for (i = 0; i < numberRawMaterials; i++)
                    totalRawMaterialUsedInMonth.Add(useVar[i, j]);
                Constraint productionBalanceCt = (totalRawMaterialUsedInMonth - productionVar[j] == 0.0);
                productionBalanceCt.Name = "ProductionBalanceCt_" + months[j];
                model.Add(productionBalanceCt);
            }

            // Fix the inventory balance for each product and each month. 
            for (i = 0; i < numberRawMaterials; i++)
            {
                // Note we start here at month 1 as we refer back to the previous month
                // and we handle the starting and ending stock levels separately below
                for (j = 1; j < numberMonths; j++)
                {
                    Constraint inventoryBalanceCt = (storeVar[i, j - 1] + buyVar[i, j] - useVar[i, j] - storeVar[i, j] == 0.0);
                    inventoryBalanceCt.Name = "InventoryBalanceCt_" + rawMaterials[i] + "_" + months[j];
                    model.Add(inventoryBalanceCt);
                }
                // Here we fix the initial stock level
                Constraint initialInventoryBalanceCt = (initialStock[i] + buyVar[i, 0] - useVar[i, 0] - storeVar[i, 0] == 0.0);
                initialInventoryBalanceCt.Name = "InitialInventoryBalanceCt_" + rawMaterials[i] + "_" + months[0];
                model.Add(initialInventoryBalanceCt);
                // Here we fix the final stock level
                Constraint finalInventoryBalanceCt = (storeVar[i, (numberMonths - 1)] == finalStock[i]);
                finalInventoryBalanceCt.Name = "FinalInventoryBalanceCt_" + rawMaterials[i];
                model.Add(finalInventoryBalanceCt);
            }

            // Find the total costs of raw material and inventory storage
            // and the total value of all the products
            Expression totalRawMaterialCost = new Expression();
            Expression totalStorageCost = new Expression();
            Expression totalProductValue = new Expression();
            for (j = 0; j < numberMonths; j++)
            {
                totalProductValue.Add(productValue, productionVar[j]);
                for (i = 0; i < numberRawMaterials; i++)
                {
                    totalRawMaterialCost.Add(rawMaterialCost[i, j], buyVar[i, j]);
                    totalStorageCost.Add(storageCosts[i], storeVar[i, j]);
                }
            }

            Objective obj = totalProductValue - totalRawMaterialCost - totalStorageCost;
            model.Objective = obj;
            model.ObjectiveSense = ObjectiveSense.Maximise;
            model.Export("Food.sonnet");
            solver.Solve();

            objValue = model.Objective.Value;
            solutionString = solver.ToSolutionString();

            if (solver.IsProvenOptimal)
            {
                Console.WriteLine("----------------------------------------------------------------------------------------------");
                for (i = 0; i < numberRawMaterials; i++)
                    Console.Write("\t\t" + rawMaterials[i]);
                Console.Write("\n");
                Console.WriteLine("----------------------------------------------------------------------------------------------");
                // Now output the solution that we have found
                for (j = 0; j < numberMonths; j++)
                {
                    Console.Write(months[j] + "\tBuy ");
                    for (i = 0; i < numberRawMaterials; i++)
                        Console.Write("\t" + buyVar[i, j].Value.ToString("F3") + "\t");
                    Console.Write("\n");
                    Console.Write(months[j] + "\tUse ");
                    for (i = 0; i < numberRawMaterials; i++)
                        Console.Write("\t" + useVar[i, j].Value.ToString("F3") + "\t");
                    Console.Write("\n");
                    Console.Write(months[j] + "\tStore");
                    for (i = 0; i < numberRawMaterials; i++)
                        Console.Write("\t" + useVar[i, j].Value.ToString("F3") + "\t");
                    Console.Write("\n");
                    Console.WriteLine(months[j] + "\tProduct\t\t\t\t\t\t\t\t\t\t" + productionVar[j].Value.ToString("F3"));
                    Console.WriteLine("----------------------------------------------------------------------------------------------");
                }
            }
        }
    }

}
