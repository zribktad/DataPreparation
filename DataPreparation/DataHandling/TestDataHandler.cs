using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;
using DataPreparation.Runners;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    internal static class  TestDataHandler
    {
        internal static void DataUp(MethodInfo testMethodInfo)
        {

            TestAttributeCountStore.AddAttributeCount(testMethodInfo);
            if (!TestAttributeCountStore.AreAllTestAttributesUp(testMethodInfo)) return;

            var testData = TestDataPreparationStore.GetPreparedData(testMethodInfo);
            if (testData == null) return;
            
            Console.WriteLine($"Data for test {testMethodInfo.Name} starting up");
            RunnerTestData.Up(testData);
            Console.WriteLine($"Data for test {testMethodInfo.Name} are up");


        }

        internal static void DataDown(MethodInfo testMethodInfo)
        {

            TestAttributeCountStore.RemoveAttributeCount(testMethodInfo);
            if(!TestAttributeCountStore.AreAllTestAttributesDown(testMethodInfo)) return;

            var testData = TestDataPreparationStore.GetPreparedData(testMethodInfo);

            if (testData == null) return;
            
            Console.WriteLine($"Data for test {testMethodInfo.Name} starting down");
            RunnerTestData.Down(testData);
            Console.WriteLine($"Data for test {testMethodInfo.Name} are down");

        }

    }
}
